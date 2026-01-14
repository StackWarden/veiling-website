namespace backend.Dtos
{
    /*
     * DTO om een nieuwe veiling (met items) aan te maken.
     * We gebruiken AuctionDate (verplicht) en AuctionTime (optioneel).
     */
    public class CreateAuctionWithItemsDto
    {
        public Guid AuctionneerId { get; set; }
        public DateOnly AuctionDate { get; set; }
        public TimeOnly? AuctionTime { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public List<Guid> ProductIds { get; set; } = new();
    }
}
