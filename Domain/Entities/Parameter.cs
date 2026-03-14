using HajjSystem.Domain.Enums;

namespace HajjSystem.Domain.Entities;

/// <summary>
/// Generic lookup table.
/// Id          — auto-increment PK (no business meaning)
/// Type        — discriminator enum (ClassType, FitCode, ConfirmCode, FlightDirection)
/// Value       — the integer used in business logic (maps to HajjConstants)
/// DescArabic  — display label in Arabic
/// DescEnglish — display label in English
/// </summary>
public class Parameter : BaseEntity
{
    public int           Id          { get; set; }   // PK — auto identity
    public ParameterType Type        { get; set; }   // discriminator
    public int           Value       { get; set; }   // business value (matches HajjConstants)
    public string        DescArabic  { get; set; } = string.Empty;
    public string        DescEnglish { get; set; } = string.Empty;
    public bool          IsActive    { get; set; } = true;

    // Navigation
    public ICollection<Pilgrim> Pilgrims { get; set; } = [];
    public ICollection<Flight>  Flights  { get; set; } = [];
}
