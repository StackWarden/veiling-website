using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using backend.Db.Entities;

namespace backend.Db;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<SaleResult> SaleResults { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<AuctionItem> AuctionItems { get; set; }
    public DbSet<Species> Species => Set<Species>();

    protected override void OnModelCreating(ModelBuilder ModelBuilder)
    {
        base.OnModelCreating(ModelBuilder);

        ModelBuilder.Entity<AuctionItem>()
            .HasOne(ai => ai.Auction)
            .WithMany(a => a.AuctionItems)
            .HasForeignKey(ai => ai.AuctionId);

        ModelBuilder.Entity<AuctionItem>()
            .HasOne(ai => ai.Product)
            .WithMany(p => p.AuctionItems)
            .HasForeignKey(ai => ai.ProductId);
    }
}
