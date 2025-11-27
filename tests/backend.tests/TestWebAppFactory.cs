namespace backend.tests;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using backend.Db; // required to add to AppDBContext

using Microsoft.AspNetCore.TestHost;

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
        });
    }
}
