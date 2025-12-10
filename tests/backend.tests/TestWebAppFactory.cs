namespace backend.tests;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using backend.Db; // required to add to AppDBContext
using backend.Db.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;

public sealed class TestFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "Tests_" + Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove original DB context options (generic)
            var optionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );
            if (optionsDescriptor != null)
                services.Remove(optionsDescriptor);

            // Remove original DB context options (non-generic)
            var optionsNonGenericDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions)
            );
            if (optionsNonGenericDescriptor != null)
                services.Remove(optionsNonGenericDescriptor);

            // Remove original DB context
            var contextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AppDbContext)
            );
            if (contextDescriptor != null)
                services.Remove(contextDescriptor);

            // Add InMemory DB
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Add a lightweight test auth scheme that always succeeds with admin/auctioneer/buyer roles.
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "TestOrJwt";
                options.DefaultAuthenticateScheme = "TestOrJwt";
                options.DefaultChallengeScheme = "TestOrJwt";
            })
            .AddPolicyScheme("TestOrJwt", "Test or JWT", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) &&
                        authHeader.StartsWith("TestAuth", StringComparison.OrdinalIgnoreCase))
                    {
                        return "TestAuth";
                    }

                    return JwtBearerDefaults.AuthenticationScheme;
                };
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                "TestAuth", _ => { });
        });

        builder.ConfigureTestServices(services =>
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var scopedServices = scope.ServiceProvider;

            var db = scopedServices.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            string[] roles = { "buyer", "supplier", "auctioneer", "admin" };
            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(new IdentityRole<Guid>(role)).GetAwaiter().GetResult();
                }
            }
        });
    }
}
