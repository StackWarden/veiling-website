using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore; 

namespace backend.Controllers;
[Route("bids")]
public class BidController : Controller
{
    private readonly AppDbContext _db;
    public BidController(AppDbContext db)
    {
        _db = db;
    }
    // POST: /bids/place
    [HttpPost("place")]
    [IgnoreAntiforgeryToken] // Dit moet weg zodra JWT is geimplementeerd
    public IActionResult PlaceBid([FromForm] PlaceBidDto dto)
    {
        // Validatie
        if (string.IsNullOrWhiteSpace(dto.AuctionneerId) || string.IsNullOrWhiteSpace(dto.BuyerId) ||
            string.IsNullOrWhiteSpace(dto.IndividualPrice) || string.IsNullOrWhiteSpace(dto.Quantity))
            return BadRequest("All fields are required.");
        // Nieuwe bod aanmaken
        var bid = new Bid
        {
            Id = Guid.NewGuid(),
            AuctionneerId = dto.AuctionneerId,
            BuyerId = dto.BuyerId,
            IndividualPrice = dto.IndividualPrice,
            Quantity = dto.Quantity,
            CreatedAt = DateTime.UtcNow
        };
        _db.Bids.Add(bid);
        _db.SaveChanges();
        return Ok($"Bid placed successfully with ID: {bid.Id}");
    }
}

public class PlaceBidDto
{
    public string AuctionneerId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public string IndividualPrice { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}
