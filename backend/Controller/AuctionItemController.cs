using Microsoft.AspNetCore.Mvc;
using backend.Db;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<IActionResult> GetAllAuctionItems()
    {
        var items = await _db.AuctionItems
            .AsNoTracking()
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
            .ToListAsync();

        return Ok(items);
    }

    // GET: /AuctionItem/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetAuctionItemById(Guid id)
    {
        var auctionItem = await _db.AuctionItems
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        if (auctionItem == null)
            return NotFound("Auction item not found.");

        return Ok(auctionItem);
    }

    // DELETE: /AuctionItem/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "auctioneer,admin")]
    public async Task<IActionResult> DeleteAuctionItem(Guid id)
    {
        var auctionItem = await _db.AuctionItems.FirstOrDefaultAsync(a => a.Id == id);
        if (auctionItem == null)
            return NotFound("Auction item not found.");

        _db.AuctionItems.Remove(auctionItem);
        await _db.SaveChangesAsync();

        return Ok($"Auction item {auctionItem.Id} deleted successfully.");
    }
}
