namespace HajjSystem.Domain.Entities;

public class Bus : BaseEntity
{
    public int      BusId       { get; set; }
    public string   BusNo       { get; set; } = string.Empty;
    public int      BusCapacity { get; set; }
    public int      Year        { get; set; }
    public DateTime Date        { get; set; }
    public int      FlightId    { get; set; }

    public Flight?              Flight     { get; set; }
    public ICollection<Passenger> Passengers { get; set; } = [];
}
