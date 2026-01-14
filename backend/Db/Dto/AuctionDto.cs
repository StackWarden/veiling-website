namespace backend.Dtos
{
    // DTO om een veiling inclusief items terug te geven aan de client.
    // Bevat: Id, Description, AuctionDate + optioneel AuctionTime, Status en items.
    public class AuctionDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateOnly AuctionDate { get; set; }
        public TimeOnly? AuctionTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<AuctionItemDto> Items { get; set; } = new();
    }
}
