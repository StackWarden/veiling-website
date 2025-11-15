using System;

namespace backend.ViewModels;

public class AuctionViewModel
{
    public Guid Id { get; set; }
    public Guid AuctionneerId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
