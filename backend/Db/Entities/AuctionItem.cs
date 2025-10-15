namespace backend.Db.Entities;

public class AuctionItem
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Auction Auction { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int LotNumber { get; set; }
    public string Status { get; set; } = string.Empty; // queued | active | sold | unsold
}