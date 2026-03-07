namespace HajjSystem.Application.Common.Interfaces;

/// <summary>
/// Resolves the logged-in user's identity and tenant at runtime.
/// Implemented in Web layer via IHttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    string  UserName   { get; }
    int     TenantId   { get; }
    bool    IsSysAdmin { get; }
}
