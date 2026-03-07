using HajjSystem.Domain.Enums;

namespace HajjSystem.Domain.Entities;

/// <summary>Registered Hajj pilgrim (was AlhajjMaster).</summary>
public class Pilgrim : BaseEntity
{
    public int            PilgrimId         { get; set; }
    public string         ServiceNumber     { get; set; } = string.Empty;
    public EmployeeStatus EmployeeStatus    { get; set; }
    public string         NIC               { get; set; } = string.Empty;
    public DateTime?      NICExpire         { get; set; }
    public string         Passport          { get; set; } = string.Empty;
    public DateTime?      PassportExpire    { get; set; }
    public string         FullName          { get; set; } = string.Empty;
    public DateTime?      DateOfBirth       { get; set; }
    public int            RankCode          { get; set; }
    public string         RankDesc          { get; set; } = string.Empty;
    public DateTime?      RegistrationDate  { get; set; }
    public int            HrmsUnitCode      { get; set; }
    public string         HrmsUnitDesc      { get; set; } = string.Empty;
    public string?        WorkLocation      { get; set; }
    public string?        Region            { get; set; }
    public int?           WilayaCode        { get; set; }
    public string?        WilayaDesc        { get; set; }
    public int?           VillageCode       { get; set; }
    public string?        VillageDesc       { get; set; }
    public string?        WorkPhone         { get; set; }
    public string?        GSM               { get; set; }
    public string?        RelativeName1     { get; set; }
    public string?        RelativeRelation1 { get; set; }
    public int?           RelativeGsm1      { get; set; }
    public string?        RelativeName2     { get; set; }
    public string?        RelativeRelation2 { get; set; }
    public int?           RelativeGsm2      { get; set; }
    public string?        SheikhName        { get; set; }
    public int?           SheikhGsm         { get; set; }
    public bool           FitFlag           { get; set; }
    public int            FitResult         { get; set; }
    public string?        DoctorNote        { get; set; }
    public string?        CancelNote        { get; set; }
    public string?        Notes             { get; set; }
    public int            HajjYear          { get; set; }
    public string?        BloodGroup        { get; set; }
    public DateTime?      DateOfEnlistment  { get; set; }
    public string?        PhotoPath         { get; set; }
    public int            ConfirmCode       { get; set; }
    public DateTime       InjectionDate     { get; set; }

    // Foreign keys
    public int?  UnitId     { get; set; }
    public int   TypeId     { get; set; }   // Parameter (ClassType)
    public int?  DocumentId { get; set; }

    // Navigation
    public Unit?       Unit      { get; set; }
    public Parameter?  Type      { get; set; }
    public Document?   Document  { get; set; }
    public ICollection<Passenger> Passengers { get; set; } = [];
}
