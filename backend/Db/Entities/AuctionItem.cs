namespace backend.Db.Entities;

public class AuctionItem
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid ProductId { get; set; }
    public Auction Auction { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public int LotNumber { get; set; }
    public string Status { get; set; } = string.Empty;
}
