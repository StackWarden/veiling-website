namespace backend.Dtos
{
    /*
     * Dit DTO bevat de huidige status van een live veiling.
     * Het omvat algemene veilinginformatie (AuctionId, status 'running' of 'stopped', de server-tijd, huidige ronde & max rondes, en wanneer de huidige ronde begon),
     * de prijzen (StartingPrice, MinPrice, DecrementPerSecond en de CurrentPrice op het moment van opvragen),
     * het huidige item dat geveild wordt (AuctionItemId en bijbehorende Product informatie),
     * en het ID van het volgende veilingitem (NextAuctionItemId) als dat er is.
     */
    public class LiveAuctionDto
    {
        public Guid AuctionId { get; set; }
        public string Status { get; set; } = "stopped";
        public DateTime ServerTimeUtc { get; set; }

        public int RoundIndex { get; set; }
        public int MaxRounds { get; set; }
        public DateTime RoundStartedAtUtc { get; set; }

        public decimal StartingPrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal DecayPerSecond { get; set; }
        public decimal CurrentPrice { get; set; }

        public Guid? AuctionItemId { get; set; }
        public ProductLiveDto? Product { get; set; }
        public Guid? NextAuctionItemId { get; set; }
    }

}
