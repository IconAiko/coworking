using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingsService.Data;
using BookingsService.Models;

namespace BookingsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly BookingsDbContext _context;

    public BookingsController(BookingsDbContext context)
    {
        _context = context;
    }

    // GET: api/bookings - все бронирования
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bookings = await _context.Bookings
            .AsNoTracking()
            .Select(b => new
            {
                b.Id,
                b.ClientId,
                ClientName = b.Client.FullName,
                b.ResourceId,
                b.CreatedAt,
                b.StartTime,
                b.EndTime,
                b.Status
            })
            .ToListAsync();

        return Ok(bookings);
    }

    // GET: api/bookings/1 - конкретное бронирование
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new
            {
                b.Id,
                b.ClientId,
                ClientName = b.Client.FullName,
                b.ResourceId,
                b.CreatedAt,
                b.StartTime,
                b.EndTime,
                b.Status
            })
            .FirstOrDefaultAsync();

        if (booking == null)
            return NotFound("Бронирование не найдено");

        return Ok(booking);
    }

    // POST: api/bookings - создать бронирование
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingRequest request)
    {
        if (request.ClientId <= 0 || request.ResourceId <= 0)
            return BadRequest("Некорректный ID клиента или ресурса");

        if (request.StartTime >= request.EndTime)
            return BadRequest(
                "Время начала должно быть раньше времени окончания");

        bool clientExists = await _context.Clients
            .AnyAsync(c => c.Id == request.ClientId);

        if (!clientExists)
            return BadRequest("Указанный клиент не существует");

        var booking = new Booking
        {
            ClientId = request.ClientId,
            ResourceId = request.ResourceId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = DateTime.UtcNow,
            Status = "Confirmed"
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = booking.Id },
            new { booking.Id });
    }

    // PUT: api/bookings/1 - изменить бронирование
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateBookingRequest request)
    {
        if (request.ResourceId <= 0)
            return BadRequest("Некорректный ID ресурса");

        if (request.StartTime >= request.EndTime)
            return BadRequest(
                "Время начала должно быть раньше времени окончания");

        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound("Бронирование не найдено");

        if (booking.Status == "Cancelled")
            return Conflict("Отменённое бронирование нельзя изменять");

        booking.ResourceId = request.ResourceId;
        booking.StartTime = request.StartTime;
        booking.EndTime = request.EndTime;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/bookings/1 - удалить бронирование
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Cancel(long id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound("Бронирование не найдено");

        if (booking.Status == "Cancelled")
            return Conflict("Бронирование уже отмененоdotnet ");

        booking.Status = "Cancelled";

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// DTO
public class CreateBookingRequest
{
    public long ClientId { get; set; }
    public long ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

// DTO
public class UpdateBookingRequest
{
    public long ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
