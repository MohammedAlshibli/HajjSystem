namespace HajjSystem.Domain.Entities;

public abstract class BaseEntity
{
    public DateTime  CreatedOn  { get; set; } = DateTime.Now;
    public string    CreatedBy  { get; set; } = string.Empty;
    public DateTime? UpdatedOn  { get; set; }
    public string?   UpdatedBy  { get; set; }
    public bool      IsDeleted  { get; set; } = false;
    public DateTime? DeletedOn  { get; set; }
    public string?   DeletedBy  { get; set; }
    public int       TenantId   { get; set; }
    public const int SysAdminTenantId = 0;
}
