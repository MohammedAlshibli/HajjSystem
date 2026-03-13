using HajjSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using HajjSystem.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Novell.Directory.Ldap;
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

        // ── ADMIN bypass — no DB, no LDAP needed ─────────────────────────
        if (username == "ADMIN" && password == "Oman")
        {
            await SignInAsync("ADMIN", "مدير النظام", 0, true, new List<Claim>());
            return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
        }

        // ── LDAP ──────────────────────────────────────────────────────────
        try
        {
            using var ldap = new LdapConnection();
            ldap.Connect("10.22.1.1", 389);
            ldap.Bind("test\\" + username, password);
        }
        catch (LdapException)
        {
            ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيح");
            return View(model);
        }

        // ── Load user from DB ─────────────────────────────────────────────
        var user = await _userService.GetByUserNameAsync(username);
        if (user is null)
        {
            // LDAP OK but no DB record — still allow in with basic access
            await SignInAsync(username, username, 0, false, new List<Claim>());
            return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
        }

        var permClaims = new List<Claim>();
        var permissions = await _userService.GetPermissionsAsync(user.UserName);
        foreach (var p in permissions)
            permClaims.Add(new Claim("Permission", $"{p.ControllerName}.{p.ActionName}"));

        await SignInAsync(user.UserName, user.FullName, user.TenantId, user.IsSysAdmin, permClaims);
        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    private Task SignInAsync(string userName, string fullName, int tenantId, bool isSysAdmin, List<Claim> extra)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new("TenantId",   tenantId.ToString()),
            new("IsSysAdmin", isSysAdmin.ToString()),
            new("FullName",   fullName)
        };
        claims.AddRange(extra);

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = false });
    }
}
