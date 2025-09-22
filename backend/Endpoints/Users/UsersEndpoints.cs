using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users");

        group.MapPost("/register", ([FromForm] RegisterDto dto) =>
        {
            // Form validation using pattern matching
            var result = (string.IsNullOrEmpty(dto.Name), string.IsNullOrEmpty(dto.Email)) switch
            {
                (true, true)  => Results.BadRequest("Name and Email are required."),
                (true, false) => Results.BadRequest("Name is required."),
                (false, true) => Results.BadRequest("Email is required."),
                _             => null
            };

            // If there's a validation error, return it
            if (result != null){
                return result;
            }

            // Proceed with registration logic
            return Results.Ok($"Proceeding with: {dto.Name}, {dto.Email}");
            
        }).DisableAntiforgery(); // Disable CSRF protection for this endpoint temporarily

        group.MapPost("/login", () =>
        {
            return Results.Ok("Hello World from Login");
        });
    }
}

// Data transfer object for registration
public class RegisterDto
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}