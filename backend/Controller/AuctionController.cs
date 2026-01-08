using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Data;

namespace backend.Controllers;

[Route("auctions")]
public class AuctionController : Controller
{
    private readonly AppDbContext _db;
    private readonly Services.IAuctionLiveRuntime _live;

    public AuctionController(AppDbContext db, backend.Services.IAuctionLiveRuntime live)
    {
        /*
            Disclaimer: Deze comments zijn puur geplaatst omdat de docent vond dat er
            te weinig comments waren, ook al spreken de namen eigenlijk al voor zich...

            Met deze variabele kan je alles doen wat te maken heeft met database-interactie:
            queries uitvoeren, entiteiten toevoegen, verwijderen of updaten, en natuurlijk
            tabellen aanspreken zoals db.Auctions, db.Users, db.Bids, enzovoort.

            Eigenlijk zou het nog netter zijn geweest als dit specifiek db.Auctions heette,
            zodat het meteen duidelijk is dat deze controller zich enkel bezighoudt met
            veilingen dat had het Single Responsibility Principle (SRP) lekker schoon gehouden.
        */
        _db = db;
        _live = live;
    }

    // GET: /auctions
    // Haalt gewoon *alle* veilingen op uit de database.
    // Ja, echt allemaal zonder filters, zonder magie, gewoon _db.Auctions.ToList().
    // In theorie zou je hier nog pagination, filtering of caching kunnen toevoegen,
    // maar hé, laten we niet te ambitieus doen voor een simpele GET-endpoint.
    [HttpGet]
    [Authorize]
    public IActionResult GetAllAuctions()
    {
        var auctions = _db.Auctions.ToList();
        return Ok(auctions);
    }

    // GET: /auctions/{id}
    // Haalt één specifieke veiling op via het opgegeven ID.
    // Als dat ID niet bestaat, krijg je natuurlijk een nette 404 terug,
    // want blijkbaar kan je niet iets ophalen wat niet bestaat (wie had dat gedacht).
    // Anders gooien we het gewoon terug met een Ok(), helemaal volgens het boekje.
    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetAuctionById(Guid id)
    {
        var auction = _db.Auctions.FirstOrDefault(a => a.Id == id);
        if (auction == null) {
            return NotFound("Auction not found.");
        }
        return Ok(auction);
    }

    // POST: /auctions
    // Maakt een nieuwe veiling aan op basis van de meegegeven DTO.
    // Controleert eerst of de eindtijd niet voor de starttijd ligt,
    // want blijkbaar zijn tijdreizen nog steeds niet ondersteund in EF Core.
    // Daarna wordt een nieuwe Auction-entity aangemaakt, toegevoegd aan de database
    // en natuurlijk direct opgeslagen zonder ingewikkelde repositories of fancy patterns.
    // Uiteindelijk gooien we een Ok() terug met het ID, zodat iedereen blij is.
    [HttpPost]
    [Authorize(Roles = "auctioneer,admin")]
    public IActionResult CreateAuction([FromBody] CreateAuctionWithItemsDto dto)
    {
        // Valideer de DTO
        if (dto == null)
            return BadRequest("Request body is required.");

        if (dto.EndTime <= dto.StartTime)
            return BadRequest("End time must be after start time.");

        // Lege list als er geen items zijn meegestuurd, zodat we nog steeds een veiling kunnen maken
        var productIds = dto.ProductIds ?? new List<Guid>();

        // Maak de auction aan
        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            AuctionneerId = dto.AuctionneerId,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = dto.Status
        };

        _db.Auctions.Add(auction);
        _db.SaveChanges();

        // AuctionItems aanmaken
        foreach (var productId in productIds)
        {
            if (!_db.Products.Any(p => p.Id == productId))
            {
                return BadRequest($"Product {productId} does not exist.");
            }

            var auctionItem = new AuctionItem
            {
                Id = Guid.NewGuid(),
                AuctionId = auction.Id,
                ProductId = productId,
                Status = AuctionItemStatus.Pending
            };

            _db.AuctionItems.Add(auctionItem);
        }

        _db.SaveChanges();

        // Bouw response
        var response = new
        {
            auction.Id,
            auction.Description,
            auction.StartTime,
            auction.EndTime,
            Items = _db.AuctionItems
                .Where(ai => ai.AuctionId == auction.Id)
                .Select(ai => new
                {
                    ai.Id,
                    ai.ProductId,
                    ai.Status
                })
                .ToList()
        };

