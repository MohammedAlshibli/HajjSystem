namespace HajjSystem.Domain.Entities;

public class Document : BaseEntity
{
    public int       DocumentId   { get; set; }
    public string    FileName     { get; set; } = string.Empty;
    public string    ContentType  { get; set; } = string.Empty;
    public string    Path         { get; set; } = string.Empty;
    public string    DocumentType { get; set; } = string.Empty;
    public int       Year         { get; set; }
    public DateTime? VisitDate    { get; set; }

    public ICollection<Pilgrim> Pilgrims { get; set; } = [];
}
