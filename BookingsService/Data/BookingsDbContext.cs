using Microsoft.EntityFrameworkCore;

namespace BookingsService.Data;

public class BookingsDbContext : DbContext
{
    public BookingsDbContext(
        DbContextOptions<BookingsDbContext> options)
        : base(options)
    {
    }
}
