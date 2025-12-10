using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[Route("auctions")]
public class AuctionController : Controller
{
    private readonly AppDbContext _db;

    public AuctionController(AppDbContext db)
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

        if (dto.ProductIds == null || !dto.ProductIds.Any())
            return BadRequest("At least one product is required.");

        if (dto.EndTime <= dto.StartTime)
            return BadRequest("End time must be after start time.");

        using var transaction = _db.Database.BeginTransaction();

        try
        {
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
            foreach (var productId in dto.ProductIds)
            {
                if (!_db.Products.Any(p => p.Id == productId))
                {
                    transaction.Rollback(); // Geen items dan rollback dus dan gaat ook de auction er niet in
                    return BadRequest($"Product {productId} does not exist.");
                }

                var auctionItem = new AuctionItem
                {
                    Id = Guid.NewGuid(),
                    AuctionId = auction.Id,
                    ProductId = productId,
                    Status = "Pending"
                };

                _db.AuctionItems.Add(auctionItem);
            }

            _db.SaveChanges();
            transaction.Commit(); // Commit de transaction

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
        catch
        {
            transaction.Rollback(); // Als iets mis gaat rollback
            throw;
        }
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
