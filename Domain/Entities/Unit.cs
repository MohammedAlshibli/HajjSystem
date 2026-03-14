using HajjSystem.Domain.Entities.Identity;

namespace HajjSystem.Domain.Entities;

public class Unit : BaseEntity
{
    public int      Id          { get; set; }   // PK
    public int      UnitCode    { get; set; }
    public string   ArabicName  { get; set; } = string.Empty;
    public string   EnglishName  { get; set; } = string.Empty;
    public bool     ModFlag     { get; set; }
    public DateTime HajjYear    { get; set; }
    public int      AllowNumber { get; set; }
    public int      StandBy     { get; set; }
    public int      UnitOrder   { get; set; }

    public ICollection<Pilgrim>      Pilgrims     { get; set; } = [];
    public ICollection<UserService>  UserServices { get; set; } = [];
}
