namespace BookingsService.Models;

public class Booking
{
    public long Id { get; set; }

    public long ClientId { get; set; }

    public Client Client { get; set; } = null!;

    public long ResourceId { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; }
        = "Confirmed";
}
