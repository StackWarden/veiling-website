using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore; 

namespace backend.Controllers;

[Route("AuctionItem")]

public class AuctionItemController : Controller
{
    private readonly AppDbContext _db;

    public AuctionItemController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /AuctionItem
    [HttpGet]
    public IActionResult GetAllAuctionItems()
    {
        var auctionItems = _db.AuctionItems.ToList();
        return Ok(auctionItems);
    }

    // GET: /AuctionItem/{id}
    [HttpGet("{id}")]
    public IActionResult GetAuctionItemById(Guid id)
    {
        var auctionItem = _db.AuctionItems.FirstOrDefault(a => a.Id == id);
        if (auctionItem == null)
            return NotFound("Auction item not found.");

        return Ok(auctionItem);
    }

    // POST: /AuctionItem
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult CreateAuctionItem([FromBody] CreateAuctionItemDto dto)
    {
        var auctionItem = new AuctionItem();
        if (auctionItem == null)
            return BadRequest("Invalid auction item data.");

        if (dto.LotNumber <= 0)
            return BadRequest("LotNumber must be a positive integer.");
        
        if (_db.Auctions.Find(dto.AuctionId) == null)
            return BadRequest("Referenced Auction does not exist.");

        if (_db.Products.Find(dto.ProductId) == null)
            return BadRequest("Referenced Product does not exist.");

        if (_db.AuctionItems.Any(ai => ai.AuctionId == dto.AuctionId && ai.ProductId == dto.ProductId))
            return BadRequest("An auction item with the same AuctionId and ProductId already exists.");

        auctionItem.Id = Guid.NewGuid();
        auctionItem.AuctionId = dto.AuctionId;
        auctionItem.ProductId = dto.ProductId;
        auctionItem.LotNumber = dto.LotNumber;
        auctionItem.Status = dto.Status;

        _db.AuctionItems.Add(auctionItem);
        _db.SaveChanges();

        return Ok($"Auction item {auctionItem.Id} created successfully.");
    }

    // PUT: /AuctionItem/{id}
    [HttpPut("{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult UpdateAuctionItem(Guid id, [FromBody] CreateAuctionItemDto dto)
    {
        var auctionItem = _db.AuctionItems.FirstOrDefault(a => a.Id == id);
        if (auctionItem == null)
            return NotFound("Auction item not found.");

        auctionItem.LotNumber = dto.LotNumber;
        auctionItem.Status = dto.Status;

        _db.SaveChanges();

        return Ok($"Auction item {auctionItem.Id} updated successfully.");
    }

    // DELETE: /AuctionItem/{id}
    [HttpDelete("{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteAuctionItem(Guid id)
    {
        var auctionItem = _db.AuctionItems.Find(id);
        if (auctionItem == null)
            return NotFound("Auction item not found.");

        _db.AuctionItems.Remove(auctionItem);
        _db.SaveChanges();

        return Ok($"Auction item {auctionItem.Id} deleted successfully.");
    }
    
    public class CreateAuctionItemDto
    {
        public Guid AuctionId { get; set; }
        public Guid ProductId { get; set; }
        public int LotNumber { get; set; }
        public string Status { get; set; } = "Pending";    }
}