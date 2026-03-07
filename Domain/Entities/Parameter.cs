namespace HajjSystem.Domain.Entities;

public class Parameter : BaseEntity
{
    public int    ParameterId { get; set; }
    public string Code        { get; set; } = string.Empty;
    public string DescArabic  { get; set; } = string.Empty;
    public string DescEnglish { get; set; } = string.Empty;
    public int    MaxValue    { get; set; }
    public int    MinValue    { get; set; }
    public int    Value       { get; set; }

    public ICollection<Pilgrim> Pilgrims { get; set; } = [];
    public ICollection<Flight>  Flights  { get; set; } = [];
}
