namespace backend.Dtos
{
    // Dit DTO wordt gebruikt om een veiling inclusief de items terug te geven aan de client.
    // Het bevat de belangrijkste gegevens van de veiling: Id, Description, StartTime en EndTime,
    // en een lijst van items met hun Id, bijbehorende ProductId en status.
    // We laten bijvoorbeeld AuctionneerId en de veiling Status weg om de response simpel te houden.
    public class AuctionDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? ClockLocationId { get; set; }
        public string? ClockLocationName { get; set; }
        public List<AuctionItemDto> Items { get; set; } = new();
    }
}
