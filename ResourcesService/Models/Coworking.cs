using Org.BouncyCastle.Asn1.Cmp;

namespace ResourcesService.Models;

public class Coworking
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TimeOnly OpeningTime { get; set; }

    public TimeOnly ClosingTime { get; set; }

    public ICollection<Resource> Resources { get; set; }
        = new List<Resource>();
}
