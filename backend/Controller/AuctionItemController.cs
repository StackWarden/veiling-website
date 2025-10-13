using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore; 

namespace backend.Controllers;

[Route("auction-items")]
public class AuctionItemController : Controller
{
    private readonly AppDbContext _db;

    public AuctionItemController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /auction-items
    [HttpGet]
    public IActionResult GetAllAuctionItems()
    {
        var auctionItems = _db.AuctionItems.Include(ai => ai.Auction).ToList();
        return Ok(auctionItems);
    }

    // GET: /auction-items/{id}
    [HttpGet("{id}")]
    public IActionResult GetAuctionItemById(Guid id)
    {
        var auctionItem = _db.AuctionItems.Include(ai => ai.Auction).FirstOrDefault(ai => ai.Id == id);
        if (auctionItem == null)
            return NotFound("Auction item not found.");

        return Ok(auctionItem);
    }

    // POST: /auction-items
    [HttpPost]
    [IgnoreAntiforgeryToken] // Dit moet weg zodra JWT is geimplementeerd
    public IActionResult CreateAuctionItem([FromForm] AuctionItemDto dto)
    {
        // Validatie
        var auction = _db.Auctions.FirstOrDefault(a => a.Id == dto.AuctionId);
        if (auction == null)
            return BadRequest("Auction not found.");

        var auctionItem = new AuctionItem
        {
            Id = Guid.NewGuid(),
            AuctionId = auction.Id,
            LotNumber = dto.LotNumber,
            ProductId = dto.ProductId,
            Status = "queued"
        };

        _db.AuctionItems.Add(auctionItem);
        _db.SaveChanges();

        return CreatedAtAction(nameof(GetAuctionItemById), new { id = auctionItem.Id }, auctionItem);
    }

    // PUT: /auction-items/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateAuctionItem(Guid id, [FromForm] AuctionItemDto dto)
    {
        // Validatie
        var auctionItem = _db.AuctionItems.Include(ai => ai.Auction).FirstOrDefault(ai => ai.Id == id);
        if (auctionItem == null)
            return NotFound("Auction item not found.");

        var auction = _db.Auctions.FirstOrDefault(a => a.Id == dto.AuctionId);
        if (auction == null)
            return BadRequest("Auction not found.");

        auctionItem.AuctionId = auction.Id;
        auctionItem.LotNumber = dto.LotNumber;
        auctionItem.ProductId = dto.ProductId;
        auctionItem.Status = dto.Status;

        _db.SaveChanges();

        return Ok(auctionItem);
    }

    // DELETE: /auction-items/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteAuctionItem(Guid id)
    {
        var auctionItem = _db.AuctionItems.Find(id);
        if (auctionItem == null)
            return NotFound("Auction item not found.");

        _db.AuctionItems.Remove(auctionItem);
        _db.SaveChanges();

        return NoContent();
    }

    public class AuctionItemDto
    {
        public Guid AuctionId { get; set; }
        public int LotNumber { get; set; }
        public Product ProductId { get; set; }
        public string Status { get; set; } = "queued"; // queued | active | sold | unsold
    }

}
