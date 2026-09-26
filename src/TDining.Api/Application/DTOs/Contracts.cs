using TDining.Api.Domain.Entities;

namespace TDining.Api.Application.DTOs;

public sealed record CreateOrderLineCommand(Guid MenuItemId, int Quantity);
public sealed record CreateOrderCommand(string TableCode, string CustomerName, List<CreateOrderLineCommand> Items);
public sealed record UpdateOrderItemCommand(Guid MenuItemId, int Quantity);
public sealed record ProcessPaymentCommand(decimal AmountVnd, PaymentMethod Method);
public sealed record CreateReservationCommand(string CustomerName, string PhoneNumber, int GuestCount, DateTime BookingTimeUtc, string? Note);

public sealed record OrderLineDto(Guid MenuItemId, string MenuItemName, int Quantity, decimal UnitPriceVnd, decimal LineTotalVnd);
public sealed record OrderDto(Guid Id, string TableCode, string CustomerName, string Status, DateTime CreatedAtUtc, decimal TotalAmountVnd, decimal PaidAmountVnd, List<OrderLineDto> Lines);
public sealed record PaymentResultDto(Guid PaymentId, Guid OrderId, decimal AmountVnd, PaymentMethod Method, DateTime PaidAtUtc, decimal RemainingBalanceVnd);
public sealed record ReservationDto(Guid Id, string CustomerName, string PhoneNumber, int GuestCount, DateTime BookingTimeUtc, string Status, string? Note);
public sealed record DailyReportDto(DateOnly Date, int TotalOrders, decimal GrossSalesVnd, int ClosedOrders, int ActiveOrders);
public sealed record DiningTableDto(string Code, int Seats, string Status);
public sealed record MenuItemDto(Guid Id, string Name, string Category, decimal PriceVnd, bool IsAvailable);
public sealed record InventoryItemDto(Guid Id, string Name, string Unit, decimal Quantity);
