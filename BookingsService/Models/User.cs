namespace BookingsService.Models;

public abstract class User
{
    public long Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime RegisteredAt { get; set; }
        = DateTime.UtcNow;
}
