using Microsoft.EntityFrameworkCore;

namespace ResourcesService.Data;

public class ResourcesDbContext : DbContext
{
    public ResourcesDbContext(
        DbContextOptions<ResourcesDbContext> options)
        : base(options)
    {
    }
}
