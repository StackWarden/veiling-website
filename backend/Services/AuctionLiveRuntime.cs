using System.Collections.Concurrent;

namespace backend.Services;

public class AuctionLiveState
{
    public Guid AuctionId { get; init; }

    public Guid? CurrentAuctionItemId { get; set; }

    public int RoundIndex { get; set; } = 1;     // 1..MaxRounds
    public int MaxRounds { get; set; } = 3;

    public DateTime RoundStartedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal StartingPrice { get; set; } = 300m;
    public decimal DecrementPerSecond { get; set; } = 1m;

    public bool IsRunning { get; set; } = false;
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
            StartingPrice = 300m,
            DecrementPerSecond = 1m,
            RoundStartedAtUtc = DateTime.UtcNow
        });
    }

    public bool TryGet(Guid auctionId, out AuctionLiveState state)
        => _states.TryGetValue(auctionId, out state!);

    public void Remove(Guid auctionId) => _states.TryRemove(auctionId, out _);
}
