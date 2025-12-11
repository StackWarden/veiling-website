using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Db.Entities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace backend.Controllers;

// Deze controller regelt alles wat te maken heeft met authenticatie.
// Login, registratie en JWT-token generatie: de poortwachter van je hele API.
// En ja, dit hoort technisch gezien in een aparte AuthService, maar voor nu doen we het gewoon hier.
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager; // Handige Identity-helper om users te vinden, aan te maken en te beheren.
    private readonly SignInManager<User> _signInManager; // Behandelt loginchecks en wachtwoordvalidatie.

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // POST: /auth/login
    // Controleert de ingevoerde email en wachtwoord via ASP.NET Identity.
    // Als de combinatie klopt, sturen we een vriendelijke welkomstboodschap terug.
    // Geen JWT, geen cookies, gewoon een ouderwets "hoi, je bent ingelogd".
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) {
            return Unauthorized("Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded) {
            return Unauthorized("Invalid email or password.");
        }

        return Ok($"Welcome back, {user.Name}!");
    }

    // POST: /auth/jwt
    // Dezelfde loginlogica als hierboven, maar deze keer krijg je wel een JWT-token.
    // Perfect voor API-clients of mobiele apps die geen sessies gebruiken.
    // Kortom: zelfde validatie, ander souvenir.
    [HttpPost("jwt")]
    public async Task<IActionResult> LoginJWT([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) {
            return Unauthorized("Invalid email or password.");
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded) {
            return Unauthorized("Invalid email or password.");
        }
        var token = GenerateJwtToken(user.Id.ToString());

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(30),
            Path = "/"
        });

        return Ok(new { message = "Login successful", token = token});
    }

    // POST: /auth/logout
    // Verwijdert de JWT-cookie door een lege, verlopen cookie te plaatsen.
    // Hierdoor wordt de user direct uitgelogd aan de clientkant.
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        Response.Cookies.Append("jwt", string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(-1),
            Path = "/"
        });

        return Ok(new { message = "Logout successful" });
    }
    [Authorize]
    [HttpGet("info")]
    public async Task<IActionResult> UserInfo()
    {
        // Extract user ID from JWT Identity puts it in ClaimTypes.NameIdentifier or "sub"
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (userId == null)
            return Unauthorized("Invalid token");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized("User not found");

        // Get the user's role (first assigned role)
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        return Ok(new
        {
            name = user.Name,
            role = role
        });
    }

    // POST: /auth/register
    // Maakt een nieuwe gebruiker aan op basis van de opgegeven data.
    // Controleert of de email nog niet bestaat, we zijn tenslotte geen duplicatenverzamelaars.
    // Daarna wordt de user aangemaakt, krijgt standaard de rol 'buyer' en is officieel onderdeel van het systeem.
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email)) {
            return BadRequest("Name and Email are required.");
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null) {
            return Conflict("A user with this email already exists.");
        }
        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            Name = dto.Name
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded) {
            return BadRequest(result.Errors);
        }

        // Default rol meegeven, omdat iedereen ergens moet beginnen.
        await _userManager.AddToRoleAsync(user, "buyer"); // Of "supplier" / "auctioneer" als je zin hebt.

        return Ok(new { message = $"User {user.Name} registered successfully." });
    }

    // Genereert een JWT-token met de user-ID als subject.
    // Gebruikt het geheime wachtwoord uit je .env (hopelijk niet hardcoded).
    // Resultaat: een versleutelde string waarmee de user kan doen alsof hij legitiem is.
    private string GenerateJwtToken(string userId)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");

        if (string.IsNullOrWhiteSpace(secret))
            throw new Exception("JWT secret is missing from configuration.");

        // Find user and roles
        User user = _userManager.Users.First(u => u.Id.ToString() == userId);
        var roles = _userManager.GetRolesAsync(user).Result;

        // Create claims
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("DOMAIN"),
            audience: Environment.GetEnvironmentVariable("DOMAIN"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// DTO voor registratie: de broodnodige gegevens om een nieuwe gebruiker aan te maken.
// Geen magie, geen extra velden, gewoon de essentials.
public class RegisterDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// DTO voor login, want blijkbaar wil niemand elke keer zijn hele user-object meesturen.
// Alleen email en wachtwoord zijn genoeg om toegang te krijgen tot de digitale wereld.
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
