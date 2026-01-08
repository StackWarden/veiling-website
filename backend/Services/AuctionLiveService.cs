using backend.Db;
using backend.Db.Entities;
using backend.Dtos;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AuctionLiveService
    {
        private readonly AppDbContext _db;
        private readonly IAuctionLiveRuntime _live;

        public AuctionLiveService(AppDbContext db, IAuctionLiveRuntime live)
        {
            // Deze service regelt de live veilingen (starten, bieden, naar het volgende item, etc.).
            // Zo blijft de controller gevrijwaard van alle hectische live-logica.
            _db = db;
            _live = live;
        }

        // Start een live veiling. Zet het eerste item klaar en zet de veiling op "running".
        // Gooit een exceptie als de veiling niet bestaat (dan valt er weinig te starten),
        // of als er geen items zijn (een veiling zonder items starten heeft weinig om het lijf).
        public async Task<LiveAuctionDto> StartLive(Guid auctionId)
        {
            var auction = await _db.Auctions
                .Include(a => a.AuctionItems)
                .FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction == null)
            {
                throw new KeyNotFoundException("Auction not found.");
            }

            // Sorteer de items zodat Pending items eerst komen (die moeten geveild worden).
            var items = auction.AuctionItems
                .Where(ai => ai.Status == AuctionItemStatus.Pending || ai.Status == AuctionItemStatus.Live)
                .OrderBy(ai => ai.Status == AuctionItemStatus.Live ? 0 : 1)
                .ThenBy(ai => ai.Id)
                .ToList();

            if (items.Count == 0)
            {
                throw new ArgumentException("Auction has no items.");
            }

            // Zet alle andere veilingen die live zijn naar ended
            var otherLiveAuctions = await _db.Auctions
                .Where(a => a.Status == "Live" && a.Id != auctionId)
                .ToListAsync();

            foreach (var other in otherLiveAuctions)
            {
                other.Status = "Ended";
            }

            // Zet deze op Live
            auction.Status = "Live";
            await _db.SaveChangesAsync();

            var state = _live.GetOrCreate(auctionId);
            state.IsRunning = true;
            state.RoundIndex = 1;
            state.RoundStartedAtUtc = DateTime.UtcNow;

            // Start bij het eerste item (we gaan er vanuit dat die Pending is, anders is er iets geks gebeurd).
            state.CurrentAuctionItemId = items.First().Id;

            // Bouw en retourneer de huidige live veilingstatus.
            return await BuildLiveDto(auctionId, state);
        }

        // Haalt de live status van de veiling op. 
        // Als de veiling nog niet live is, geven we een status "stopped" terug met ronde 0.
        public async Task<LiveAuctionDto> GetLive(Guid auctionId)
        {
            if (!_live.TryGet(auctionId, out var state))
            {
                // Live nog niet gestart, dus we geven een 'stopped' status terug.
                return new LiveAuctionDto
                {
                    AuctionId = auctionId,
                    Status = "stopped",
                    ServerTimeUtc = DateTime.UtcNow,
                    RoundIndex = 0,
                    MaxRounds = 3
                };
            }

            // Als er al een state is, bouwen we de actuele status.
            return await BuildLiveDto(auctionId, state);
        }

        // Plaatst een live bod op het huidige item van de veiling.
        // Controleert eerst of de veiling wel bezig is en of er op het juiste item geboden wordt.
        // We zorgen er ook voor dat er maar één bod per ronde kan (het is tenslotte een veiling, geen chaos).
        // Als alles in orde is, slaan we het bod op en verwerken we het resultaat: 
        // bij de laatste ronde verkopen we het item en gaan we naar het volgende, anders verhogen we de ronde.
        public async Task<LiveBidResultDto> PlaceLiveBid(Guid auctionId, Guid buyerId, PlaceLiveBidDto dto)
        {
            // Veiling moet gestart en gaande zijn, anders heeft bieden geen zin.
            if (!_live.TryGet(auctionId, out var state) || !state.IsRunning)
            {
                throw new ArgumentException("Auction is not running.");
            }

            // Er moet een huidig item zijn om op te bieden.
            if (state.CurrentAuctionItemId is null)
            {
                throw new ArgumentException("No current auction item.");
            }

            // Check dat het bod gericht is op het juiste (huidige) item.
            if (dto.AuctionItemId != state.CurrentAuctionItemId.Value)
            {
                throw new InvalidOperationException("Bid is not for the current auction item.");
            }

            // Quantity moet positief zijn (we verkopen geen negatieve of nul stuks).
            if (dto.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be > 0.");
            }

            var now = DateTime.UtcNow;

            // Bepaal de huidige prijs op basis van de status vlak vóór het bod.
            var liveDtoBefore = await BuildLiveDto(auctionId, state);
            var acceptedPrice = liveDtoBefore.CurrentPrice;
            var currentItemId = state.CurrentAuctionItemId.Value;
            var roundStartedAt = state.RoundStartedAtUtc;

            // Start een database transactie om race conditions te voorkomen (één bod per ronde maximaal).
            await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            // Is er al een bod in deze ronde geplaatst? Zo ja, dan ben je te laat.
            var bidAlreadyPlacedThisRound = await _db.Bids.AnyAsync(b =>
                b.AuctionId == auctionId &&
                b.AuctionItemId == currentItemId &&
                b.CreatedAtUtc > roundStartedAt);

            if (bidAlreadyPlacedThisRound)
            {
                // Iemand anders was net iets sneller deze ronde.
                throw new InvalidOperationException("A bid was already placed this round.");
            }

            // Haal het huidige item op uit de database om de status te controleren.
            var item = await _db.AuctionItems.FirstOrDefaultAsync(ai => ai.Id == currentItemId);
            if (item == null)
            {
                // Dit zou niet moeten gebeuren tenzij het item buiten de veiling om is verwijderd.
                throw new KeyNotFoundException("Auction item not found.");
            }

            // Als het item al verkocht of gepasseerd is, kun je er niet meer op bieden.
            if (item.Status == AuctionItemStatus.Sold || item.Status == AuctionItemStatus.Passed)
            {
                throw new InvalidOperationException("Auction item is no longer live.");
            }

            // Maak het bod aan en sla het op.
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

            // Check of dit de laatste ronde is voor dit item.
            var isFinalBid = state.RoundIndex >= state.MaxRounds;

            if (isFinalBid)
            {
                // Laatste ronde: markeer item als verkocht en sla de verkoopdetails op.
                item.Status = AuctionItemStatus.Sold;
                item.BuyerId = buyerId;
                item.SoldPrice = acceptedPrice;
                item.SoldAtUtc = now;

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                // Naar het volgende item (de show must go on).
                await AdvanceInternal(auctionId, state);
            }
            else
            {
                // Niet laatste ronde: ga naar de volgende ronde voor ditzelfde item.
                if (item.Status == AuctionItemStatus.Pending)
                {
                    item.Status = AuctionItemStatus.Live;
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                // Verhoog de ronde en reset de starttijd voor de nieuwe ronde.
                state.RoundIndex += 1;
                state.RoundStartedAtUtc = now;
            }

            // Bouw de status na het bod (met eventueel het volgende item als we verkocht hebben).
            var liveDtoAfter = await BuildLiveDto(auctionId, state);

            return new LiveBidResultDto
            {
                Accepted = true,
                AcceptedPrice = acceptedPrice,
                BidId = bid.Id,
                Final = isFinalBid,
                State = liveDtoAfter
            };
        }

        // Gaat naar het volgende item in de live veiling (handmatig).
        // Als de veiling niet loopt, gooien we een fout (je kunt niet verder als er niks gaande is).
        public async Task<LiveAuctionDto> AdvanceLive(Guid auctionId)
        {
            if (!_live.TryGet(auctionId, out var state) || !state.IsRunning)
            {
                throw new ArgumentException("Auction is not running.");
            }

            // Ga intern naar het volgende item.
            await AdvanceInternal(auctionId, state);
            // Geef de nieuwe status terug (als er geen volgend item is, zal status "stopped" zijn).
            return await BuildLiveDto(auctionId, state);
        }

        // Interne helper om naar het volgende item te gaan en de state bij te werken.
        private async Task AdvanceInternal(Guid auctionId, AuctionLiveState state)
        {
            var auction = await _db.Auctions
                .Include(a => a.AuctionItems)
                    .ThenInclude(ai => ai.Product)
                        .ThenInclude(p => p.Species)
                .FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction == null)
            {
                // Als de veiling niet (meer) bestaat, doen we niets.
                return;
            }

            // Bepaal de lijst van items en kies het volgende item.
            var items = auction.AuctionItems
                .OrderBy(ai => ai.Status == AuctionItemStatus.Pending ? 0 : 1)
                .ThenBy(ai => ai.Id)
                .ToList();

            var nextId = PickNextItemId(items, state.CurrentAuctionItemId);
            state.CurrentAuctionItemId = nextId;
            state.RoundIndex = 1;
            state.RoundStartedAtUtc = DateTime.UtcNow;

            if (nextId is null)
            {
                // Geen volgend item meer: veiling is klaar.
                state.IsRunning = false;

                auction.Status = "Ended";
                await _db.SaveChangesAsync();
            }
        }

        // Berekent het ID van het volgende item op basis van de huidige lijst en het huidige item.
        private static Guid? PickNextItemId(List<AuctionItem> items, Guid? currentId)
        {
            if (items.Count == 0) return null;

            var remaining = items
                .Where(x => x.Status == AuctionItemStatus.Pending)
                .OrderBy(x => x.Id)
                .ToList();

            return remaining.FirstOrDefault()?.Id;
        }

        // Stelt een LiveAuctionDto samen met de actuele status van de veiling, inclusief het huidige item en de huidige prijs.
        // We berekenen de huidige prijs op basis van de verstreken tijd en de daling per seconde (een klassieke Dutch auction).
        private async Task<LiveAuctionDto> BuildLiveDto(Guid auctionId, AuctionLiveState state)
        {
            var now = DateTime.UtcNow;

            // Haal de veiling inclusief items en productgegevens op (zonder tracking, we gaan niets wijzigen).
            var auction = await _db.Auctions
                .AsNoTracking()
                .Include(a => a.AuctionItems)
                    .ThenInclude(ai => ai.Product)
                        .ThenInclude(p => p.Species)
                .FirstOrDefaultAsync(a => a.Id == auctionId);

            if (auction == null)
            {
                // Als de veiling niet (meer) bestaat, geven we de huidige state terug zodat de frontend iets ziet.
                return new LiveAuctionDto
                {
                    AuctionId = auctionId,
                    Status = state.IsRunning ? "running" : "stopped",
                    ServerTimeUtc = now,
                    RoundIndex = state.RoundIndex,
                    MaxRounds = state.MaxRounds,
                    RoundStartedAtUtc = state.RoundStartedAtUtc
                };
            }

            // Sorteer items zodat we consistent door de lijst lopen.
            var items = auction.AuctionItems
                .OrderBy(ai => ai.Status == AuctionItemStatus.Pending ? 0 : 1)
                .ThenBy(ai => ai.Id)
                .ToList();

            // Bepaal het huidige item (object) en het volgende item-ID.
            AuctionItem? current = null;
            if (state.CurrentAuctionItemId.HasValue)
            {
                current = items.FirstOrDefault(x => x.Id == state.CurrentAuctionItemId.Value);
            }
            var nextId = (current == null) ? null : PickNextItemId(items, current.Id);

            // Bereken de huidige prijs: startprijs - (verstreken seconden * daling per seconde), niet lager dan de minimumprijs.
            var minPrice = current?.Product.MinPrice ?? 0m;
            var elapsedSeconds = (decimal)(now - state.RoundStartedAtUtc).TotalSeconds;
            var rawPrice = state.StartingPrice - (elapsedSeconds * state.DecrementPerSecond);
            var currentPrice = Math.Max(minPrice, rawPrice);

            // Stel het LiveAuctionDto object samen met alle relevante informatie.
            return new LiveAuctionDto
            {
                AuctionId = auctionId,
                Status = state.IsRunning ? "running" : "stopped",
                ServerTimeUtc = now,

                RoundIndex = state.RoundIndex,
                MaxRounds = state.MaxRounds,
                RoundStartedAtUtc = state.RoundStartedAtUtc,

                StartingPrice = state.StartingPrice,
                MinPrice = minPrice,
                DecrementPerSecond = state.DecrementPerSecond,
                CurrentPrice = currentPrice,

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
