
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ResourcesService.Data;

namespace ResourcesService.Services;

public class ResourceVisibilityService
{
    private readonly ResourcesDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public ResourceVisibilityService(
        ResourcesDbContext context,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    // Изменение видимости ресурса
    public async Task<ResourceVisibilityError?> ToggleVisibilityAsync(
        long resourceId)
    {
        // Находим ресурс
        var resource = await _context.Resources.FindAsync(resourceId);

        if (resource == null)
            return new(404, "Ресурс не найден.");

        // Если ресурс скрыт — делаем его доступным
        if (!resource.IsAvailable)
        {
            resource.IsAvailable = true;
            await _context.SaveChangesAsync();

            return null;
        }

        // Если ресурс доступен — проверяем бронирования
        ActiveBookingsResponse? result;

        try
        {
            var client = _httpClientFactory
                .CreateClient("BookingsService");

            using var response = await client.GetAsync(
                $"api/bookings/resource/{resourceId}/has-active");

            if (!response.IsSuccessStatusCode)
                return new(503, "Ошибка сервиса бронирований");

            result = await response.Content
                .ReadFromJsonAsync<ActiveBookingsResponse>();

            if (result == null)
                return new(503, "Получен пустой ответ сервиса");
        }
        catch (HttpRequestException)
        {
            return new(503, "Сервис бронирований недоступен");
        }
        catch (TaskCanceledException)
        {
            return new(503, "Превышено время ожидания");
        }
        catch (System.Text.Json.JsonException)
        {
            return new(503, "Некорректный ответ сервиса");
        }

        // Если есть активные бронирования — скрытие запрещено
        if (result.HasActiveBookings)
        {
            return new(409,
                "Нельзя скрыть ресурс с действующими бронированиями");
        }

        // Если бронирований нет — скрываем
        resource.IsAvailable = false;

        await _context.SaveChangesAsync();

        return null;
    }

}

public record ResourceVisibilityError(
    int StatusCode,
    string Message);

public record ActiveBookingsResponse(
    bool HasActiveBookings);
