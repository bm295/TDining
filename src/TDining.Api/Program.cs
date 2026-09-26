using TDining.Api.Application.DTOs;
using TDining.Api.Application.Ports.In;
using TDining.Api.Application.Ports.Out;
using TDining.Api.Application.UseCases;
using TDining.Api.Domain.Entities;
using TDining.Api.Domain.Services;
using TDining.Api.Infrastructure.Outbox;
using TDining.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var databasePath = Path.Combine(builder.Environment.ContentRootPath, "tdining.db");
var connectionString = builder.Configuration.GetConnectionString("Default") ?? $"Data Source={databasePath}";

builder.Services.AddProblemDetails();
builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200").AllowAnyHeader().AllowAnyMethod()));
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

builder.Services.AddDbContext<TDiningDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<ITableRepository, EfTableRepository>();
builder.Services.AddScoped<IMenuRepository, EfMenuRepository>();
builder.Services.AddScoped<IInventoryRepository, EfInventoryRepository>();
builder.Services.AddScoped<IPaymentRepository, EfPaymentRepository>();
builder.Services.AddScoped<IReservationRepository, EfReservationRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddSingleton<InventoryConsumptionService>();
builder.Services.AddScoped<IOrderUseCases, OrderUseCases>();
builder.Services.AddScoped<IReservationUseCases, ReservationUseCases>();
builder.Services.AddScoped<IReportingUseCases, ReportingUseCases>();
builder.Services.AddSingleton<IIntegrationEventPublisher, LoggingIntegrationEventPublisher>();
builder.Services.AddHostedService<OutboxProcessor>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TDiningDbContext>();
    await TDiningDbSeeder.InitializeAsync(dbContext);
}

app.MapGet("/api", () => Results.Ok(new
{
    service = "T Dining API",
    architecture = "Hexagonal (Ports & Adapters)",
    seatingCapacity = "60-70 seats",
    persistence = "SQLite with transactional outbox"
}));

app.MapGet("/", () => Results.Ok(new { service = "T Dining API", frontend = "Run the Angular app from frontend/" }));

app.MapGet("/tables", async (ITableRepository tableRepository, CancellationToken ct) =>
{
    var tables = await tableRepository.ListAsync(ct);
    return Results.Ok(tables.Select(t => new { t.Code, t.Seats, status = t.Status.ToString() }));
});

app.MapPatch("/tables/{tableCode}/status", async (string tableCode, UpdateTableStatusRequest request, ITableRepository tableRepository, CancellationToken ct) =>
{
    var table = await tableRepository.GetByCodeAsync(tableCode, ct);
    if (table is null) return Results.NotFound(new { error = "Table not found." });

    table.UpdateStatus(request.Status);
    return Results.Ok(new { table.Code, table.Seats, status = table.Status.ToString() });
});

app.MapGet("/menu", async (IMenuRepository menuRepository, CancellationToken ct) =>
{
    var menu = await menuRepository.ListAsync(ct);
    return Results.Ok(menu.Select(m => new { m.Id, m.Name, m.Category, m.PriceVnd, m.IsAvailable }));
});

app.MapGet("/inventory", async (IInventoryRepository inventoryRepository, CancellationToken ct) =>
{
    var inventory = await inventoryRepository.ListAsync(ct);
    return Results.Ok(inventory.Select(i => new { i.Id, i.Name, i.Unit, i.Quantity }));
});

app.MapGet("/orders", async (IOrderUseCases useCases, CancellationToken ct) => Results.Ok(await useCases.ListOrdersAsync(ct)));

app.MapPost("/orders", async (CreateOrderCommand command, IOrderUseCases useCases, CancellationToken ct) =>
{
    return await Execute(async () => Results.Created("/orders", await useCases.CreateOrderAsync(command, ct)));
});

app.MapPost("/orders/{orderId:guid}/items/add", async (Guid orderId, UpdateOrderItemCommand command, IOrderUseCases useCases, CancellationToken ct) =>
    await Execute(async () => Results.Ok(await useCases.AddItemAsync(orderId, command, ct))));

app.MapPost("/orders/{orderId:guid}/items/remove", async (Guid orderId, UpdateOrderItemCommand command, IOrderUseCases useCases, CancellationToken ct) =>
    await Execute(async () => Results.Ok(await useCases.RemoveItemAsync(orderId, command, ct))));

app.MapPost("/orders/{orderId:guid}/send-to-kitchen", async (Guid orderId, IOrderUseCases useCases, CancellationToken ct) =>
    await Execute(async () => Results.Ok(await useCases.SendToKitchenAsync(orderId, ct))));

app.MapPost("/orders/{orderId:guid}/mark-served", async (Guid orderId, IOrderUseCases useCases, CancellationToken ct) =>
    await Execute(async () => Results.Ok(await useCases.MarkServedAsync(orderId, ct))));

app.MapPost("/orders/{orderId:guid}/payments", async (Guid orderId, ProcessPaymentCommand command, IOrderUseCases useCases, CancellationToken ct) =>
    await Execute(async () => Results.Ok(await useCases.ProcessPaymentAsync(orderId, command, ct))));

app.MapPost("/orders/{orderId:guid}/close", async (Guid orderId, IOrderUseCases useCases, CancellationToken ct) =>
    await Execute(async () => Results.Ok(await useCases.CloseOrderAsync(orderId, ct))));

app.MapGet("/reservations", async (IReservationUseCases useCases, CancellationToken ct) => Results.Ok(await useCases.ListReservationsAsync(ct)));
app.MapPost("/reservations", async (CreateReservationCommand command, IReservationUseCases useCases, CancellationToken ct) =>
    await Execute(async () => Results.Created("/reservations", await useCases.CreateReservationAsync(command, ct))));

app.MapGet("/reports/daily/{date}", async (string date, IReportingUseCases useCases, CancellationToken ct) =>
{
    if (!DateOnly.TryParse(date, out var parsed)) return Results.BadRequest(new { error = "Date must be yyyy-MM-dd." });
    return Results.Ok(await useCases.GetDailyReportAsync(parsed, ct));
});

await app.RunAsync();

static async Task<IResult> Execute(Func<Task<IResult>> run)
{
    try
    {
        return await run();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}

public sealed record UpdateTableStatusRequest(TableStatus Status);
