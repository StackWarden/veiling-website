using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Db.Entities;

public class ClockLocation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
