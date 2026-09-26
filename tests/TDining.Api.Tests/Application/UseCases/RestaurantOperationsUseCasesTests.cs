using TDining.Api.Application.Ports.Out;
using TDining.Api.Application.UseCases;
using TDining.Api.Domain.Entities;

namespace TDining.Api.Tests.Application.UseCases;

public sealed class RestaurantOperationsUseCasesTests
{
    [Fact]
    public async Task ListOperations_ReturnApplicationDtosWithoutExposingDomainEntities()
    {
        var ingredient = new InventoryItem(Guid.NewGuid(), "Noodles", "g", 500m);
        var menuItem = new MenuItem(Guid.NewGuid(), "Pho", "Main", 75_000m, true, []);
        var table = new DiningTable("A1", 4);
        var useCases = CreateUseCases([table], [menuItem], [ingredient]);

        var tables = await useCases.ListTablesAsync(CancellationToken.None);
        var menu = await useCases.ListMenuAsync(CancellationToken.None);
        var inventory = await useCases.ListInventoryAsync(CancellationToken.None);

        var tableDto = Assert.Single(tables);
        Assert.Equal(("A1", 4, "Available"), (tableDto.Code, tableDto.Seats, tableDto.Status));
        var menuDto = Assert.Single(menu);
        Assert.Equal((menuItem.Id, "Pho", "Main", 75_000m, true), (menuDto.Id, menuDto.Name, menuDto.Category, menuDto.PriceVnd, menuDto.IsAvailable));
        var inventoryDto = Assert.Single(inventory);
        Assert.Equal((ingredient.Id, "Noodles", "g", 500m), (inventoryDto.Id, inventoryDto.Name, inventoryDto.Unit, inventoryDto.Quantity));
    }

    [Fact]
    public async Task UpdateTableStatusAsync_WhenTableExists_UpdatesAndCommitsOnce()
    {
        var table = new DiningTable("A1", 4);
        var unitOfWork = new CountingUnitOfWork();
        var useCases = CreateUseCases([table], unitOfWork: unitOfWork);

        var result = await useCases.UpdateTableStatusAsync("A1", TableStatus.Cleaning, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Cleaning", result.Status);
        Assert.Equal(TableStatus.Cleaning, table.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateTableStatusAsync_WhenTableIsMissing_DoesNotCommit()
    {
        var unitOfWork = new CountingUnitOfWork();
        var useCases = CreateUseCases(unitOfWork: unitOfWork);

        var result = await useCases.UpdateTableStatusAsync("missing", TableStatus.Cleaning, CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    private static RestaurantOperationsUseCases CreateUseCases(
        DiningTable[]? tables = null,
        MenuItem[]? menu = null,
        InventoryItem[]? inventory = null,
        CountingUnitOfWork? unitOfWork = null) =>
        new(new TableRepository(tables ?? []), new MenuRepository(menu ?? []),
            new InventoryRepository(inventory ?? []), unitOfWork ?? new CountingUnitOfWork());

    private sealed class TableRepository(DiningTable[] tables) : ITableRepository
    {
        public Task<DiningTable?> GetByCodeAsync(string tableCode, CancellationToken cancellationToken) =>
            Task.FromResult(tables.FirstOrDefault(table => table.Code == tableCode));
        public Task<IReadOnlyCollection<DiningTable>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<DiningTable>>(tables);
    }

    private sealed class MenuRepository(MenuItem[] items) : IMenuRepository
    {
        public Task<IReadOnlyCollection<MenuItem>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<MenuItem>>(items);
        public Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(items.FirstOrDefault(item => item.Id == id));
    }

    private sealed class InventoryRepository(InventoryItem[] items) : IInventoryRepository
    {
        public Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(items.FirstOrDefault(item => item.Id == id));
        public Task<IReadOnlyCollection<InventoryItem>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<InventoryItem>>(items);
    }

    private sealed class CountingUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }
        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }
}
