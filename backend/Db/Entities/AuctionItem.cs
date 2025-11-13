namespace backend.Db.Entities;

public class AuctionItem
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid ProductId { get; set; }
    public Auction Auction { get; set; }
    public Product Product { get; set; }
    public int LotNumber { get; set; }
    public String Status { get; set; } 
}