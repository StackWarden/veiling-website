namespace backend.Db.Entities;

public enum ClockLocation
{
    Naaldwijk,
    Aalsmeer,
    Rijnsburg,
    Eelde
}

public class Product
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }        // toekomstige foreign key naar User (supplier)
    public string Species { get; set; } = string.Empty;
    public string PotSize { get; set; } = string.Empty;
    public int StemLength { get; set; }         
    public int Quantity { get; set; }
    public decimal MinPrice { get; set; }
    public ClockLocation ClockLocation { get; set; }
    public DateOnly? AuctionDate { get; set; }
    public string? PhotoUrl { get; set; }
    public ICollection<AuctionItem> AuctionItems { get; set; } = new List<AuctionItem>();
}
