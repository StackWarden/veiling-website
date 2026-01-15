namespace backend.Dtos;

public class PriceHistoryDto
{
    public decimal? AvgSupplier { get; set; }
    public decimal? AvgOverall { get; set; }

    public List<PricePointDto> Last10Supplier { get; set; } = new();
    public List<PricePointDto> Last10Overall { get; set; } = new();
}