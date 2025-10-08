using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore; 

namespace backend.Controllers;

[Route("auctions")]
public class AuctionController : Controller
{
    private readonly AppDbContext _db;

    public AuctionController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /auctions
    [HttpGet]
    public IActionResult GetAllAuctions()
    {
        var auctions = _db.Auctions.Include(a => a.Seller).ToList();
        return Ok(auctions);
    }

    // GET: /auctions/{id}
    [HttpGet("{id}")]
    public IActionResult GetAuctionById(Guid id)
    {
        var auction = _db.Auctions.Include(a => a.Seller).FirstOrDefault(a => a.Id == id);
        if (auction == null)
            return NotFound("Auction not found.");

        return Ok(auction);
    }

    // POST: /auctions
    [HttpPost]
    [IgnoreAntiforgeryToken] // Dit moet weg zodra JWT is geimplementeerd
    public IActionResult CreateAuction([FromForm] CreateAuctionDto dto)
    {
        // Validatie
        if (string.IsNullOrWhiteSpace(dto.Title) || dto.StartingPrice <= 0 || dto.EndTime <= DateTime.UtcNow)
            return BadRequest("Invalid auction data.");

        var auctioneer = _db.Users.FirstOrDefault(u => u.Id == dto.AuctionneerId);
        if (auctioneer == null)
            return BadRequest("Auctioneer not found.");

        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            EndTime = dto.EndTime,
            AuctionneerId = auctioneer.Id,
            StartTime = DateTime.UtcNow,
            StartingPrice = dto.StartingPrice,
        };

        _db.Auctions.Add(auction);
        _db.SaveChanges();

        return Ok($"Auction {auction.Title} created successfully.");
    }

    // PUT: /auctions/{id}
    [HttpPut("{id}")]
    [IgnoreAntiforgeryToken] // Dit moet weg zodra JWT is geimplementeerd
    public IActionResult UpdateAuction(Guid id, [FromForm] CreateAuctionDto dto)
    {
        var auction = _db.Auctions.Find(id);
        if (auction == null)
            return NotFound("Auction not found.");

        // Validatie
        if (string.IsNullOrWhiteSpace(dto.Title) || dto.AuctionneerId == Guid.Empty || dto.EndTime <= DateTime.UtcNow)
            return BadRequest("Invalid auction data.");

        auction.Title = dto.Title;
        auction.Description = dto.Description;
        auction.StartTime = dto.StartTime;
        auction.EndTime = dto.EndTime;

        _db.SaveChanges();

        return Ok($"Auction {auction.Title} updated successfully.");
    }

    // DELETE: /auctions/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteAuction(Guid id)
    {
        var auction = _db.Auctions.Find(id);
        if (auction == null)
            return NotFound("Auction not found.");

        _db.Auctions.Remove(auction);
        _db.SaveChanges();

        return Ok($"Auction {auction.Title} deleted successfully.");
    }

    public class CreateAuctionDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public Guid AuctionneerId { get; set; }
        public DateTime StartTime { get; set; }

        public decimal StartingPrice { get; set; }
    }
}