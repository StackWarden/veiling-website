using DotNetEnv;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

var envFilePath = Path.Combine(builder.Environment.ContentRootPath, ".env");

Env.Load(envFilePath);

var isTesting = builder.Environment.IsEnvironment("Testing");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Veiling API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token.\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        if (!isTesting)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
            options.UseSqlServer(connectionString);
        }
    });

    builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("DOMAIN"),
        ValidAudience = Environment.GetEnvironmentVariable("DOMAIN"),
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? string.Empty)),

        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("jwt", out var token))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuctionLiveRuntime, AuctionLiveRuntime>();
builder.Services.AddScoped<AuctionService>();
builder.Services.AddScoped<AuctionLiveService>();

// Get allowed origins from environment or use defaults
var allowedOriginsEnv = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
var allowedOrigins = allowedOriginsEnv?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[]
    {
        "http://localhost:3000",
        "http://localhost:3001",
        "http://localhost:3002",
        "http://127.0.0.1:3000",
        "http://127.0.0.1:3001",
        "http://127.0.0.1:3002",
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // In development, allow any localhost origin for easier testing
            policy.SetIsOriginAllowed(origin =>
            {
                // #region agent log
                try { var logPath = "/app/debug.log"; System.IO.File.AppendAllText(logPath, System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "Program.cs:CORS-OriginCheck", message = "Checking origin", data = new { origin = origin ?? "null", allowedOrigins = allowedOrigins }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                
                if (string.IsNullOrEmpty(origin)) return false;
                
                // Check if it's in the explicitly allowed origins list
                if (allowedOrigins.Contains(origin))
                {
                    // #region agent log
                    try { var logPath = "/app/debug.log"; System.IO.File.AppendAllText(logPath, System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "Program.cs:CORS-OriginCheck", message = "Origin allowed (in list)", data = new { origin }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    return true;
                }
                
                try
                {
                    var uri = new Uri(origin);
                    var allowed = (uri.Scheme == "http" || uri.Scheme == "https") &&
                           (uri.Host == "localhost" || uri.Host == "127.0.0.1" || uri.Host == "::1");
                    
                    // #region agent log
                    try { var logPath = "/app/debug.log"; System.IO.File.AppendAllText(logPath, System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "Program.cs:CORS-OriginCheck", message = "Origin check result", data = new { origin, scheme = uri.Scheme, host = uri.Host, allowed }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    
                    return allowed;
                }
                catch (Exception ex)
                {
                    // #region agent log
                    try { var logPath = "/app/debug.log"; System.IO.File.AppendAllText(logPath, System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "Program.cs:CORS-OriginCheck", message = "Origin check exception", data = new { origin, error = ex.Message }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    return false;
                }
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        }
        else
        {
            // In production, use specific origins only
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Veiling API v1");
        options.RoutePrefix = "swagger";
    });
    await DatabaseSeeder.SeedAsync(app.Services);
}
else
{
    app.UseHttpsRedirection();
}

app.UseRouting();

// CORS must be before Authentication/Authorization to handle preflight requests
app.Use(async (context, next) =>
{
    // #region agent log
    try { var logPath = "/app/debug.log"; System.IO.File.AppendAllText(logPath, System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "Program.cs:CORS-Middleware", message = "Request received", data = new { method = context.Request.Method, path = context.Request.Path, origin = context.Request.Headers["Origin"].ToString(), hasOrigin = context.Request.Headers.ContainsKey("Origin") }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
    // #endregion
    await next();
});
app.UseCors("AllowNextJs");
app.Use(async (context, next) =>
{
    // #region agent log
    try { var logPath = "/app/debug.log"; System.IO.File.AppendAllText(logPath, System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "Program.cs:After-CORS", message = "After CORS middleware", data = new { method = context.Request.Method, path = context.Request.Path, corsHeaders = context.Response.Headers.ContainsKey("Access-Control-Allow-Origin") }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
    // #endregion
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Allow tests to boot up API
public partial class Program { }
