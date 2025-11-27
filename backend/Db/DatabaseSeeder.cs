namespace backend.Db;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using backend.Db.Entities;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Create DB if not exists
        await db.Database.EnsureCreatedAsync();

        // ---- Seed Roles ----
        string[] roles = { "buyer", "supplier", "auctioneer", "admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.Roles.AnyAsync(r => r.Name == role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        // ---- Seed Admin User ----
        if (await userManager.FindByEmailAsync("admin@live.nl") == null)
        {
            var admin = new User
            {
                UserName = "admin@live.nl",
                Email = "admin@live.nl",
                Name = "System Administrator",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, "pepernoot");
            await userManager.AddToRoleAsync(admin, "admin");
        }

        // ---- Seed Supplier1 ----
        if (await userManager.FindByEmailAsync("supplier1@live.nl") == null)
        {
            var s1 = new User
            {
                UserName = "supplier1@live.nl",
                Email = "supplier1@live.nl",
                Name = "Supplier One",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(s1, "pepernoot");
            await userManager.AddToRoleAsync(s1, "supplier");
        }

        // ---- Seed Supplier2 ----
        if (await userManager.FindByEmailAsync("supplier2@live.nl") == null)
        {
            var s2 = new User
            {
                UserName = "supplier2@live.nl",
                Email = "supplier2@live.nl",
                Name = "Supplier Two",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(s2, "pepernoot");
            await userManager.AddToRoleAsync(s2, "supplier");
        }

        // ---- Seed Products ----
        if (!await db.Products.AnyAsync())
        {
            var supplier1 = await userManager.FindByEmailAsync("supplier1@live.nl");
            var supplier2 = await userManager.FindByEmailAsync("supplier2@live.nl");

            if (supplier1 == null || supplier2 == null)
                throw new Exception("Seeder failed: supplier users not found.");

            db.Products.AddRange(
                new Product
                {
                    Species = "Rosa Avalanche",
                    SupplierId = supplier1.Id,
                    PotSize = "12cm",
                    StemLength = 50,
                    Quantity = 200,
                    MinPrice = 0.15m,
                    ClockLocation = ClockLocation.Naaldwijk,
                    PhotoUrl = ""
                },
                new Product
                {
                    Species = "Tulipa Red Impression",
                    SupplierId = supplier2.Id,
                    PotSize = "10cm",
                    StemLength = 40,
                    Quantity = 300,
                    MinPrice = 0.10m,
                    ClockLocation = ClockLocation.Aalsmeer,
                    PhotoUrl = ""
                }
            );

            await db.SaveChangesAsync();
        }
    }
}