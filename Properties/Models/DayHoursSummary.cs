namespace UrenRegistratie.Models;

/// <summary>
/// De berekende uren-verdeling voor één dag, voor gebruik op het weekrapport.
/// Wordt automatisch berekend uit de WorkSegments van die dag door TimeTrackingService.
/// </summary>
public class DayHoursSummary
{
    public DateOnly Date { get; set; }
    public TimeSpan TravelTo { get; set; } = TimeSpan.Zero;
    public TimeSpan Work { get; set; } = TimeSpan.Zero;
    public TimeSpan TravelBack { get; set; } = TimeSpan.Zero;
    public TimeSpan Overtime { get; set; } = TimeSpan.Zero;
    public TimeSpan Break { get; set; } = TimeSpan.Zero;

    public TimeSpan Total => TravelTo + Work + TravelBack + Overtime;
}

/// <summary>
/// Eén segment met daaraan gekoppeld hoeveel van de duur "normale tijd" was en
/// hoeveel er in overuren viel (zodra de 8-uursgrens van die dag gepasseerd is),
/// plus de categorie (reistijd heen/terug of werk).
/// </summary>
public class SegmentBreakdown
{
    public required WorkSegment Segment { get; set; }
    public required string Category { get; set; } // "TravelTo", "Work", of "TravelBack"
    public double NormalMinutes { get; set; }
    public double OvertimeMinutes { get; set; }
}
