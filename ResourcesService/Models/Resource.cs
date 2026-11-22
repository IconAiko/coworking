namespace ResourcesService.Models;

public class Resource
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal PricePerHour { get; set; }

    public bool IsAvailable { get; set; } = true;

    public long CoworkingId { get; set; }

    public Coworking Coworking { get; set; } = null!;
}
