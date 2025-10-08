namespace backend.Db.Entities;

public class Auction
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime EndTime { get; set; }
    public Guid AuctionneerId { get; set; }
    public DateTime StartTime { get; set; }
    public string Status { get; set; }
    public User Seller { get; set; }
    public decimal StartingPrice { get; set; }
}
