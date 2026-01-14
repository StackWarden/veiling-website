namespace backend.Db.Entities;

public class AuctionItem
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid ProductId { get; set; }
    public Auction Auction { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public AuctionItemStatus Status { get; set; } = AuctionItemStatus.Pending;
    public Guid? BuyerId { get; set; }
    public User? Buyer { get; set; }

    public decimal? SoldPrice { get; set; }
    public DateTime? SoldAtUtc { get; set; }
}
public enum AuctionItemStatus
{
    Pending, // Nog in de wachtrij
    Live, // Nu aan de beurt
    Sold, // Verkocht uiteraard
    Passed // Niemand gekocht voor 3 rondes
}
