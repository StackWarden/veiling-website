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
    [Authorize(Roles = "admin")]
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

    [HttpPost("{id}/roles")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> SetUserRole(Guid id, [FromBody] string newRole)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound("User not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Contains(newRole))
            return BadRequest("User already has this role.");

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return BadRequest(removeResult.Errors.Select(e => e.Description));

        var addResult = await _userManager.AddToRoleAsync(user, newRole);
        if (!addResult.Succeeded)
            return BadRequest(addResult.Errors.Select(e => e.Description));

        return Ok($"User {user.Email} now has the role '{newRole}' (previous roles removed).");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound("User not found.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok($"User {user.Email} deleted.");
    }
}
