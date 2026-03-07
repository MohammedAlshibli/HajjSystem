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
        if (!ModelState.IsValid) return View(model);

        model.UserName = model.UserName?.Trim().ToUpper() ?? string.Empty;

        if (string.IsNullOrEmpty(model.UserName) ||
            model.UserName.Contains("'") || model.UserName.Contains("/") ||
            model.UserName.ToLower().Contains("select"))
        {
            ModelState.AddModelError("", "اسم المستخدم غير صالح");
            return View(model);
        }

        bool authenticated = false;

        // Local admin bypass (development / emergency)
        if (model.UserName == "ADMIN" && model.Password == "Oman")
        {
            authenticated = true;
        }
        else
        {
            // LDAP against Active Directory
            using var ldap = new LdapConnection();
            try
            {
                ldap.Connect("10.22.8.8", 389);
                ldap.Bind("ITS\\" + model.UserName, model.Password);
                authenticated = true;
            }
            catch (LdapException)
            {
                ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيح");
                return View(model);
            }
        }

        if (!authenticated) return View(model);

        var user = await _userService.GetByUserNameAsync(model.UserName);
        if (user is null)
        {
            ModelState.AddModelError("", "لم يتم تسجيل هذا المستخدم — يرجى التواصل مع المسؤول");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new("TenantId",   user.TenantId.ToString()),
            new("IsSysAdmin", user.IsSysAdmin.ToString()),
            new("FullName",   user.FullName)
        };

        var permissions = await _userService.GetPermissionsAsync(user.UserName);
        foreach (var p in permissions)
            claims.Add(new Claim("Permission", $"{p.ControllerName}.{p.ActionName}"));

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = false });

        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home")!);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}

public class LoginModel
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
