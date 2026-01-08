using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Authorization;

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
    [Authorize]
    public IActionResult GetAllAuctionItems()
    {
        var items = _db.AuctionItems
            .Select(ai => new
            {
                ai.Id,
                ai.Status,
                ai.ProductId,
                ai.AuctionId,
                ai.BuyerId,
                ai.SoldAtUtc,
                ai.SoldPrice
            })
            .ToList();

        return Ok(items);
    }

    // GET: /AuctionItem/{id}
    [HttpGet("{id}")]
    [Authorize]
    public IActionResult GetAuctionItemById(Guid id)
    {
        var auctionItem = _db.AuctionItems.FirstOrDefault(a => a.Id == id);
        if (auctionItem == null)
            return NotFound("Auction item not found.");

        return Ok(auctionItem);
    }

    // DELETE: /AuctionItem/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "auctioneer,admin")]
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

        public string Status { get; set; } = "Pending";
    }
}
