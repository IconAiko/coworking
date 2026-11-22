
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
            .HasMany(c => c.Resources)
            .WithOne(r => r.Coworking)
            .HasForeignKey(r => r.CoworkingId);

        modelBuilder.Entity<Resource>()
            .Property(r => r.PricePerHour)
            .HasPrecision(10, 2);
    }
}
