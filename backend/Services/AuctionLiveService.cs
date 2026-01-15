using backend.Db;
using backend.Db.Entities;
using backend.Dtos;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AuctionLiveService
    {
        private readonly AppDbContext _db;
        private readonly IAuctionLiveRuntime _live;

        public AuctionLiveService(AppDbContext db, IAuctionLiveRuntime live)
        {
            _db = db;
            _live = live;
        }

        private static decimal ResolveStartPrice(Product product)
        {
            var start = product.StartPrice;

            if (start <= 0m)
                start = product.MinPrice * 1.5m;

            if (start < product.MinPrice)
                start = product.MinPrice;

            return start;
        }

        public async Task<LiveAuctionDto> StartLive(Guid auctionId)
        {
            var auction = await _db.Auctions
                .Include(a => a.AuctionItems)
                    .ThenInclude(ai => ai.Product)
                        .ThenInclude(p => p.Species)
                .FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction == null)
                throw new KeyNotFoundException("Auction not found.");

            if (!auction.ClockLocationId.HasValue)
                throw new ArgumentException("Auction cannot be started without a clock location.");

            var items = auction.AuctionItems
                .Where(ai => ai.Status == AuctionItemStatus.Pending || ai.Status == AuctionItemStatus.Live)
                .OrderBy(ai => ai.Status == AuctionItemStatus.Live ? 0 : 1)
                .ThenBy(ai => ai.Id)
                .ToList();

            if (items.Count == 0)
                throw new ArgumentException("Auction has no items.");

            var otherLiveAuctions = await _db.Auctions
                .Where(a => a.Status == "Live" && a.Id != auctionId)
                .ToListAsync();

            foreach (var other in otherLiveAuctions)
                other.Status = "Ended";

            auction.Status = "Live";
            await _db.SaveChangesAsync();

            var first = items.First();

            if (first.Status == AuctionItemStatus.Pending)
                first.Status = AuctionItemStatus.Live;

            await _db.SaveChangesAsync();

            var state = _live.GetOrCreate(auctionId);
            state.IsRunning = true;
            state.RoundIndex = 1;
            state.MaxRounds = 3;
            state.RoundStartedAtUtc = DateTime.UtcNow;

            state.CurrentAuctionItemId = first.Id;

            state.StartingPrice = ResolveStartPrice(first.Product);
            state.MinPrice = first.Product.MinPrice;

            state.DecayPerSecond = 0.02m;

            state.ClearLastBid();

            return await BuildLiveDto(auctionId, state);
        }

        public async Task<LiveAuctionDto> GetLive(Guid auctionId)
        {
            if (!_live.TryGet(auctionId, out var state))
            {
                return new LiveAuctionDto
                {
                    AuctionId = auctionId,
                    Status = "stopped",
                    ServerTimeUtc = DateTime.UtcNow,
                    RoundIndex = 0,
                    MaxRounds = 3
                };
            }

            await MaybeAutoFinalizeOrPass(auctionId, state);

            return await BuildLiveDto(auctionId, state);
        }

        public async Task<LiveBidResultDto> PlaceLiveBid(Guid auctionId, Guid buyerId, PlaceLiveBidDto dto)
        {
            if (!_live.TryGet(auctionId, out var state) || !state.IsRunning)
                throw new ArgumentException("Auction is not running.");

            await MaybeAutoFinalizeOrPass(auctionId, state);

            if (!state.IsRunning || state.CurrentAuctionItemId is null)
                throw new ArgumentException("Auction is not running.");

            if (dto.AuctionItemId != state.CurrentAuctionItemId.Value)
                throw new InvalidOperationException("Bid is not for the current auction item.");

            if (dto.Quantity <= 0)
                throw new ArgumentException("Quantity must be > 0.");

            var now = DateTime.UtcNow;
            var currentItemId = state.CurrentAuctionItemId.Value;
            var roundStartedAt = state.RoundStartedAtUtc;

            var acceptedPrice = ComputeCurrentPrice(state, now);

            var isFinalRound = state.RoundIndex >= state.MaxRounds;

            await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var bidAlreadyPlacedThisRound = await _db.Bids.AnyAsync(b =>
                b.AuctionId == auctionId &&
                b.AuctionItemId == currentItemId &&
                b.CreatedAtUtc > roundStartedAt);

            if (bidAlreadyPlacedThisRound)
                throw new InvalidOperationException("A bid was already placed this round.");

            var item = await _db.AuctionItems
                .Include(ai => ai.Product)
                .FirstOrDefaultAsync(ai => ai.Id == currentItemId);

            if (item == null)
                throw new KeyNotFoundException("Auction item not found.");

            if (item.Status == AuctionItemStatus.Sold || item.Status == AuctionItemStatus.Passed)
                throw new InvalidOperationException("Auction item is no longer live.");

            var available = item.Product.Quantity;
            var acceptedQty = Math.Min(dto.Quantity, available);

            if (acceptedQty <= 0)
                throw new InvalidOperationException("No stock left for this product.");

            var bid = new Bid
            {
                Id = Guid.NewGuid(),
                AuctionId = auctionId,
                AuctionItemId = currentItemId,
                BuyerId = buyerId,
                Price = acceptedPrice,
                Quantity = acceptedQty,
                CreatedAtUtc = now
            };

            _db.Bids.Add(bid);

            if (isFinalRound)
            {
                item.Status = AuctionItemStatus.Sold;
                item.BuyerId = buyerId;
                item.SoldPrice = acceptedPrice;
                item.SoldAtUtc = now;
                item.SoldAmount = acceptedQty;

                item.Product.Quantity = Math.Max(0, item.Product.Quantity - acceptedQty);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                state.ClearLastBid();
                await AdvanceInternal(auctionId, state);

                var afterFinal = await BuildLiveDto(auctionId, state);

                return new LiveBidResultDto
                {
                    Accepted = true,
                    AcceptedPrice = acceptedPrice,
                    BidId = bid.Id,
                    Final = true,
                    State = afterFinal
                };
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            state.LastBidBuyerId = buyerId;
            state.LastBidPrice = acceptedPrice;
            state.LastBidQuantity = acceptedQty;
            state.LastBidAtUtc = now;

            state.RoundIndex += 1;
            state.RoundStartedAtUtc = now;

            if (acceptedPrice > state.MinPrice)
                state.MinPrice = acceptedPrice;

            var liveDtoAfter = await BuildLiveDto(auctionId, state);

            return new LiveBidResultDto
            {
                Accepted = true,
                AcceptedPrice = acceptedPrice,
                BidId = bid.Id,
                Final = false,
                State = liveDtoAfter
            };
        }

        public async Task<LiveAuctionDto> AdvanceLive(Guid auctionId)
        {
            if (!_live.TryGet(auctionId, out var state) || !state.IsRunning)
                throw new ArgumentException("Auction is not running.");

            await AdvanceInternal(auctionId, state);
            return await BuildLiveDto(auctionId, state);
        }

        private async Task MaybeAutoFinalizeOrPass(Guid auctionId, AuctionLiveState state)
        {
            if (!state.IsRunning || state.CurrentAuctionItemId is null)
                return;

            var now = DateTime.UtcNow;
            var currentPrice = ComputeCurrentPrice(state, now);

            if (currentPrice > state.MinPrice)
                return;

            var itemId = state.CurrentAuctionItemId.Value;

            var item = await _db.AuctionItems
                .Include(ai => ai.Product)
                .FirstOrDefaultAsync(ai => ai.Id == itemId);

            if (item == null) return;

            if (state.LastBidBuyerId is null || state.LastBidPrice is null || state.LastBidQuantity is null)
            {
                if (state.RoundIndex != 1)
                    return;

                var anyBidEverForItem = await _db.Bids.AnyAsync(b =>
                    b.AuctionId == auctionId &&
                    b.AuctionItemId == itemId);

                if (anyBidEverForItem)
                    return;

                item.Status = AuctionItemStatus.Passed;
                await _db.SaveChangesAsync();

                await AdvanceInternal(auctionId, state);
                return;
            }

            var available = item.Product.Quantity;
            var soldQty = Math.Min(state.LastBidQuantity.Value, available);

            if (soldQty <= 0)
            {
                item.Status = AuctionItemStatus.Passed;
                await _db.SaveChangesAsync();
                await AdvanceInternal(auctionId, state);
                return;
            }

            item.Status = AuctionItemStatus.Sold;
            item.BuyerId = state.LastBidBuyerId.Value;
            item.SoldPrice = state.LastBidPrice.Value;
            item.SoldAtUtc = now;

            item.SoldAmount = soldQty;

            item.Product.Quantity = Math.Max(0, item.Product.Quantity - soldQty);

            await _db.SaveChangesAsync();

            await AdvanceInternal(auctionId, state);
        }

        private async Task AdvanceInternal(Guid auctionId, AuctionLiveState state)
        {
            var auction = await _db.Auctions
                .Include(a => a.AuctionItems)
                    .ThenInclude(ai => ai.Product)
                        .ThenInclude(p => p.Species)
                .FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction == null) return;

            var items = auction.AuctionItems
                .OrderBy(ai => ai.Status == AuctionItemStatus.Pending ? 0 : 1)
                .ThenBy(ai => ai.Id)
                .ToList();

            var nextId = PickNextItemId(items);

            state.CurrentAuctionItemId = nextId;
            state.RoundIndex = 1;
            state.RoundStartedAtUtc = DateTime.UtcNow;

            state.ClearLastBid();

            if (nextId is null)
            {
                state.IsRunning = false;
                auction.Status = "Ended";
                await _db.SaveChangesAsync();
                return;
            }

            var nextItem = items.First(x => x.Id == nextId.Value);

            if (nextItem.Status == AuctionItemStatus.Pending)
                nextItem.Status = AuctionItemStatus.Live;

            state.StartingPrice = ResolveStartPrice(nextItem.Product);
            state.MinPrice = nextItem.Product.MinPrice;

            await _db.SaveChangesAsync();
        }

        private static Guid? PickNextItemId(List<AuctionItem> items)
        {
            return items
                .Where(x => x.Status == AuctionItemStatus.Pending)
                .OrderBy(x => x.Id)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefault();
        }

        private static decimal ComputeCurrentPrice(AuctionLiveState state, DateTime nowUtc)
        {
            var elapsedSeconds = (nowUtc - state.RoundStartedAtUtc).TotalSeconds;
            if (elapsedSeconds < 0) elapsedSeconds = 0;

            var start = state.StartingPrice;
            var min = state.MinPrice;

            var k = (double)state.DecayPerSecond;
            var price = (decimal)((double)start * Math.Exp(-k * elapsedSeconds));

            return Math.Max(min, price);
        }

        private async Task<LiveAuctionDto> BuildLiveDto(Guid auctionId, AuctionLiveState state)
        {
            var now = DateTime.UtcNow;

            var auction = await _db.Auctions
                .AsNoTracking()
                .Include(a => a.AuctionItems)
                    .ThenInclude(ai => ai.Product)
                        .ThenInclude(p => p.Species)
                .FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction == null)
            {
                return new LiveAuctionDto
                {
                    AuctionId = auctionId,
                    Status = state.IsRunning ? "running" : "stopped",
                    ServerTimeUtc = now,
                    RoundIndex = state.RoundIndex,
                    MaxRounds = state.MaxRounds,
                    RoundStartedAtUtc = state.RoundStartedAtUtc,
                    StartingPrice = state.StartingPrice,
                    MinPrice = state.MinPrice,
                    DecayPerSecond = state.DecayPerSecond,
                    CurrentPrice = ComputeCurrentPrice(state, now),
                    AuctionItemId = state.CurrentAuctionItemId,
                    Product = null,
                    NextAuctionItemId = null
                };
            }

            var items = auction.AuctionItems
                .OrderBy(ai => ai.Status == AuctionItemStatus.Pending ? 0 : 1)
                .ThenBy(ai => ai.Id)
                .ToList();

            AuctionItem? current = null;
            if (state.CurrentAuctionItemId.HasValue)
                current = items.FirstOrDefault(x => x.Id == state.CurrentAuctionItemId.Value);

            var nextId = PickNextItemId(items);

            return new LiveAuctionDto
            {
                AuctionId = auctionId,
                Status = state.IsRunning ? "running" : "stopped",
                ServerTimeUtc = now,

                RoundIndex = state.RoundIndex,
                MaxRounds = state.MaxRounds,
                RoundStartedAtUtc = state.RoundStartedAtUtc,

                StartingPrice = state.StartingPrice,
                MinPrice = state.MinPrice,
                DecayPerSecond = state.DecayPerSecond,
                CurrentPrice = ComputeCurrentPrice(state, now),

                AuctionItemId = current?.Id,
                Product = current == null ? null : new ProductLiveDto
                {
                    Id = current.Product.Id,
                    Title = current.Product.Species?.Title ?? "Plant",
                    PhotoUrl = current.Product.PhotoUrl,
                    Species = current.Product.Species?.LatinName ?? "Plant",
                    StemLength = current.Product.StemLength,
                    Quantity = current.Product.Quantity,
                    MinPrice = current.Product.MinPrice,
                    PotSize = current.Product.PotSize
                },

                NextAuctionItemId = nextId
            };
        }
    }
}
