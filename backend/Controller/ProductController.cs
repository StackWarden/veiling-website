using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

// Deze controller beheert alle productgerelateerde acties.
// CRUD-endpoints, validatie en lichte logica voor kloklocaties en veilingdatums.
// Simpel gehouden, zodat het overzichtelijk blijft.
[Route("products")]
public class ProductController : Controller
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    // GET /products
    // Haalt alle producten op zonder tracking (lees: geen onnodige EF-overhead).
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return Ok(await _db.Products.AsNoTracking().ToListAsync());
    }

    // GET /products/{id}
    // Haalt één product op op basis van het ID, of geeft 404 als het niet bestaat.
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    // POST /products
    // Maakt een nieuw product aan, inclusief basisvalidatie van kloklocatie en veilingdatum.
    [HttpPost("")]
    [IgnoreAntiforgeryToken] // weghalen zodra JWT klaar is
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        if (!Enum.TryParse<ClockLocation>(dto.ClockLocation, true, out var loc))
        {
            return BadRequest("Invalid ClockLocation.");
        }

        DateOnly? auctionDate = null;

        if (!string.IsNullOrWhiteSpace(dto.AuctionDate))
        {
            if (!DateOnly.TryParse(dto.AuctionDate, out var parsed))
            {
                return BadRequest("AuctionDate must be YYYY-MM-DD.");
            }

            auctionDate = parsed;
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            SupplierId = dto.SupplierId,
            Species = dto.Species,
            PotSize = dto.PotSize,
            StemLength = dto.StemLength,
            Quantity = dto.Quantity,
            MinPrice = dto.MinPrice,
            ClockLocation = loc,
            AuctionDate = auctionDate,
            PhotoUrl = dto.PhotoUrl
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT /products/{id}
    // Werkt bestaande producten bij, veld per veld, met lichte validatie.
    [HttpPut("{id:guid}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(dto.Species))
        {
            p.Species = dto.Species;
        }

        if (!string.IsNullOrWhiteSpace(dto.PotSize))
        {
            p.PotSize = dto.PotSize;
        }

        if (dto.StemLength is not null)
        {
            p.StemLength = dto.StemLength.Value;
        }

        if (dto.Quantity is not null)
        {
            p.Quantity = dto.Quantity.Value;
        }

        if (dto.MinPrice is not null)
        {
            p.MinPrice = dto.MinPrice.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.ClockLocation) &&
            Enum.TryParse<ClockLocation>(dto.ClockLocation, true, out var loc))
        {
            p.ClockLocation = loc;
        }

        if (dto.AuctionDate is not null)
        {
            if (dto.AuctionDate == "")
            {
                p.AuctionDate = null;
            }
            else if (DateOnly.TryParse(dto.AuctionDate, out var d))
            {
                p.AuctionDate = d;
            }
            else
            {
                return BadRequest("AuctionDate must be YYYY-MM-DD.");
            }
        }

        if (dto.PhotoUrl is not null)
        {
            p.PhotoUrl = string.IsNullOrWhiteSpace(dto.PhotoUrl) ? null : dto.PhotoUrl;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /products/{id}
    // Verwijdert een product uit de database.
    [HttpDelete("{id:guid}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
        {
            return NotFound();
        }

        _db.Products.Remove(p);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

// DTO voor het aanmaken van een product.
// Bevat de basisinformatie die een leverancier aanlevert.
public class CreateProductDto
{
    public Guid SupplierId { get; set; }
    public string Species { get; set; } = string.Empty;
    public string PotSize { get; set; } = string.Empty;
    public int StemLength { get; set; }
    public int Quantity { get; set; }
    public decimal MinPrice { get; set; }
    public string ClockLocation { get; set; } = string.Empty; // Naaldwijk | Aalsmeer | Rijnsburg | Eelde
    public string? AuctionDate { get; set; }
    public string? PhotoUrl { get; set; }
}

// DTO voor het bijwerken van een product.
// Alle velden optioneel, zodat je alleen aanpast wat nodig is.
public class UpdateProductDto
{
    public string? Species { get; set; }
    public string? PotSize { get; set; }
    public int? StemLength { get; set; }
    public int? Quantity { get; set; }
    public decimal? MinPrice { get; set; }
    public string? ClockLocation { get; set; }
    public string? AuctionDate { get; set; }
    public string? PhotoUrl { get; set; }
}
