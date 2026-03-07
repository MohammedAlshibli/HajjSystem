namespace HajjSystem.Domain.Entities.Identity;

public class Permission
{
    public int     PermissionId   { get; set; }
    public string  ControllerName { get; set; } = string.Empty;
    public string  ActionName     { get; set; } = string.Empty;
    public string  ScreenNameAr   { get; set; } = string.Empty;
    public string? Icon           { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
