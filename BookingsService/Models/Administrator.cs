namespace BookingsService.Models;

public class Administrator : User
{
    public string Position { get; set; }
        = string.Empty;
}
