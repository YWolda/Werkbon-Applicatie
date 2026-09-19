namespace UrenRegistratie.Models;

/// <summary>
/// Een afgerond en (deels) ondertekend weekrapport voor één opdracht.
/// Wordt getoond in het Overzicht zodra hij is opgeslagen.
/// </summary>
public class WerkbonRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public int WeekNumber { get; set; }
    public int Year { get; set; }
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }

    public string EmployeeName { get; set; } = string.Empty;
    public string WorkDescription { get; set; } = string.Empty;
    public List<MaterialLine> Materials { get; set; } = new();
    public List<WerkbonDayHours> DayHours { get; set; } = new();

    public string? EmployeeSignatureDataUrl { get; set; }
    public string? ClientSignatureDataUrl { get; set; }
    public string ClientSignerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsSigned =>
        !string.IsNullOrEmpty(EmployeeSignatureDataUrl) && !string.IsNullOrEmpty(ClientSignatureDataUrl);
}
