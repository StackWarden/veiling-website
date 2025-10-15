namespace backend.Db.Entities;

public class Auction
{
    public Guid Id { get; set; }
    public Guid AuctionneerId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public required string Status { get; set; }

    public List<AuctionItem> AuctionItems { get; set; } = new();
}
