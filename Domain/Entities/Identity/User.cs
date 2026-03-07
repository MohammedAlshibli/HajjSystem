namespace HajjSystem.Domain.Entities.Identity;

public class User
{
    public int       UserId        { get; set; }
    public string    UserName      { get; set; } = string.Empty;
    public string    FullName      { get; set; } = string.Empty;
    public string?   Rank          { get; set; }
    public bool      Active        { get; set; }
    public bool      IsSysAdmin    { get; set; }
    public int       TenantId      { get; set; }
    public int?      MainUnitId    { get; set; }
    public DateTime  CreatedOn     { get; set; }
    public string    CreatedBy     { get; set; } = string.Empty;
    public DateTime? ModifiedOn    { get; set; }
    public string?   ModifiedBy    { get; set; }
    public DateTime? LastLoginDate { get; set; }

    public HajjSystem.Domain.Entities.Unit? Unit { get; set; }
    public ICollection<UserRole>    UserRoles    { get; set; } = [];
    public ICollection<UserService> UserServices { get; set; } = [];
}
