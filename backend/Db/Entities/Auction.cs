namespace backend.Db.Entities;

public class Auction
{
    public Guid Id { get; set; }
    public Guid AuctionneerId { get; set; } = new();
    public DateTime StartTime { get; set; } = new();
    public DateTime EndTime { get; set; } = new();
    public string Status { get; set; } = "Scheduled";
    public ICollection<AuctionItem> AuctionItems { get; set; } = new List<AuctionItem>();
}
