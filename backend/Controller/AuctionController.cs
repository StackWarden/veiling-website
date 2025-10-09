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
        var auctions = _db.Auctions.ToList();
        return Ok(auctions);
    }

    // GET: /auctions/{id}
    [HttpGet("{id}")]
    public IActionResult GetAuctionById(Guid id)
    {
        var auction = _db.Auctions.FirstOrDefault(a => a.Id == id);
        if (auction == null)
            return NotFound("Auction not found.");

        return Ok(auction);
    }

    // POST: /auctions
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult CreateAuction([FromBody] CreateAuctionDto dto)
    {
        if (dto.EndTime <= dto.StartTime)
            return BadRequest("End time must be after start time.");

        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            AuctionneerId = dto.AuctionneerId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = dto.Status ?? "draft"
        };

        _db.Auctions.Add(auction);
        _db.SaveChanges();

        return Ok($"Auction {auction.Id} created successfully.");
    }

    // PUT: /auctions/{id}
    [HttpPut("{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult UpdateAuction(Guid id, [FromBody] CreateAuctionDto dto)
    {
        var auction = _db.Auctions.Find(id);
        if (auction == null)
            return NotFound("Auction not found.");

        if (dto.EndTime <= dto.StartTime)
            return BadRequest("End time must be after start time.");

        auction.StartTime = dto.StartTime;
        auction.EndTime = dto.EndTime;
        auction.Status = dto.Status;
        auction.AuctionneerId = dto.AuctionneerId;

        _db.SaveChanges();

        return Ok($"Auction {auction.Id} updated successfully.");
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

        return Ok($"Auction {auction.Id} deleted successfully.");
    }

    public class CreateAuctionDto
    {
        public Guid AuctionneerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = "draft";
    }
}
