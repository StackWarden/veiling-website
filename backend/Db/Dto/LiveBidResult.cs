namespace backend.Dtos
{
    // Dit DTO is het resultaat van het plaatsen van een bod tijdens een live veiling.
    // Het geeft aan of het bod geaccepteerd is (Accepted), de prijs waartegen het geaccepteerd is (AcceptedPrice),
    // de Id van het aangemaakte bod (BidId), of dit bod het laatste was voor dat item (Final),
    // en de nieuwe status van de veiling na het bod (State).
    public class LiveBidResultDto
    {
        public bool Accepted { get; set; }
        public decimal AcceptedPrice { get; set; }
        public Guid BidId { get; set; }
        public bool Final { get; set; }
        public LiveAuctionDto State { get; set; } = null!;
    }
}
