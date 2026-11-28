using Microsoft.EntityFrameworkCore;
using ResourcesService.Data;
using ResourcesService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "MySQL connection string not configured");

builder.Services.AddDbContext<ResourcesDbContext>(options =>
    options.UseMySQL(connectionString, mysql =>
        mysql.MigrationsHistoryTable("__ResourcesMigrationsHistory")));

builder.Services.AddHttpClient("BookingsService", client =>
{
    var serviceUrl = builder.Configuration[
        "Services:BookingsServiceUrl"]
        ?? throw new InvalidOperationException(
            "Не указан адрес BookingsService");

    client.BaseAddress = new Uri(serviceUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddScoped<ResourceVisibilityService>();

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
