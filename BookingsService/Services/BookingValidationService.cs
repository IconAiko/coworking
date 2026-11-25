using BookingsService.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookingsService.Services;

public class BookingValidationService
{
    private readonly BookingsDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public BookingValidationService(
        BookingsDbContext context,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<BookingValidationError?> ValidateAsync(
        long resourceId,
        DateTime start,
        DateTime end,
        long? excludeBookingId = null)
    {
        // Проверка корректности данных

        if (resourceId <= 0)
            return new(400, "Некорректный ID ресурса");

        if (start >= end)
            return new(400,
                "Время начала должно быть раньше окончания");

        if (start <= DateTime.Now)
            return new(400,
                "Нельзя бронировать время в прошлом");

        if (start.Date != end.Date)
            return new(400,
                "Бронирование должно быть в пределах одного дня");

        // Получение информации о ресурсе

        ResourceInfo? resource;

        try
        {
            var client = _httpClientFactory
                .CreateClient("ResourcesService");

            using var response = await client.GetAsync(
                $"api/resources/{resourceId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new(400, "Ресурс не существует");

            if (!response.IsSuccessStatusCode)
                return new(503,
                    "Ошибка сервиса ресурсов");

            resource = await response.Content
                .ReadFromJsonAsync<ResourceInfo>();

            if (resource == null)
                return new(503,
                    "Сервис ресурсов вернул пустой ответ");
        }
        catch (HttpRequestException)
        {
            return new(503,
                "Сервис ресурсов недоступен");
        }
        catch (TaskCanceledException)
        {
            return new(503,
                "Превышено время ожидания сервиса ресурсов");
        }
        catch (JsonException)
        {
            return new(503,
                "Некорректный ответ сервиса ресурсов");
        }

        // Проверка доступности ресурса

        if (!resource.IsAvailable)
            return new(409,
                "Ресурс недоступен для бронирования");

        // Проверка часов работы коворкинга

        var startTime = TimeOnly.FromDateTime(start);
        var endTime = TimeOnly.FromDateTime(end);

        if (startTime < resource.OpeningTime ||
            endTime > resource.ClosingTime)
        {
            return new(400,
                "Время выходит за часы работы коворкинга");
        }

        // Проверка пересечений бронирований

        var bookings = _context.Bookings
            .AsNoTracking()
            .Where(b =>
                b.ResourceId == resourceId &&
                (b.Status == "Confirmed" ||
                 b.Status == "InUse") &&
                b.StartTime < end &&
                start < b.EndTime);

        // Исключение текущей брони при редактировании
        if (excludeBookingId.HasValue)
        {
            bookings = bookings.Where(
                b => b.Id != excludeBookingId.Value);
        }

        if (await bookings.AnyAsync())
        {
            return new(409,
                "Ресурс уже забронирован на выбранное время");
        }

        // Все проверки пройдены
        return null;
    }
}

// Результат неуспешной проверки
public record BookingValidationError(
    int StatusCode,
    string Message);

// Данные, получаемые от ResourcesService
public class ResourceInfo
{
    public long Id { get; set; }

    public bool IsAvailable { get; set; }

    public TimeOnly OpeningTime { get; set; }

    public TimeOnly ClosingTime { get; set; }
}
