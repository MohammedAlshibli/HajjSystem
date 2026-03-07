namespace HajjSystem.Domain.Entities;

public class Passenger : BaseEntity
{
    public int PassengerId { get; set; }
    public int HajjYear    { get; set; } = DateTime.Now.Year;
    public int FlightId    { get; set; }
    public int BusId       { get; set; }
    public int ResidenceId { get; set; }
    public int PilgrimId   { get; set; }

    public Flight?    Flight    { get; set; }
    public Bus?       Bus       { get; set; }
    public Residence? Residence { get; set; }
    public Pilgrim?   Pilgrim   { get; set; }
}
