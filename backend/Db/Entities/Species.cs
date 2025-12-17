using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Db.Entities;

public class Species
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [MaxLength(200)]
    public string? LatinName { get; set; }

    [MaxLength(100)]
    public string? Family { get; set; }

    [MaxLength(50)]
    public string? GrowthType { get; set; }

    public string? Description { get; set; }

    public bool IsPerennial { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}