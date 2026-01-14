namespace backend.Db.Entities;

public class Auction
{
    public Guid Id { get; set; }
    public string Description { get; set; } = "Default Description";
    public Guid AuctionneerId { get; set; } = new();
    public DateTime StartTime { get; set; } = new();
    public DateTime EndTime { get; set; } = new();
    public string Status { get; set; } = "Scheduled";
    public Guid? ClockLocationId { get; set; }
    public ClockLocation? ClockLocation { get; set; }
    public ICollection<AuctionItem> AuctionItems { get; set; } = new List<AuctionItem>();
}
