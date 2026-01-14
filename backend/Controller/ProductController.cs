using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace backend.Controllers;

[Route("products")]
public class ProductController : Controller
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    // GET /products
    [Authorize(Roles = "admin,supplier,auctioneer")]
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] Guid? clockLocationId)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Species)
            .Include(p => p.ClockLocation)
            .AsQueryable();

        if (clockLocationId.HasValue)
        {
            query = query.Where(p => p.ClockLocationId == clockLocationId.Value);
        }

        var products = await query.ToListAsync();
        return Ok(products);
    }

    // GET /products/{id}
    [Authorize(Roles = "supplier,auctioneer,admin")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Species)
            .Include(p => p.ClockLocation)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return NotFound();

        return Ok(product);
    }

    // POST /products
    [Authorize(Roles = "supplier,admin")]
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid supplierId))
        {
            return Unauthorized("Invalid user id");
        }
        // check of species bestaat
        var speciesExists = await _db.Species.AnyAsync(s => s.Id == dto.SpeciesId);
        if (!speciesExists)
            return BadRequest("Invalid SpeciesId.");

        if (dto.ClockLocationId.HasValue)
        {
            var clockLocationExists = await _db.ClockLocations.AnyAsync(cl => cl.Id == dto.ClockLocationId.Value);
            if (!clockLocationExists)
                return BadRequest("Invalid ClockLocationId.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            SpeciesId = dto.SpeciesId,
            PotSize = dto.PotSize,
            StemLength = dto.StemLength,
            Quantity = dto.Quantity,
            MinPrice = dto.MinPrice,
            PhotoUrl = dto.PhotoUrl,
            ClockLocationId = dto.ClockLocationId
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT /products/{id}
    [Authorize(Roles = "supplier,admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return NotFound();

        if (dto.SpeciesId.HasValue)
        {
            var exists = await _db.Species.AnyAsync(s => s.Id == dto.SpeciesId.Value);
            if (!exists)
                return BadRequest("Invalid SpeciesId.");

            product.SpeciesId = dto.SpeciesId.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.PotSize))
            product.PotSize = dto.PotSize;

        if (dto.StemLength.HasValue)
            product.StemLength = dto.StemLength.Value;

        if (dto.Quantity.HasValue)
            product.Quantity = dto.Quantity.Value;

        if (dto.MinPrice.HasValue)
            product.MinPrice = dto.MinPrice.Value;

        if (dto.PhotoUrl is not null)
            product.PhotoUrl = string.IsNullOrWhiteSpace(dto.PhotoUrl) ? null : dto.PhotoUrl;

        if (dto.ClockLocationId.HasValue)
        {
            var clockLocationExists = await _db.ClockLocations.AnyAsync(cl => cl.Id == dto.ClockLocationId.Value);
            if (!clockLocationExists)
                return BadRequest("Invalid ClockLocationId.");
            product.ClockLocationId = dto.ClockLocationId.Value;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /products/{id}
    [Authorize(Roles = "supplier,admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateProductDto
{
    public Guid SupplierId { get; set; }
    public Guid SpeciesId { get; set; }
    public string PotSize { get; set; } = string.Empty;
    public int StemLength { get; set; }
    public int Quantity { get; set; }
    public decimal MinPrice { get; set; }
    public string? PhotoUrl { get; set; }
    public Guid? ClockLocationId { get; set; }
}

public class UpdateProductDto
{
    public Guid? SpeciesId { get; set; }
    public string? PotSize { get; set; }
    public int? StemLength { get; set; }
    public int? Quantity { get; set; }
    public decimal? MinPrice { get; set; }
    public string? PhotoUrl { get; set; }
    public Guid? ClockLocationId { get; set; }
}