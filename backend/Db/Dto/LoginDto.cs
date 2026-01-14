namespace backend.Dtos;

// DTO voor login, want blijkbaar wil niemand elke keer zijn hele user-object meesturen.
// Alleen email en wachtwoord zijn genoeg om toegang te krijgen tot de digitale wereld.
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}