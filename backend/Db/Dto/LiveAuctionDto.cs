namespace backend.Db;

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
    public decimal DecrementPerSecond { get; set; }
    public decimal CurrentPrice { get; set; }

    public Guid? AuctionItemId { get; set; }
    public ProductLiveDto? Product { get; set; }

    public Guid? NextAuctionItemId { get; set; }
}