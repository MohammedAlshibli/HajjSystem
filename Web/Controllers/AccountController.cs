using HajjSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using HajjSystem.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HajjSystem.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService) => _userService = userService;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model, string? returnUrl)
    {
        var username = (model.UserName ?? "").Trim().ToUpper();
        var password = model.Password ?? "";

        // ── Hardcoded bypass — always works regardless of DB ─────────────
        if (username == "ADMIN" && (password == "Oman" || password == "oman"))
        {
            await SignInAsync("ADMIN", "مدير النظام", tenantId: 0, isSysAdmin: true);
            return LocalRedirect(returnUrl ?? "/");
        }

        // ── LDAP + DB for real users ──────────────────────────────────────
        try
        {
            using var ldap = new Novell.Directory.Ldap.LdapConnection();
            ldap.Connect("10.22.8.8", 389);
            ldap.Bind("ITS\\" + username, password);
        }
        catch
        {
            ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيح");
            return View(model);
        }

        var user = await _userService.GetByUserNameAsync(username);
        if (user is null)
        {
            ModelState.AddModelError("", "المستخدم غير مسجل في النظام");
            return View(model);
        }

        await SignInAsync(user.UserName, user.FullName, user.TenantId, user.IsSysAdmin);
        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private Task SignInAsync(string userName, string fullName, int tenantId, bool isSysAdmin)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new("TenantId",   tenantId.ToString()),
            new("IsSysAdmin", isSysAdmin.ToString()),
            new("FullName",   fullName)
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false });
    }
}
