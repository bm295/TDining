using TDining.Api.Application.DTOs;

namespace TDining.Api.Application.Ports.In;

public interface IOrderUseCases
{
    Task<OrderDto> CreateOrderAsync(CreateOrderCommand command, CancellationToken cancellationToken);
    Task<OrderDto> AddItemAsync(Guid orderId, UpdateOrderItemCommand command, CancellationToken cancellationToken);
    Task<OrderDto> RemoveItemAsync(Guid orderId, UpdateOrderItemCommand command, CancellationToken cancellationToken);
    Task<OrderDto> SendToKitchenAsync(Guid orderId, CancellationToken cancellationToken);
    Task<OrderDto> MarkServedAsync(Guid orderId, CancellationToken cancellationToken);
    Task<PaymentResultDto> ProcessPaymentAsync(Guid orderId, ProcessPaymentCommand command, CancellationToken cancellationToken);
    Task<OrderDto> CloseOrderAsync(Guid orderId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrderDto>> ListOrdersAsync(CancellationToken cancellationToken);
}

public interface IReservationUseCases
{
    Task<ReservationDto> CreateReservationAsync(CreateReservationCommand command, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReservationDto>> ListReservationsAsync(CancellationToken cancellationToken);
}

public interface IReportingUseCases
{
    Task<DailyReportDto> GetDailyReportAsync(DateOnly date, CancellationToken cancellationToken);
}
