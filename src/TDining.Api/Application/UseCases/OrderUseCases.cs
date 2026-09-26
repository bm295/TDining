using TDining.Api.Application.DTOs;
using TDining.Api.Application.Ports.In;
using TDining.Api.Application.Ports.Out;
using TDining.Api.Domain.Entities;
using TDining.Api.Domain.Services;

namespace TDining.Api.Application.UseCases;

public sealed class OrderUseCases(
    IOrderRepository orderRepository,
    ITableRepository tableRepository,
    IMenuRepository menuRepository,
    IInventoryRepository inventoryRepository,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    InventoryConsumptionService consumptionService) : IOrderUseCases
{
    public async Task<OrderDto> CreateOrderAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var table = await tableRepository.GetByCodeAsync(command.TableCode, cancellationToken)
            ?? throw new InvalidOperationException($"Table '{command.TableCode}' was not found.");

        var order = new Order(Guid.NewGuid(), table.Code, command.CustomerName.Trim(), DateTime.UtcNow);
        foreach (var line in command.Items)
        {
            var menu = await menuRepository.GetByIdAsync(line.MenuItemId, cancellationToken)
                ?? throw new InvalidOperationException($"Menu item '{line.MenuItemId}' does not exist.");
            order.AddItem(menu.Id, menu.Name, line.Quantity, menu.PriceVnd);
        }

        order.RecordCreatedEvent();
        table.UpdateStatus(TableStatus.Occupied);
        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> AddItemAsync(Guid orderId, UpdateOrderItemCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken) ?? throw new InvalidOperationException("Order not found.");
        var menu = await menuRepository.GetByIdAsync(command.MenuItemId, cancellationToken) ?? throw new InvalidOperationException("Menu item not found.");

        order.AddItem(menu.Id, menu.Name, command.Quantity, menu.PriceVnd);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> RemoveItemAsync(Guid orderId, UpdateOrderItemCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken) ?? throw new InvalidOperationException("Order not found.");
        order.RemoveItem(command.MenuItemId, command.Quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> SendToKitchenAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken) ?? throw new InvalidOperationException("Order not found.");
        order.SendToKitchen();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<OrderDto> MarkServedAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken) ?? throw new InvalidOperationException("Order not found.");
        order.MarkServed();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(order);
    }

    public async Task<PaymentResultDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken) ?? throw new InvalidOperationException("Order not found.");
        order.ApplyPayment(command.AmountVnd);

        var allMenu = await menuRepository.ListAsync(cancellationToken);
        var consumption = consumptionService.CalculateConsumption(order, allMenu);
        foreach (var (ingredientId, amount) in consumption)
        {
            var item = await inventoryRepository.GetByIdAsync(ingredientId, cancellationToken)
                ?? throw new InvalidOperationException($"Inventory item '{ingredientId}' not found.");
            item.Deduct(amount);
        }

        var payment = new Payment(Guid.NewGuid(), order.Id, command.AmountVnd, command.Method, DateTime.UtcNow);
        await paymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PaymentResultDto(payment.Id, payment.OrderId, payment.AmountVnd, payment.Method, payment.PaidAtUtc, Math.Max(0, order.TotalAmountVnd - order.PaidAmountVnd));
    }

    public async Task<OrderDto> CloseOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken) ?? throw new InvalidOperationException("Order not found.");
        order.Close();

        var table = await tableRepository.GetByCodeAsync(order.TableCode, cancellationToken);
        table?.UpdateStatus(TableStatus.Cleaning);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(order);
    }

    public async Task<IReadOnlyCollection<OrderDto>> ListOrdersAsync(CancellationToken cancellationToken)
    {
        var orders = await orderRepository.ListAsync(cancellationToken);
        return orders.Select(ToDto).ToList();
    }

    private static OrderDto ToDto(Order order) =>
        new(order.Id, order.TableCode, order.CustomerName, order.Status.ToString(), order.CreatedAtUtc, order.TotalAmountVnd, order.PaidAmountVnd,
            order.Lines.Select(l => new OrderLineDto(l.MenuItemId, l.MenuItemName, l.Quantity, l.UnitPriceVnd, l.LineTotalVnd)).ToList());
}
