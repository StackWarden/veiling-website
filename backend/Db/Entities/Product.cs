namespace backend.Db.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public Guid SpeciesId { get; set; }
    public Species Species { get; set; } = null!;
    public string PotSize { get; set; } = string.Empty;
    public int StemLength { get; set; }         
    public int Quantity { get; set; }
    public decimal StartPrice { get; set; }
    public decimal MinPrice { get; set; }
    public string? PhotoUrl { get; set; }
    public Guid? ClockLocationId { get; set; }
    public ClockLocation? ClockLocation { get; set; }
    public ICollection<AuctionItem> AuctionItems { get; set; } = new List<AuctionItem>();
}
