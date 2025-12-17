using Microsoft.AspNetCore.Mvc;
using backend.Db;
using backend.Db.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[Route("species")]
[Authorize(Roles = "admin")]
public class SpeciesController : Controller
{
    private readonly AppDbContext _db;

    public SpeciesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /species
    [Authorize]
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var species = await _db.Species
            .AsNoTracking()
            .OrderBy(s => s.Title)
            .ToListAsync();

        return Ok(species);
    }

    // GET /species/{id}
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var species = await _db.Species
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (species is null)
            return NotFound();

        return Ok(species);
    }

    // POST /species
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateSpeciesDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest("Title is required");

        var species = new Species
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            LatinName = dto.LatinName,
            Family = dto.Family,
            GrowthType = dto.GrowthType,
            Description = dto.Description,
            IsPerennial = dto.IsPerennial
        };

        _db.Species.Add(species);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = species.Id },
            species
        );
    }

    // PUT /species/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpeciesDto dto)
    {
        var species = await _db.Species.FirstOrDefaultAsync(s => s.Id == id);
        if (species is null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Title))
            species.Title = dto.Title.Trim();

        if (dto.LatinName is not null)
            species.LatinName = dto.LatinName;

        if (dto.Family is not null)
            species.Family = dto.Family;

        if (dto.GrowthType is not null)
            species.GrowthType = dto.GrowthType;

        if (dto.Description is not null)
            species.Description = dto.Description;

        if (dto.IsPerennial.HasValue)
            species.IsPerennial = dto.IsPerennial.Value;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /species/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var species = await _db.Species.FirstOrDefaultAsync(s => s.Id == id);
        if (species is null)
            return NotFound();

        _db.Species.Remove(species);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
