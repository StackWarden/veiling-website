namespace backend.Db.Entities;

public class Bid
{
    public Guid Id { get; set; }

    public Guid AuctionId { get; set; }
    public Guid AuctionItemId { get; set; }

    public Guid BuyerId { get; set; }

    // prijs per stuk op het moment van bieden
    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
