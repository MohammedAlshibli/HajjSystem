namespace HajjSystem.Domain.Entities;

public class Flight : BaseEntity
{
    public int      FlightId       { get; set; }
    public string   FlightNo       { get; set; } = string.Empty;
    public DateTime FlightDate     { get; set; }
    public DateTime ArriveDate     { get; set; }
    public int      FlightYear     { get; set; }
    public int      FlightCapacity { get; set; }
    public string?  Direction      { get; set; }
    public int      ParameterId    { get; set; }  // 34=dep 35=return

    public Parameter?             Parameter  { get; set; }
    public ICollection<Bus>       Buses      { get; set; } = [];
    public ICollection<Passenger> Passengers { get; set; } = [];
}
