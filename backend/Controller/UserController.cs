using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore; 

namespace backend.Controllers;

[Route("users")]
public class UserController : Controller
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db)
    {
        _db = db;
    }
    // POST: /users/register
    [HttpPost("register")]
    [IgnoreAntiforgeryToken] // Dit moet weg zodra JWT is geimplementeerd
    public IActionResult Register([FromForm] RegisterDto dto)
    {
        // Validatie
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Name and Email are required.");

        // Check of e-mailadres al bestaat
        if (_db.Users.Any(u => u.Email == dto.Email))
            return Conflict("A user with this email already exists.");

        // Nieuwe gebruiker aanmaken
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "user",
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        return Ok($"User {user.Name} registered successfully.");
    }

    // POST: /users/login
    [HttpPost("login")]
    [IgnoreAntiforgeryToken]
    public IActionResult Login([FromForm] LoginDto dto)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        // Password check
        var validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!validPassword)
            return Unauthorized("Invalid email or password.");

        return Ok($"Welcome back, {user.Name}!");
    }

    // GET: /users
    [HttpGet("")]
    public IActionResult Index()
    {
        var users = _db.Users
            .AsNoTracking()
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .ToList();

        return Ok(users);
    }
}

public class RegisterDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
