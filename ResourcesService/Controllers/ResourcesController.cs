using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourcesService.Data;
using ResourcesService.Models;

namespace ResourcesService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly ResourcesDbContext _context;

    public ResourcesController(ResourcesDbContext context)
    {
        _context = context;
    }

    // GET: api/resources - все ресурсы
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var resources = await _context.Resources
            .AsNoTracking()
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Type,
                r.Description,
                r.Capacity,
                r.PricePerHour,
                r.IsAvailable,
                r.CoworkingId,
                CoworkingName = r.Coworking.Name
            })
            .ToListAsync();

        return Ok(resources);
    }

    // GET: api/resources/1 - конкретный ресурс
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var resource = await _context.Resources
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Type,
                r.Description,
                r.Capacity,
                r.PricePerHour,
                r.IsAvailable,
                r.CoworkingId,
                CoworkingName = r.Coworking.Name,
                OpeningTime = r.Coworking.OpeningTime,
                ClosingTime = r.Coworking.ClosingTime
            })
            .FirstOrDefaultAsync();

        if (resource == null)
            return NotFound("Ресурс не найден");

        return Ok(resource);
    }

    // POST: api/resources - создать ресурс
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ResourceRequest request)
    {
        var validationError = Validate(request);
        if (validationError != null)
            return BadRequest(validationError);

        bool coworkingExists = await _context.Coworkings
            .AnyAsync(c => c.Id == request.CoworkingId);

        if (!coworkingExists)
            return BadRequest("Указанный коворкинг не существует");

        var resource = new Resource
        {
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            Capacity = request.Capacity,
            PricePerHour = request.PricePerHour,
            IsAvailable = request.IsAvailable,
            CoworkingId = request.CoworkingId
        };

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = resource.Id },
            new { resource.Id });
    }

    // PUT: api/resources/1 - изменить ресурс
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] ResourceRequest request)
    {
        var validationError = Validate(request);
        if (validationError != null)
            return BadRequest(validationError);

        var resource = await _context.Resources.FindAsync(id);

        if (resource == null)
            return NotFound("Ресурс не найден");

        bool coworkingExists = await _context.Coworkings
            .AnyAsync(c => c.Id == request.CoworkingId);

        if (!coworkingExists)
            return BadRequest("Указанный коворкинг не существует");

        resource.Name = request.Name;
        resource.Type = request.Type;
        resource.Description = request.Description;
        resource.Capacity = request.Capacity;
        resource.PricePerHour = request.PricePerHour;
        resource.IsAvailable = request.IsAvailable;
        resource.CoworkingId = request.CoworkingId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Валидация
    private static string? Validate(ResourceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Название ресурса обязательно";

        if (string.IsNullOrWhiteSpace(request.Type))
            return "Тип ресурса обязателен";

        if (request.Capacity <= 0)
            return "Вместимость должна быть больше нуля";

        if (request.PricePerHour < 0)
            return "Стоимость не может быть отрицательной";

        return null;
    }
}

// DTO
public class ResourceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsAvailable { get; set; } = true;
    public long CoworkingId { get; set; }
}
