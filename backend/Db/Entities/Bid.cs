namespace backend.Db.Entities;

public class Bid
{
    public Guid Id { get; set; }
    public string AuctionneerId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string IndividualPrice { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}