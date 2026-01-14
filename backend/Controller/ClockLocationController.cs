using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[Route("clock-locations")]
[Authorize]
public class ClockLocationController : Controller
{
    private readonly AppDbContext _db;

    public ClockLocationController(AppDbContext db)
    {
        _db = db;
    }

    // GET /clock-locations
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var clockLocations = await _db.ClockLocations
            .AsNoTracking()
            .OrderBy(cl => cl.Name)
            .ToListAsync();

        return Ok(clockLocations);
    }

    // GET /clock-locations/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var clockLocation = await _db.ClockLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(cl => cl.Id == id);

        if (clockLocation is null)
            return NotFound();

        return Ok(clockLocation);
    }

    // POST /clock-locations
    [HttpPost("")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateClockLocationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required");

        var clockLocation = new ClockLocation
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim()
        };

        _db.ClockLocations.Add(clockLocation);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = clockLocation.Id },
            clockLocation
        );
    }

    // DELETE /clock-locations/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clockLocation = await _db.ClockLocations.FirstOrDefaultAsync(cl => cl.Id == id);
        if (clockLocation is null)
            return NotFound();

        // Check if any auctions are using this clock location
        var hasAuctions = await _db.Auctions.AnyAsync(a => a.ClockLocationId == id);
        if (hasAuctions)
            return BadRequest("Cannot delete clock location that is in use by auctions");

        _db.ClockLocations.Remove(clockLocation);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
