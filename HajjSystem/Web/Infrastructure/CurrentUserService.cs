using HajjSystem.Application.Common.Interfaces;
using HajjSystem.Application.Services.Interfaces;
using System.Security.Claims;

namespace HajjSystem.Web.Infrastructure;

/// <summary>
/// Resolves the logged-in user from ClaimsPrincipal.
/// IsSysAdmin and TenantId are stored as Claims at login time.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    private ClaimsPrincipal? User => _http.HttpContext?.User;

    public string UserName =>
        User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public int TenantId =>
        int.TryParse(User?.FindFirstValue("TenantId"), out var t) ? t : 0;

    public bool IsSysAdmin =>
        bool.TryParse(User?.FindFirstValue("IsSysAdmin"), out var b) && b;
}
