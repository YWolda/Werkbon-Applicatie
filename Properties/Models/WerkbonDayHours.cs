namespace UrenRegistratie.Models;

/// <summary>
/// De (mogelijk handmatig aangepaste) uren voor één dag op de werkbon. Wordt
/// voorgevuld vanuit de automatische berekening, maar blijft aanpasbaar -
/// voor het geval er iets niet klopt.
/// </summary>
public class WerkbonDayHours
{
    public DateOnly Date { get; set; }
    public double TravelToHours { get; set; }
    public double WorkHours { get; set; }
    public double PrepReworkHours { get; set; }
    public double OvertimeHours { get; set; }
    public double TravelBackHours { get; set; }
    public double Kilometers { get; set; }
}
