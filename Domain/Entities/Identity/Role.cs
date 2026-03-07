namespace HajjSystem.Domain.Entities.Identity;

public class Role
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public DateTime CreatedOn   { get; set; }
    public string   CreatedBy   { get; set; } = string.Empty;

    public ICollection<UserRole>       UserRoles       { get; set; } = [];
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
