using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TDining.Api.Application.DTOs;
using TDining.Api.Domain.Entities;

namespace TDining.Api.Tests.Api;

public sealed class ApiEndpointTests
{
    [Fact]
    public async Task GetMenu_ReturnsSeededMenu()
    {
        await using var factory = new TDiningApiFactory();
        using var client = factory.CreateClient();

        var menu = await client.GetFromJsonAsync<List<MenuItemResponse>>("/menu");

        Assert.NotNull(menu);
        Assert.Contains(menu, item => item.Name == "Pho bo tai" && item.Category == "Main" && item.PriceVnd == 95_000m && item.IsAvailable);
        Assert.Contains(menu, item => item.Name == "Goi cuon tom thit" && item.Category == "Starter" && item.PriceVnd == 70_000m && item.IsAvailable);
        Assert.Contains(menu, item => item.Name == "Ca phe sua da" && item.Category == "Beverage" && item.PriceVnd == 45_000m && item.IsAvailable);
    }

    [Fact]
    public async Task PostOrder_WithInvalidMenuItem_ReturnsBadRequestProblemDetails()
    {
        await using var factory = new TDiningApiFactory();
        using var client = factory.CreateClient();
        var missingMenuItemId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync(
            "/orders",
            new CreateOrderCommand("T1", "Linh", [new CreateOrderLineCommand(missingMenuItemId, 1)]));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.NotNull(problem);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal(400, problem.Status);
        Assert.Equal($"Menu item '{missingMenuItemId}' does not exist.", problem.Detail);
    }

    [Fact]
    public async Task PatchTableStatus_PersistsStatusAcrossRequests()
    {
        await using var factory = new TDiningApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            "/tables/T1/status",
            new UpdateTableStatusRequest(TableStatus.Cleaning));
        var tables = await client.GetFromJsonAsync<List<DiningTableResponse>>("/tables");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(tables);
        Assert.Contains(tables, table => table.Code == "T1" && table.Status == "Cleaning");
    }

    private sealed class TDiningApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"tdining-api-tests-{Guid.NewGuid():N}.db");

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = $"Data Source={_databasePath}"
                });
            });

            return base.CreateHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
    }

    private sealed record MenuItemResponse(Guid Id, string Name, string Category, decimal PriceVnd, bool IsAvailable);
    private sealed record DiningTableResponse(string Code, int Seats, string Status);
    private sealed record ProblemDetailsResponse(string? Title, int? Status, string? Detail);
}
