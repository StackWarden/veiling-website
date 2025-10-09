using Microsoft.EntityFrameworkCore;
using backend.Db.Entities;

namespace backend.Db;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Auction> Auctions { get; set; }
}