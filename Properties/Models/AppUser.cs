namespace UrenRegistratie.Models;

public class AppUser
{
    public string Uid { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
