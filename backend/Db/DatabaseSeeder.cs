namespace backend.Db;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using backend.Db.Entities;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Apply pending migrations to ensure database schema is up to date
        // Handle case where database was created with EnsureCreated (no migration history)
        bool hasMigrationHistory = false;
        bool shouldRunMigrations = false;
        
        try
        {
            if (await db.Database.CanConnectAsync())
            {
                // First check if migration history table exists using raw SQL
                try
                {
                    var connection = db.Database.GetDbConnection();
                    await connection.OpenAsync();
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'";
                    var historyTableCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    hasMigrationHistory = historyTableCount > 0;
                    await connection.CloseAsync();
                    
                    if (hasMigrationHistory)
                    {
                        // Migration history exists - check for pending migrations
                        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
                        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
                        
                        shouldRunMigrations = pendingMigrations.Any();
                        
                        if (shouldRunMigrations)
                        {
                            Console.WriteLine($"Applying {pendingMigrations.Count()} pending migrations...");
                            await db.Database.MigrateAsync();
                        }
                        else
                        {
                            Console.WriteLine("All migrations already applied");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No migration history table found - database was created with EnsureCreated");
                    }
                }
                catch (Exception historyCheckEx)
                {
                    // Error checking migration history - assume no history
                    hasMigrationHistory = false;
                    shouldRunMigrations = false;
                    Console.WriteLine($"Could not check migration history: {historyCheckEx.Message}");
                    Console.WriteLine("Assuming database was created with EnsureCreated");
                }
                
                try
                {
                    Console.WriteLine("Checking and fixing Auctions table schema...");
                    
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync(@"
                            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Auctions' AND COLUMN_NAME = 'StartTime')
                            BEGIN
                                ALTER TABLE [Auctions] DROP COLUMN [StartTime];
                            END
                        ");
                        Console.WriteLine("StartTime column checked/removed");
                    }
                    catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 4924 || sqlEx.Number == 2705)
                    {
                        // Column doesn't exist or already dropped - that's fine
                        Console.WriteLine("StartTime column doesn't exist (already removed)");
                    }
                    
                    // Drop old EndTime column if it exists
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync(@"
                            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Auctions' AND COLUMN_NAME = 'EndTime')
                            BEGIN
                                ALTER TABLE [Auctions] DROP COLUMN [EndTime];
                            END
                        ");
                        Console.WriteLine("EndTime column checked/removed");
                    }
                    catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 4924 || sqlEx.Number == 2705)
                    {
                        Console.WriteLine("EndTime column doesn't exist (already removed)");
                    }
                    
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync(@"
                            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Auctions' AND COLUMN_NAME = 'AuctionDate')
                            BEGIN
                                ALTER TABLE [Auctions] ADD [AuctionDate] date NOT NULL DEFAULT '0001-01-01';
                            END
                        ");
                        Console.WriteLine("AuctionDate column checked/added");
                    }
                    catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2705 || sqlEx.Number == 1913 || sqlEx.Number == 2714)
                    {
                        Console.WriteLine("AuctionDate column already exists");
                    }
                    
                    try
                    {
                        await db.Database.ExecuteSqlRawAsync(@"
                            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Auctions' AND COLUMN_NAME = 'AuctionTime')
                            BEGIN
                                ALTER TABLE [Auctions] ADD [AuctionTime] time NULL;
                            END
                        ");
                        Console.WriteLine("AuctionTime column checked/added");
                    }
                    catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2705 || sqlEx.Number == 1913 || sqlEx.Number == 2714)
                    {
                        Console.WriteLine("AuctionTime column already exists");
                    }
                }
                catch (Exception colFixEx)
                {
                    Console.WriteLine($"Warning: Could not fix Auction columns: {colFixEx.Message}");
                }
                
                // If no migration history, manually create ClockLocations table if needed
                if (!hasMigrationHistory)
                {
                    Console.WriteLine("Manually creating ClockLocations table if needed...");
                    try
                    {
                        // Check if ClockLocations table exists
                        var tableExists = await db.Database.ExecuteSqlRawAsync(
                            "SELECT CASE WHEN EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClockLocations') THEN 1 ELSE 0 END"
                        );
                        
                        // Use a simpler approach - try to create, ignore if exists
                        try
                        {
                            await db.Database.ExecuteSqlRawAsync(@"
                                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClockLocations')
                                CREATE TABLE [ClockLocations] (
                                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                                    [Name] nvarchar(200) NOT NULL,
                                    [CreatedAt] datetime2 NOT NULL
                                );
                            ");
                            Console.WriteLine("ClockLocations table created");
                        }
                        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2714)
                        {
                            // Table already exists - that's fine
                            Console.WriteLine("ClockLocations table already exists");
                        }
                        
                        // Check and add ClockLocationId column to Auctions
                        try
                        {
                            await db.Database.ExecuteSqlRawAsync(@"
                                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Auctions' AND COLUMN_NAME = 'ClockLocationId')
                                BEGIN
                                    ALTER TABLE [Auctions] ADD [ClockLocationId] uniqueidentifier NULL;
                                    CREATE INDEX [IX_Auctions_ClockLocationId] ON [Auctions] ([ClockLocationId]);
                                    ALTER TABLE [Auctions] ADD CONSTRAINT [FK_Auctions_ClockLocations_ClockLocationId] 
                                        FOREIGN KEY ([ClockLocationId]) REFERENCES [ClockLocations] ([Id]) ON DELETE SET NULL;
                                END
                            ");
                            Console.WriteLine("ClockLocationId column added to Auctions table");
                        }
                        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2714 || sqlEx.Number == 1913)
                        {
                            // Column/index/constraint already exists - that's fine
                            Console.WriteLine("ClockLocationId column/index already exists");
                        }
                    }
                    catch (Exception createEx)
                    {
                        Console.WriteLine($"Warning: Could not create ClockLocations table manually: {createEx.Message}");
                        // Continue anyway - table might already exist
                    }
                }
            }
            else
            {
                // Database doesn't exist, create it with migrations
                Console.WriteLine("Database doesn't exist - creating with migrations");
                await db.Database.MigrateAsync();
            }
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 2714 || sqlEx.Number == 1750 || sqlEx.Number == 1913)
        {
            // Table/constraint/index already exists - skip migration
            Console.WriteLine($"Migration skipped - database objects already exist (SQL error {sqlEx.Number})");
        }
        catch (Exception ex)
        {
            // Log but continue - don't crash the app
            Console.WriteLine($"Migration warning (app will continue): {ex.GetType().Name} - {ex.Message}");
        }

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

        // ---- Seed Clock Locations ----
        if (!await db.ClockLocations.AnyAsync())
        {
            db.ClockLocations.AddRange(
                new ClockLocation
                {
                    Id = Guid.NewGuid(),
                    Name = "clock 1"
                },
                new ClockLocation
                {
                    Id = Guid.NewGuid(),
                    Name = "clock 2"
                },
                new ClockLocation
                {
                    Id = Guid.NewGuid(),
                    Name = "clock 3"
                }
            );

            await db.SaveChangesAsync();
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
