using System.Collections.Concurrent;

namespace backend.Services;

public class AuctionLiveState
{
    public Guid AuctionId { get; init; }

    public Guid? CurrentAuctionItemId { get; set; }

    // Round 2+ only happens because a bid was placed
    public int RoundIndex { get; set; } = 1;
    public int MaxRounds { get; set; } = 3;

    public DateTime RoundStartedAtUtc { get; set; } = DateTime.UtcNow;

    // Per item pricing
    public decimal StartingPrice { get; set; }
    public decimal MinPrice { get; set; } // dynamic min (Round 1 = product min, Round 2+ = last bid price, etc.)

    // Exponential percentage decay per second
    public decimal DecayPerSecond { get; set; } = 0.02m;

    public bool IsRunning { get; set; } = false;

    // Track last bid for current item (so we can auto-sell when clock hits min)
    public Guid? LastBidBuyerId { get; set; }
    public decimal? LastBidPrice { get; set; }
    public int? LastBidQuantity { get; set; }
    public DateTime? LastBidAtUtc { get; set; }

    public void ClearLastBid()
    {
        LastBidBuyerId = null;
        LastBidPrice = null;
        LastBidQuantity = null;
        LastBidAtUtc = null;
    }
}

public interface IAuctionLiveRuntime
{
    AuctionLiveState GetOrCreate(Guid auctionId);
    bool TryGet(Guid auctionId, out AuctionLiveState state);
    void Remove(Guid auctionId);
}

public class AuctionLiveRuntime : IAuctionLiveRuntime
{
    private readonly ConcurrentDictionary<Guid, AuctionLiveState> _states = new();

    public AuctionLiveState GetOrCreate(Guid auctionId)
    {
        return _states.GetOrAdd(auctionId, id => new AuctionLiveState
        {
            AuctionId = id,
            IsRunning = false,
            RoundIndex = 1,
            MaxRounds = 3,
            RoundStartedAtUtc = DateTime.UtcNow,
            StartingPrice = 0m,
            MinPrice = 0m,
            DecayPerSecond = 0.02m
        });
    }

    public bool TryGet(Guid auctionId, out AuctionLiveState state)
        => _states.TryGetValue(auctionId, out state!);

    public void Remove(Guid auctionId) => _states.TryRemove(auctionId, out _);
}
