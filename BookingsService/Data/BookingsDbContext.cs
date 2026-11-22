using Microsoft.EntityFrameworkCore;
using BookingsService.Models;

namespace BookingsService.Data;

public class BookingsDbContext : DbContext
{
    public BookingsDbContext(
        DbContextOptions<BookingsDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Administrator> Administrators =>
        Set<Administrator>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Наследование User - Client / Administrator
        modelBuilder.Entity<User>()
            .HasDiscriminator(u => u.Role)
            .HasValue<Client>("Client")
            .HasValue<Administrator>("Administrator");

        // 1:N (Клиент/Бронирования)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Client)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Email пользователя должен быть уникальным
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
