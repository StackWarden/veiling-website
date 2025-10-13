namespace backend.Db.Entities;

public class AuctionItem
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid ProductId { get; set; }
    public int LotNumber { get; set; }
    public string Status { get; set; } = string.Empty; // queued | active | sold | unsold
}