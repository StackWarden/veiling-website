namespace backend.Dtos
{
    public class WonAuctionItemDto
    {
        public Guid AuctionItemId { get; set; }
        public Guid AuctionId { get; set; }
        public string AuctionDescription { get; set; } = string.Empty;
        public DateOnly AuctionDate { get; set; }
        public TimeOnly? AuctionTime { get; set; }
        public Guid ProductId { get; set; }
        public string ProductSpecies { get; set; } = string.Empty;
        public int SoldAmount { get; set; }
        public decimal SoldPrice { get; set; }
        public decimal PricePerUnit { get; set; }
        public DateTime? SoldAtUtc { get; set; }
    }
}