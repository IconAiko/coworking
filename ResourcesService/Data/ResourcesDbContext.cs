
using Microsoft.EntityFrameworkCore;
using ResourcesService.Models;

namespace ResourcesService.Data;

public class ResourcesDbContext : DbContext
{
    public ResourcesDbContext(
        DbContextOptions<ResourcesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Coworking> Coworkings => Set<Coworking>();

    public DbSet<Resource> Resources => Set<Resource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1:N (Коворкинг/Ресурс)
        modelBuilder.Entity<Coworking>()
            .Property(c => c.OpeningTime)
            .HasConversion(
                time => time.ToTimeSpan(),
                span => TimeOnly.FromTimeSpan(span))
            .HasColumnType("time");

        modelBuilder.Entity<Coworking>()
            .Property(c => c.ClosingTime)
            .HasConversion(
                time => time.ToTimeSpan(),
                span => TimeOnly.FromTimeSpan(span))
            .HasColumnType("time");
    }
}
