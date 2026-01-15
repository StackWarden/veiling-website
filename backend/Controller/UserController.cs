using Microsoft.AspNetCore.Mvc;
using backend.Db.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using backend.Db;

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
    private readonly AppDbContext _db;

    public UserController(UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    // GET: /users
    // Haalt een lijst met gebruikers op — zonder wachtwoorden, want we zijn niet gek.
    // Gebruikt dependency injection om de UserManager binnen te halen, 
    // ook al hebben we 'm eigenlijk al als class property (want waarom niet).
    [HttpGet("")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.CreatedAt
            })
            .ToListAsync();

        // Geen ingewikkelde DTO's of filters, gewoon de basics.
        return Ok(users);
    }

    // GET: /users/{id}
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "";

        return Ok(new
        {
            user.Id,
            Username = user.Name,
            Role = role
        });
    }

    public class UpdateUserRoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }

    // PUT: /users/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest body)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var role = (body.Role ?? "").Trim().ToLowerInvariant();

        // Alleen deze rollen toegestaan:
        if (role != "auctioneer" && role != "supplier" && role != "buyer" && role != "admin")
            return BadRequest("Invalid role.");

        // roles verwijderen
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);
        }

        // nieuwe role
        var addResult = await _userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded) return BadRequest(addResult.Errors);

        return Ok(new
        {
            user.Id,
            Role = role
        });
    }

    // DELETE: /users/delete/{id}
    // products van user ook weg.
    [HttpDelete("delete/{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        // producten verwijderen met SupplierId == userId
        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.SupplierId == id)
            .ToListAsync();


        if (products.Count > 0)
        {
            _db.Products.RemoveRange(products);
            await _db.SaveChangesAsync();
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { Id = id });
    }
}
