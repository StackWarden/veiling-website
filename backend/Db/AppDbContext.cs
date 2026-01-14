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
    public DbSet<Bid> Bids { get; set; }
    public DbSet<AuctionItem> AuctionItems { get; set; }
    public DbSet<Species> Species => Set<Species>();
    public DbSet<ClockLocation> ClockLocations => Set<ClockLocation>();

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
        ModelBuilder.Entity<Product>()
            .Property(p => p.MinPrice)
            .HasPrecision(18, 2);

        ModelBuilder.Entity<AuctionItem>()
            .Property(ai => ai.SoldPrice)
            .HasPrecision(18, 2);
        
        ModelBuilder.Entity<AuctionItem>()
            .HasOne(ai => ai.Buyer)
            .WithMany()
            .HasForeignKey(ai => ai.BuyerId)
            .OnDelete(DeleteBehavior.SetNull);

        ModelBuilder.Entity<Bid>()
            .Property(b => b.Price)
            .HasPrecision(18, 2);

        ModelBuilder.Entity<Bid>()
            .HasOne<AuctionItem>()
            .WithMany()
            .HasForeignKey(b => b.AuctionItemId)
            .OnDelete(DeleteBehavior.Cascade);
        
        ModelBuilder.Entity<Bid>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        ModelBuilder.Entity<Auction>()
            .HasOne(a => a.ClockLocation)
            .WithMany()
            .HasForeignKey(a => a.ClockLocationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
