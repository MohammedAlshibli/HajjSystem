using HajjSystem.Application;
using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Services.Interfaces;
using HajjSystem.Infrastructure;
using HajjSystem.Infrastructure.Services;
using HajjSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(opts =>
        opts.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// ── Authentication ────────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        opts.LoginPath  = "/Account/Login";
        opts.LogoutPath = "/Account/Logout";
        opts.ExpireTimeSpan = TimeSpan.FromHours(8);
        opts.SlidingExpiration = true;
    });

// ── Http context (needed by CurrentUserService) ───────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Settings ──────────────────────────────────────────────────────────────
builder.Services.Configure<HajjSettings>(builder.Configuration.GetSection("HajjSettings"));

// ── Application + Infrastructure DI ──────────────────────────────────────
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// ── Web-layer services ────────────────────────────────────────────────────
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IWebHostEnvironmentAccessor, WebHostEnvironmentAccessor>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
