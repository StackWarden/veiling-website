namespace backend.Dtos;

public class CreateProductDto
{
    public Guid SpeciesId { get; set; }

    public string PotSize { get; set; } = string.Empty;
    public int StemLength { get; set; }
    public int Quantity { get; set; }
    public decimal StartPrice { get; set; }
    public decimal MinPrice { get; set; }
    public string? PhotoUrl { get; set; }
    public Guid? ClockLocationId { get; set; }
}
