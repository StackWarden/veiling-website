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

        await db.Database.EnsureCreatedAsync();

        // ---- Seed Roles ----
        string[] roles = { "buyer", "supplier", "auctioneer", "admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        // ---- Seed Admin ----
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

        // ---- Seed Supplier 1 ----
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

        // ---- Seed Supplier 2 ----
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

        // ---- Seed Species ----
        if (!await db.Species.AnyAsync())
        {
            db.Species.AddRange(
                new Species
                {
                    Id = Guid.NewGuid(),
                    Title = "Rosa Avalanche",
                    LatinName = "Rosa Avalanche",
                    Family = "Rosaceae",
                    GrowthType = "Snijbloem",
                    IsPerennial = true
                },
                new Species
                {
                    Id = Guid.NewGuid(),
                    Title = "Tulipa Red Impression",
                    LatinName = "Tulipa gesneriana",
                    Family = "Liliaceae",
                    GrowthType = "Bolplant",
                    IsPerennial = true
                }
            );

            await db.SaveChangesAsync();
        }

        // ---- Seed Products ----
        if (!await db.Products.AnyAsync())
        {
            var supplier1 = await userManager.FindByEmailAsync("supplier1@live.nl");
            var supplier2 = await userManager.FindByEmailAsync("supplier2@live.nl");

            if (supplier1 == null || supplier2 == null)
                throw new Exception("Seeder failed: suppliers not found.");

            var rosa = await db.Species.FirstAsync(s => s.Title == "Rosa Avalanche");
            var tulipa = await db.Species.FirstAsync(s => s.Title == "Tulipa Red Impression");

            db.Products.AddRange(
                new Product
                {
                    Id = Guid.NewGuid(),
                    SupplierId = supplier1.Id,
                    SpeciesId = rosa.Id,
                    PotSize = "12cm",
                    StemLength = 50,
                    Quantity = 200,
                    MinPrice = 0.15m,
                    PhotoUrl = null
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    SupplierId = supplier2.Id,
                    SpeciesId = tulipa.Id,
                    PotSize = "10cm",
                    StemLength = 40,
                    Quantity = 300,
                    MinPrice = 0.10m,
                    PhotoUrl = null
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
