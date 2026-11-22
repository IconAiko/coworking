namespace BookingsService.Models;

public class Client : User
{
    public string ContactPhone { get; set; }
        = string.Empty;

    public ICollection<Booking> Bookings { get; set; }
        = new List<Booking>();
}
