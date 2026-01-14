namespace backend.Dtos
{
    // Dit DTO wordt gebruikt om een bod te plaatsen tijdens een live veiling.
    // Het bevat het AuctionItemId (het ID van het item waarop je biedt - dit moet het item zijn dat aan de beurt is)
    // en de Quantity (het aantal stuks dat je wilt kopen).
    // De prijs wordt niet meegestuurd; die bepaalt het systeem op basis van de huidige veilingprijs.
    public class PlaceLiveBidDto
    {
        public Guid AuctionItemId { get; set; }
        public int Quantity { get; set; }
    }
}
