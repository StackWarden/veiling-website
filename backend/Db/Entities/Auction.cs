namespace backend.Db.Entities;

public class Auction
{
    public Guid Id { get; set; }
    public string Description { get; set; } = "Default Description";
    public Guid AuctionneerId { get; set; }
    public DateOnly AuctionDate { get; set; }
    public TimeOnly? AuctionTime { get; set; }
    public string Status { get; set; } = "Scheduled";
    public ICollection<AuctionItem> AuctionItems { get; set; } = new List<AuctionItem>();
}
