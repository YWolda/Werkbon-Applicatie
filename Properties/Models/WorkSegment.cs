namespace UrenRegistratie.Models;

public enum SegmentType
{
    Travel,
    Work,
    Break
}

/// <summary>
/// Eén aaneengesloten periode van "onderweg", "aan het werk" of "pauze". Een werkdag
/// bestaat uit één of meer segmenten, die je aanmaakt door in de app te wisselen
/// tussen de knoppen. Pauzetijd telt niet mee voor de 8-uursgrens of overuren.
/// </summary>
public class WorkSegment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public SegmentType Type { get; set; }
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public string? Note { get; set; }

    public bool IsRunning => End is null;

    public TimeSpan Duration => (End ?? DateTime.Now) - Start;
}
