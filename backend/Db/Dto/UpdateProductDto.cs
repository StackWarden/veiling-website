namespace backend.Dtos;
public class UpdateProductDto
{
    public Guid? SpeciesId { get; set; }
    public string? PotSize { get; set; }
    public int? StemLength { get; set; }
    public int? Quantity { get; set; }
    public decimal? MinPrice { get; set; }
    public string? PhotoUrl { get; set; }
    public Guid? ClockLocationId { get; set; }
}