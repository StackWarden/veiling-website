namespace backend.Dtos
{
    // Eenvoudig DTO voor AuctionItem (gebruikt binnen AuctionDto).
    // Bevat alleen de Id van het veilingitem, de ProductId (welk product het is) en de Status van het item (Pending/Live/Sold/Passed).
    // Dit is alles wat de frontend hoeft te weten over een item in de context van een veilinglijst.
    public class AuctionItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
