using TDining.Api.Application.Ports.Out;
using TDining.Api.Application.UseCases;
using TDining.Api.Domain.Entities;

namespace TDining.Api.Tests.Application.UseCases;

public sealed class ReportingUseCasesTests
{
    [Fact]
    public async Task GetDailyReportAsync_ReturnsSalesAndOrderCountsForDate()
    {
        var reportDate = new DateOnly(2026, 7, 1);
        var closedOrder = CreateOrder(new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc), 95_000m);
        closedOrder.ApplyPayment(95_000m);
        closedOrder.Close();
        var activeOrder = CreateOrder(new DateTime(2026, 7, 1, 19, 0, 0, DateTimeKind.Utc), 70_000m);
        var otherDayOrder = CreateOrder(new DateTime(2026, 7, 2, 12, 0, 0, DateTimeKind.Utc), 45_000m);
        var useCases = new ReportingUseCases(new InMemoryOrderRepository(closedOrder, activeOrder, otherDayOrder));

        var result = await useCases.GetDailyReportAsync(reportDate, CancellationToken.None);

        Assert.Equal(reportDate, result.Date);
        Assert.Equal(2, result.TotalOrders);
        Assert.Equal(165_000m, result.GrossSalesVnd);
        Assert.Equal(1, result.ClosedOrders);
        Assert.Equal(1, result.ActiveOrders);
    }

    private static Order CreateOrder(DateTime createdAtUtc, decimal priceVnd)
    {
        var order = new Order(Guid.NewGuid(), "T1", "Linh", createdAtUtc);
        order.AddItem(Guid.NewGuid(), "Pho", 1, priceVnd);
        return order;
    }

    private sealed class InMemoryOrderRepository(params Order[] orders) : IOrderRepository
    {
        private readonly List<Order> _orders = [.. orders];

        public Task AddAsync(Order order, CancellationToken cancellationToken)
        {
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken) =>
            Task.FromResult(_orders.FirstOrDefault(order => order.Id == orderId));

        public Task<IReadOnlyCollection<Order>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<Order>>(_orders);
    }
}
