using Microsoft.EntityFrameworkCore;
using BookingsService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "MySQL connection string not configured.");

builder.Services.AddDbContext<BookingsDbContext>(options =>
    options.UseMySQL(connectionString, mysql =>
        mysql.MigrationsHistoryTable("__BookingsMigrationsHistory")));

builder.Services.AddHttpClient("ResourcesService", client =>
{
    var serviceUrl = builder.Configuration[
        "Services:ResourcesServiceUrl"]
        ?? throw new InvalidOperationException(
            "Не указан адрес ResourcesService");

    client.BaseAddress = new Uri(serviceUrl);

    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
