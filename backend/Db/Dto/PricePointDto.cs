namespace backend.Dtos;
public class PricePointDto
{
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public Guid? SupplierId { get; set; }
}