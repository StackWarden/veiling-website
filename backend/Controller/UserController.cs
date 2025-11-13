using Microsoft.AspNetCore.Mvc;
using backend.Db.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace backend.Controllers;

// Deze controller regelt alles rondom gebruikersbeheer.
// Alleen toegankelijk voor geauthenticeerde gebruikers via JWT of Identity cookies.
// Kortom: geen token, geen toegang.
[Route("users")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(AuthenticationSchemes = "Identity.Application")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;   // Voor alles wat met usermanagement te maken heeft.
    private readonly SignInManager<User> _signInManager; // Wordt hier nog niet gebruikt, maar ooit vast wel.

    public UserController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /users
    // Haalt een lijst met gebruikers op — zonder wachtwoorden, want we zijn niet gek.
    // Gebruikt dependency injection om de UserManager binnen te halen, 
    // ook al hebben we 'm eigenlijk al als class property (want waarom niet).
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

        // Geen ingewikkelde DTO's of filters, gewoon de basics.
        return Ok(users);
    }
}