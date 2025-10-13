using Microsoft.AspNetCore.Mvc;
using backend.Db.Entities;
using Microsoft.AspNetCore.Identity;

namespace backend.Controllers;

[Route("users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public UserController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // POST: /users/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Name and Email are required.");

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return Conflict("A user with this email already exists.");

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            Name = dto.Name
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // default role
        await _userManager.AddToRoleAsync(user, "buyer"); // or supplier / auctioneer

        return Ok($"User {user.Name} registered successfully.");
    }

    // POST: /users/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        return Ok($"Welcome back, {user.Name}!");
    }

    // GET: /users
    [HttpGet("")]
    public IActionResult Index([FromServices] UserManager<User> userManager)
    {
        var users = userManager.Users.Select(u => new
        {
            u.Id,
            u.Name,
            u.Email,
            u.CreatedAt
        });

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
