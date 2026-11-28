
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookingsService.Data;
using BookingsService.Models;
using BookingsService.Services;

namespace BookingsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly BookingsDbContext _context;
    private readonly BookingValidationService _validationService;

    public BookingsController(
        BookingsDbContext context,
        BookingValidationService validationService)
    {
        _context = context;
        _validationService = validationService;
    }

    // GET: api/bookings
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

    // GET: api/bookings/1
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

    // POST: api/bookings
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBookingRequest request)
    {
        if (request.ClientId <= 0)
            return BadRequest("Некорректный ID клиента");

        bool clientExists = await _context.Clients
            .AnyAsync(c => c.Id == request.ClientId);

        if (!clientExists)
            return BadRequest("Указанный клиент не существует");

        var error = await _validationService.ValidateAsync(
            request.ResourceId,
            request.StartTime,
            request.EndTime);

        if (error != null)
            return StatusCode(error.StatusCode, error.Message);

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

    // PUT: api/bookings/1
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateBookingRequest request)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound("Бронирование не найдено");

        if (booking.Status == "Cancelled")
            return Conflict("Отменённое бронирование нельзя изменять");

        var error = await _validationService.ValidateAsync(
            request.ResourceId,
            request.StartTime,
            request.EndTime,
            id);

        if (error != null)
            return StatusCode(error.StatusCode, error.Message);

        booking.ResourceId = request.ResourceId;
        booking.StartTime = request.StartTime;
        booking.EndTime = request.EndTime;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/bookings/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Cancel(long id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound("Бронирование не найдено");

        if (booking.Status == "Cancelled")
            return Conflict("Бронирование уже отменено");

        booking.Status = "Cancelled";

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/bookings/resource/1/has-active - проверка бронирований на ресурсе
    [HttpGet("resource/{resourceId:long}/has-active")]
    public async Task<IActionResult> HasActiveBookings(
        long resourceId)
    {
        if (resourceId <= 0)
            return BadRequest("Некорректный ID ресурса");

        bool hasActiveBookings = await _context.Bookings
            .AsNoTracking()
            .AnyAsync(b =>
                b.ResourceId == resourceId &&
                (b.Status == "Confirmed" ||
                 b.Status == "InUse") &&
                b.EndTime > DateTime.Now);

        return Ok(new
        {
            hasActiveBookings
        });
    }

}

public class CreateBookingRequest
{
    public long ClientId { get; set; }
    public long ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class UpdateBookingRequest
{
    public long ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