        return CreatedAtAction(nameof(GetAuctionById),
            new { id = auction.Id },
            response
        );
    }

    // PUT: /auctions/{id}
    // Werkt een bestaande veiling bij op basis van het opgegeven ID en de nieuwe data.
    // Eerst checken we natuurlijk of de veiling uberhaupt bestaat we zijn geen tovenaars.
    // Daarna controleren we of de eindtijd niet vóór de starttijd ligt,
    // want zelfs EF Core kan geen negatieve tijdspanne aan.
    // Vervolgens worden de velden netjes overschreven en alles opgeslagen met SaveChanges(),
    // oftewel: de standaard “ja dit hoort eigenlijk in een service-laag” aanpak.
    [HttpPut("{id}")]
    [Authorize(Roles = "auctioneer,admin")]
    public IActionResult UpdateAuction(Guid id, [FromBody] CreateAuctionWithItemsDto dto)
    {
        var auction = _db.Auctions.Find(id);
        if (auction == null) {
            return NotFound("Auction not found.");
        }
        if (dto.EndTime <= dto.StartTime) {
            return BadRequest("End time must be after start time.");
        }
        auction.StartTime = dto.StartTime;
        auction.EndTime = dto.EndTime;
        auction.Status = dto.Status;
        auction.AuctionneerId = dto.AuctionneerId;

        _db.SaveChanges();

        return Ok($"Auction {auction.Id} updated successfully.");
    }

    // DELETE: /auctions/{id}
    // Verwijdert een veiling op basis van het opgegeven ID.
    // Als de veiling niet bestaat, krijg je netjes een 404 geen magie, gewoon eerlijkheid.
    // Bestaat hij wel, dan gooien we ‘m uit de database en slaan dat op alsof er nooit iets gebeurd is.
    // Kortom: de digitale equivalent van “weg is weg”.
    [HttpDelete("{id}")]
    [Authorize(Roles = "auctioneer,admin")]
    public IActionResult DeleteAuction(Guid id)
    {
        var auction = _db.Auctions.Find(id); // Dit zoekt en geeft terug wat ie vind, als dat niks is geeft het dus ook null terug
        if (auction == null) {
            return NotFound("Auction not found."); // Dit doen we als het null is 404 status code returnen
        }
        _db.Auctions.Remove(auction);
        _db.SaveChanges();

        return Ok($"Auction {auction.Id} deleted successfully.");// 200 status code returnen en response
    }

    [HttpPost("{id:guid}/live/start")]
    [Authorize(Roles = "auctioneer,admin")]
    public async Task<IActionResult> StartLive(Guid id)
    {
        var auction = await _db.Auctions
            .Include(a => a.AuctionItems)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction is null) return NotFound("Auction not found.");

        // Selecteer items (stable)
        var items = auction.AuctionItems
            .OrderBy(ai => ai.Status == AuctionItemStatus.Pending ? 0 : 1)
            .ThenBy(ai => ai.Id)
            .ToList();
            
        if (items.Count == 0) return BadRequest("Auction has no items.");

        var state = _live.GetOrCreate(id);
        state.IsRunning = true;
        state.RoundIndex = 1;
        state.RoundStartedAtUtc = DateTime.UtcNow;

        // Start bij eerste pending
        state.CurrentAuctionItemId = items.First().Id;

        return Ok(await BuildLiveDto(id, state));
    }

    [HttpGet("{id:guid}/live")]
    [Authorize]
    public async Task<IActionResult> GetLive(Guid id)
    {
        if (!_live.TryGet(id, out var state))
        {
            // nog niet gestart
            return Ok(new LiveAuctionDto
            {
                AuctionId = id,
                Status = "stopped",
                ServerTimeUtc = DateTime.UtcNow,
                RoundIndex = 0,
                MaxRounds = 3
            });
        }

        return Ok(await BuildLiveDto(id, state));
    }

    [HttpPost("{id:guid}/live/bid")]
    [Authorize(Roles = "buyer,supplier,admin")]
    public async Task<IActionResult> PlaceLiveBid(Guid id, [FromBody] PlaceLiveBidDto dto)
    {
        if (!_live.TryGet(id, out var state) || !state.IsRunning)
            return BadRequest("Auction is not running.");

        if (state.CurrentAuctionItemId is null)
            return BadRequest("No current auction item.");

        if (dto.AuctionItemId != state.CurrentAuctionItemId.Value)
            return Conflict("Bid is not for the current auction item.");

        if (dto.Quantity <= 0)
            return BadRequest("Quantity must be > 0.");

        var buyerIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(buyerIdString, out var buyerId))
            return Unauthorized("Invalid user id");

        var now = DateTime.UtcNow;

        var liveDtoBefore = await BuildLiveDto(id, state);
        var acceptedPrice = liveDtoBefore.CurrentPrice;

        var currentItemId = state.CurrentAuctionItemId.Value;

        var roundStartedAt = state.RoundStartedAtUtc;

        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var bidAlreadyPlacedThisRound = await _db.Bids.AnyAsync(b =>
            b.AuctionId == id &&
            b.AuctionItemId == currentItemId &&
            b.CreatedAtUtc > roundStartedAt);

        if (bidAlreadyPlacedThisRound)
            return Conflict("A bid was already placed this round.");

        var item = await _db.AuctionItems.FirstOrDefaultAsync(ai => ai.Id == currentItemId);
        if (item is null)
            return NotFound("Auction item not found.");

        if (item.Status == AuctionItemStatus.Sold || item.Status == AuctionItemStatus.Passed)
            return Conflict("Auction item is no longer live.");

        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            AuctionId = id,
            AuctionItemId = currentItemId,
            BuyerId = buyerId,
            Price = acceptedPrice,
            Quantity = dto.Quantity,
            CreatedAtUtc = now
        };

        _db.Bids.Add(bid);

        var isFinalBid = state.RoundIndex >= state.MaxRounds;

        if (isFinalBid)
        {
            item.Status = AuctionItemStatus.Sold;
            item.BuyerId = buyerId;
            item.SoldPrice = acceptedPrice;
            item.SoldAtUtc = now;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            // Naar volgende item
            await AdvanceInternal(id, state);
        }
        else
        {
            if (item.Status == AuctionItemStatus.Pending)
                item.Status = AuctionItemStatus.Live;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            state.RoundIndex += 1;
            state.RoundStartedAtUtc = now;
        }

        var liveDtoAfter = await BuildLiveDto(id, state);

        return Ok(new
        {
            accepted = true,
            acceptedPrice,
            bidId = bid.Id,
            final = isFinalBid,
            state = liveDtoAfter
        });
    }



    [HttpPost("{id:guid}/live/advance")]
    [Authorize(Roles = "auctioneer,admin")]
    public async Task<IActionResult> AdvanceLive(Guid id)
    {
        if (!_live.TryGet(id, out var state) || !state.IsRunning)
            return BadRequest("Auction is not running.");

        await AdvanceInternal(id, state);
        return Ok(await BuildLiveDto(id, state));
    }

    private async Task AdvanceInternal(Guid auctionId, backend.Services.AuctionLiveState state)
    {
        var auction = await _db.Auctions
            .Include(a => a.AuctionItems)
                .ThenInclude(ai => ai.Product)
                    .ThenInclude(p => p.Species)
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction is null) return;

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
            state.IsRunning = false; // done
        }
    }

    private static Guid? PickNextItemId(List<AuctionItem> items, Guid? currentId)
    {
        if (items.Count == 0) return null;
        if (currentId is null) return items[0].Id;

        var idx = items.FindIndex(x => x.Id == currentId.Value);
        if (idx < 0) return items[0].Id;
        return (idx + 1 < items.Count) ? items[idx + 1].Id : (Guid?)null;
    }
    private async Task<LiveAuctionDto> BuildLiveDto(Guid auctionId, backend.Services.AuctionLiveState state)
    {
        var now = DateTime.UtcNow;

        var auction = await _db.Auctions
            .AsNoTracking()
            .Include(a => a.AuctionItems)
                .ThenInclude(ai => ai.Product)
                    .ThenInclude(p => p.Species)
            .FirstOrDefaultAsync(a => a.Id == auctionId);

        if (auction is null)
        {
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

        var items = auction.AuctionItems
            .OrderBy(ai => ai.Status == AuctionItemStatus.Pending ? 0 : 1)
            .ThenBy(ai => ai.Id)
            .ToList();

        AuctionItem? current = null;
        if (state.CurrentAuctionItemId is not null)
            current = items.FirstOrDefault(x => x.Id == state.CurrentAuctionItemId.Value);

        var nextId = (current is null) ? null : PickNextItemId(items, current.Id);

        var minPrice = current?.Product.MinPrice ?? 0m;

        var elapsedSeconds = (decimal)(now - state.RoundStartedAtUtc).TotalSeconds;
        var raw = state.StartingPrice - (elapsedSeconds * state.DecrementPerSecond);
        var currentPrice = Math.Max(minPrice, raw);

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
            Product = current is null ? null : new ProductLiveDto
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

    public class CreateAuctionWithItemsDto
    {
        public Guid AuctionneerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "draft";

        public List<Guid> ProductIds { get; set; } = new();
    }
}
