using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Db;
using backend.Db.Entities;

namespace backend.Controllers;

[Route("salesresults")]
public class SaleResultsController : Controller
{
    private readonly AppDbContext _db;
    public SaleResultsController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
        => Ok(await _db.SaleResults.AsNoTracking().ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var s = await _db.SaleResults.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpPost("")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateSaleResultDto dto)
    {
        if (dto.Quantity <= 0) return BadRequest("Quantity must be > 0.");
        if (dto.FinalPrice < 0) return BadRequest("FinalPrice must be >= 0.");

        // 0..1 relatie voor actionitem en salesresult
        if (await _db.SaleResults.AnyAsync(r => r.AuctionItemId == dto.AuctionItemId))
            return Conflict("This auction item already has a sale result.");

        var s = new SaleResult
        {
            Id            = Guid.NewGuid(),
            AuctionItemId = dto.AuctionItemId,
            BuyerId       = dto.BuyerId,
            FinalPrice    = dto.FinalPrice,
            Quantity      = dto.Quantity,
            TotalProceeds = dto.FinalPrice * dto.Quantity
        };

        _db.SaleResults.Add(s);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = s.Id }, s);
    }

    [HttpPut("{id:guid}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSaleResultDto dto)
    {
        var s = await _db.SaleResults.FirstOrDefaultAsync(x => x.Id == id);
        if (s is null) return NotFound();

        if (dto.AuctionItemId is not null) s.AuctionItemId = dto.AuctionItemId.Value;
        if (dto.BuyerId       is not null) s.BuyerId       = dto.BuyerId.Value;
        if (dto.FinalPrice    is not null) s.FinalPrice    = dto.FinalPrice.Value;
        if (dto.Quantity      is not null) s.Quantity      = dto.Quantity.Value;

        if (dto.FinalPrice is not null || dto.Quantity is not null)
            s.TotalProceeds = s.FinalPrice * s.Quantity;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var s = await _db.SaleResults.FirstOrDefaultAsync(x => x.Id == id);
        if (s is null) return NotFound();
        _db.SaleResults.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateSaleResultDto
{
    public Guid AuctionItemId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal FinalPrice { get; set; }
    public int Quantity { get; set; }
}

public class UpdateSaleResultDto
{
    public Guid? AuctionItemId { get; set; }
    public Guid? BuyerId { get; set; }
    public decimal? FinalPrice { get; set; }
    public int? Quantity { get; set; }
}
