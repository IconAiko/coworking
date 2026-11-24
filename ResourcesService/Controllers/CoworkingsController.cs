using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResourcesService.Data;
using ResourcesService.Models;

namespace ResourcesService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoworkingsController : ControllerBase
{
    private readonly ResourcesDbContext _context;

    public CoworkingsController(ResourcesDbContext context)
    {
        _context = context;
    }

    // GET: api/coworkings - все коворкинги
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var coworkings = await _context.Coworkings
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Address,
                c.ContactPhone,
                c.Description,
                c.OpeningTime,
                c.ClosingTime
            })
            .ToListAsync();

        return Ok(coworkings);
    }

    // GET: api/coworkings/1 - конкретный коворкинг
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var coworking = await _context.Coworkings
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Address,
                c.ContactPhone,
                c.Description,
                c.OpeningTime,
                c.ClosingTime
            })
            .FirstOrDefaultAsync();

        if (coworking == null)
            return NotFound("Коворкинг не найден");

        return Ok(coworking);
    }

    // POST: api/coworkings - создать коовркинг
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCoworkingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest(
                "Название и адрес обязательны");
        }

        if (request.OpeningTime >= request.ClosingTime)
        {
            return BadRequest(
                "Некорректное время работы");
        }

        var coworking = new Coworking
        {
            Name = request.Name,
            Address = request.Address,
            ContactPhone = request.ContactPhone,
            Description = request.Description,
            OpeningTime = request.OpeningTime,
            ClosingTime = request.ClosingTime
        };

        _context.Coworkings.Add(coworking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = coworking.Id },
            new { coworking.Id });
    }
}

// DTO
public class CreateCoworkingRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
}
