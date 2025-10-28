using Microsoft.AspNetCore.Mvc;
using backend.Db.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace backend.Controllers;

[Route("users")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize(AuthenticationSchemes = "Identity.Application")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public UserController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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
