namespace backend.Db.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // supplier | buyer | auctioneer
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}