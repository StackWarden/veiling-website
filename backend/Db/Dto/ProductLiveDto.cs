namespace backend.Db;

public class ProductLiveDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string Species { get; set; } = "";
    public int StemLength { get; set; }
    public int Quantity { get; set; }
    public decimal MinPrice { get; set; }
    public string PotSize { get; set; } = "";
}