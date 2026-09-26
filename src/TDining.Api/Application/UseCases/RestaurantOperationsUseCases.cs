using TDining.Api.Application.DTOs;
using TDining.Api.Application.Ports.In;
using TDining.Api.Application.Ports.Out;
using TDining.Api.Domain.Entities;

namespace TDining.Api.Application.UseCases;

public sealed class RestaurantOperationsUseCases(
    ITableRepository tableRepository,
    IMenuRepository menuRepository,
    IInventoryRepository inventoryRepository,
    IUnitOfWork unitOfWork) : IRestaurantOperationsUseCases
{
    public async Task<IReadOnlyCollection<DiningTableDto>> ListTablesAsync(CancellationToken cancellationToken)
    {
        var tables = await tableRepository.ListAsync(cancellationToken);
        return tables.Select(ToDto).ToList();
    }

    public async Task<DiningTableDto?> UpdateTableStatusAsync(
        string tableCode,
        TableStatus status,
        CancellationToken cancellationToken)
    {
        var table = await tableRepository.GetByCodeAsync(tableCode, cancellationToken);
        if (table is null)
        {
            return null;
        }

        table.UpdateStatus(status);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(table);
    }

    public async Task<IReadOnlyCollection<MenuItemDto>> ListMenuAsync(CancellationToken cancellationToken)
    {
        var menu = await menuRepository.ListAsync(cancellationToken);
        return menu.Select(item => new MenuItemDto(item.Id, item.Name, item.Category, item.PriceVnd, item.IsAvailable)).ToList();
    }

    public async Task<IReadOnlyCollection<InventoryItemDto>> ListInventoryAsync(CancellationToken cancellationToken)
    {
        var inventory = await inventoryRepository.ListAsync(cancellationToken);
        return inventory.Select(item => new InventoryItemDto(item.Id, item.Name, item.Unit, item.Quantity)).ToList();
    }

    private static DiningTableDto ToDto(DiningTable table) =>
        new(table.Code, table.Seats, table.Status.ToString());
}
