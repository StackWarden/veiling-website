namespace backend.Dtos;

// DTO voor registratie: de broodnodige gegevens om een nieuwe gebruiker aan te maken.
// Geen magie, geen extra velden, gewoon de essentials.
public class RegisterDto
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
}