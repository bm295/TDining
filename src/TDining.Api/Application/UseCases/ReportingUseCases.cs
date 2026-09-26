using TDining.Api.Application.DTOs;
using TDining.Api.Application.Ports.In;
using TDining.Api.Application.Ports.Out;
using TDining.Api.Domain.Entities;

namespace TDining.Api.Application.UseCases;

public sealed class ReportingUseCases(IOrderRepository orderRepository) : IReportingUseCases
{
    public async Task<DailyReportDto> GetDailyReportAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.ListAsync(cancellationToken);
        var selected = orders.Where(o => DateOnly.FromDateTime(o.CreatedAtUtc.ToLocalTime()) == date).ToList();

        return new DailyReportDto(
            date,
            selected.Count,
            selected.Sum(o => o.TotalAmountVnd),
            selected.Count(o => o.Status == OrderStatus.Closed),
            selected.Count(o => o.Status is OrderStatus.New or OrderStatus.Preparing or OrderStatus.Served));
    }
}
