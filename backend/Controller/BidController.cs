using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore; 

namespace backend.Controllers;

// Deze controller behandelt alles wat met biedingen te maken heeft.
// Simpel gezegd: mensen doen een bod, wij slaan het op.
// Geen geavanceerde businesslogica of veilinglogica... nog niet tenminste.
[Route("bids")]
public class BidController : Controller
{
    private readonly AppDbContext _db; // Databasecontext om met de tabel Bids te praten.

    public BidController(AppDbContext db)
    {
        _db = db;
    }

    // POST: /bids/place
    // Plaatst een nieuw bod op een veiling.
    // Valideert de input heel minimaal, want blijkbaar vertrouwen we gebruikers nog steeds.
    // Zodra JWT is geïmplementeerd, kan de IgnoreAntiforgeryToken er eindelijk uit.
    [HttpPost("place")]
    [IgnoreAntiforgeryToken] // Dit moet weg zodra JWT is geimplementeerd
    public IActionResult PlaceBid([FromForm] PlaceBidDto dto)
    {
        // Validatie: we checken of alle verplichte velden zijn ingevuld.
        // Geen rocket science, gewoon simpele sanity-checks.
        if (string.IsNullOrWhiteSpace(dto.AuctionneerId) || string.IsNullOrWhiteSpace(dto.BuyerId) ||
            string.IsNullOrWhiteSpace(dto.IndividualPrice) || string.IsNullOrWhiteSpace(dto.Quantity))
            return BadRequest("All fields are required.");

        // Nieuwe bod-entity aanmaken en invullen met de ontvangen data.
        // CreatedAt wordt op UTC gezet zodat niemand ruzie krijgt over tijdzones.
        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            AuctionneerId = dto.AuctionneerId,
            BuyerId = dto.BuyerId,
            IndividualPrice = dto.IndividualPrice,
            Quantity = dto.Quantity,
            CreatedAt = DateTime.UtcNow
        };

        // Bod opslaan in de database, zonder transacties of ingewikkelde checks.
        _db.Bids.Add(bid);
        _db.SaveChanges();

        // Simpele success response met het ID van het bod. Lekker duidelijk.
        return Ok($"Bid placed successfully with ID: {bid.Id}");
    }
}

// DTO voor het plaatsen van een bod.
// Bevat de minimale informatie die nodig is om een bod te registreren.
// Alles als string, want waarom moeilijk doen met types zolang het werkt.
public class PlaceBidDto
{
    public string AuctionneerId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string IndividualPrice { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}
