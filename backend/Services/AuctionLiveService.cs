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

            // End other live auctions
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

            state.StartingPrice = first.Product.StartPrice;
            state.MinPrice = first.Product.MinPrice;

            state.DecayPerSecond = 0.02m;

            // IMPORTANT: new item => clear last bid
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

            // Auto rules
            await MaybeAutoFinalizeOrPass(auctionId, state);

            return await BuildLiveDto(auctionId, state);
        }

        public async Task<LiveBidResultDto> PlaceLiveBid(Guid auctionId, Guid buyerId, PlaceLiveBidDto dto)
        {
            if (!_live.TryGet(auctionId, out var state) || !state.IsRunning)
                throw new ArgumentException("Auction is not running.");

            // If the clock already hit min, finalize/pass first
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

            // authoritative accepted price right now
            var acceptedPrice = ComputeCurrentPrice(state, now);

            await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            // One bid per round
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

            var bid = new Bid
            {
                Id = Guid.NewGuid(),
                AuctionId = auctionId,
                AuctionItemId = currentItemId,
                BuyerId = buyerId,
                Price = acceptedPrice,
                Quantity = dto.Quantity,
                CreatedAtUtc = now
            };
            _db.Bids.Add(bid);

            // Persist bid first
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // Track last bid in memory (for auto-sell when min reached)
            state.LastBidBuyerId = buyerId;
            state.LastBidPrice = acceptedPrice;
            state.LastBidQuantity = dto.Quantity;
            state.LastBidAtUtc = now;

            // Round 2+ only because of bid
            state.RoundIndex += 1;
            state.RoundStartedAtUtc = now;

            // New dynamic minimum becomes the last bid price (can only go up)
            if (acceptedPrice > state.MinPrice)
                state.MinPrice = acceptedPrice;

            var liveDtoAfter = await BuildLiveDto(auctionId, state);

            return new LiveBidResultDto
            {
                Accepted = true,
                AcceptedPrice = acceptedPrice,
                BidId = bid.Id,
                Final = false, // final happens by auto-sell OR if you still use MaxRounds logic
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

        /// <summary>
        /// Core rule:
        /// - If price hits min and there was NO bid ever for this item and we're in round 1 => PASS
        /// - If price hits min and there IS a last bid => SOLD to last bidder
        /// </summary>
        private async Task MaybeAutoFinalizeOrPass(Guid auctionId, AuctionLiveState state)
        {
            if (!state.IsRunning || state.CurrentAuctionItemId is null)
                return;

            var now = DateTime.UtcNow;
            var currentPrice = ComputeCurrentPrice(state, now);

            if (currentPrice > state.MinPrice)
                return; // still above min

            var itemId = state.CurrentAuctionItemId.Value;

            // load item
            var item = await _db.AuctionItems
                .Include(ai => ai.Product)
                .FirstOrDefaultAsync(ai => ai.Id == itemId);

            if (item == null) return;

            // CASE 1: No last bid => only auto-pass in round 1
            if (state.LastBidBuyerId is null || state.LastBidPrice is null || state.LastBidQuantity is null)
            {
                if (state.RoundIndex != 1)
                {
                    // No bid but we're past round 1 shouldn't happen with your rules.
                    // Do nothing (or you could force pass).
                    return;
                }

                // Ensure truly no bids in DB as safety
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

            // CASE 2: There IS a last bid => auto-sell to last bidder
            item.Status = AuctionItemStatus.Sold;
            item.BuyerId = state.LastBidBuyerId.Value;
            item.SoldPrice = state.LastBidPrice.Value;      // this equals min at this moment (or higher)
            item.SoldAtUtc = now;

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

            // new item => clear last bid info
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

            // reset pricing from product
            state.StartingPrice = nextItem.Product.StartPrice;
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
