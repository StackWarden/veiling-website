using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Db;
using backend.Db.Entities;
using backend.ViewModels;

namespace backend.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public NotificationsController(AppDbContext db) => _db = db;

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetForUser(Guid userId)
    {
        var items = await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationViewModel
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var n = await _db.Notifications
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new NotificationViewModel
            {
                Id = x.Id,
                UserId = x.UserId,
                Type = x.Type,
                Title = x.Title,
                Message = x.Message,
                CreatedAt = x.CreatedAt,
                ReadAt = x.ReadAt
            })
            .FirstOrDefaultAsync();

        return n is null ? NotFound() : Ok(n);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NotificationViewModel dto)
    {
        var entity = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt,
            ReadAt = dto.ReadAt
        };

        _db.Notifications.Add(entity);
        await _db.SaveChangesAsync();

        var vm = new NotificationViewModel
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Type = entity.Type,
            Title = entity.Title,
            Message = entity.Message,
            CreatedAt = entity.CreatedAt,
            ReadAt = entity.ReadAt
        };

        return CreatedAtAction(nameof(GetById), new { id = vm.Id }, vm);
    }

    [HttpPost("{id:guid}/markread")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var n = await _db.Notifications.FindAsync(id);
        if (n == null) return NotFound();
        n.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}