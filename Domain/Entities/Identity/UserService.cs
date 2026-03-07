namespace HajjSystem.Domain.Entities.Identity;

public class UserService
{
    public int UserId    { get; set; }
    public int ServiceId { get; set; }
    public User?                              User { get; set; }
    public HajjSystem.Domain.Entities.Unit?  Unit { get; set; }
}
