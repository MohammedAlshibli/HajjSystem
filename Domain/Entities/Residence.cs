namespace HajjSystem.Domain.Entities;

public class Residence : BaseEntity
{
    public int      ResidenceId  { get; set; }
    public string   Building     { get; set; } = string.Empty;
    public int      Room         { get; set; }
    public int      RoomCapacity { get; set; }
    public int      Floor        { get; set; }
    public DateTime Year         { get; set; }

    public ICollection<Passenger> Passengers { get; set; } = [];
}
