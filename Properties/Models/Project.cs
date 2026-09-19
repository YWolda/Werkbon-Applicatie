namespace UrenRegistratie.Models;

/// <summary>
/// Een opdracht/klus voor een klant of werkgever waarop uren geschreven kunnen worden.
/// Deze gegevens zijn "vast" per opdracht en vullen straks automatisch het weekrapport in.
/// </summary>
public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ProjectNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ReferenceClient { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
}
