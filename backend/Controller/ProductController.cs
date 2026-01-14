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

    private async Task<bool> ValidateClockLocationAsync(Guid? clockLocationId)
    {
        if (!clockLocationId.HasValue)
            return true;

        return await _db.ClockLocations
            .AsNoTracking()
            .AnyAsync(cl => cl.Id == clockLocationId.Value);
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
        if (dto == null)
        {
            return BadRequest("Request body is required.");
        }

        string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid supplierId))
        {
            return Unauthorized("Invalid user id");
        }

        try
        {
            var speciesExists = await _db.Species.AnyAsync(s => s.Id == dto.SpeciesId);
            var clockLocationValid = await ValidateClockLocationAsync(dto.ClockLocationId);

            if (!speciesExists)
                return BadRequest("Invalid SpeciesId.");

            if (!clockLocationValid)
                return BadRequest("Invalid ClockLocationId.");

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
            
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException saveEx)
            {
                return StatusCode(500, new { error = $"Failed to save product: {saveEx.Message}", type = saveEx.GetType().Name, innerException = saveEx.InnerException?.Message });
            }

            // Reload product with related entities for the response
            Product? createdProduct;
            try
            {
                createdProduct = await _db.Products
                    .AsNoTracking()
                    .Include(p => p.Species)
                    .Include(p => p.ClockLocation)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);
            }
            catch (Exception reloadEx)
            {
                return StatusCode(500, new { error = $"Failed to reload product: {reloadEx.Message}", type = reloadEx.GetType().Name });
            }

            if (createdProduct == null)
            {
                return StatusCode(500, new { error = "Failed to retrieve created product." });
            }

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, createdProduct);
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, new { error = $"Database error: {dbEx.Message}", type = dbEx.GetType().Name, innerException = dbEx.InnerException?.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, type = ex.GetType().Name, stackTrace = ex.StackTrace });
        }
    }

    // PUT /products/{id}
    [Authorize(Roles = "supplier,admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized("Invalid user id");
        }

        var isAdmin = User.IsInRole("admin");

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return NotFound();

        if (!isAdmin && product.SupplierId != userId)
        {
            return Forbid("You can only update your own products.");
        }

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
            if (!await ValidateClockLocationAsync(dto.ClockLocationId))
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
        string userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return Unauthorized("Invalid user id");
        }

        var isAdmin = User.IsInRole("admin");

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return NotFound();

        if (!isAdmin && product.SupplierId != userId)
        {
            return Forbid("You can only delete your own products.");
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateProductDto
{
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