using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TDining.Api.Application.Ports.In;
using TDining.Api.Application.Ports.Out;
using TDining.Api.Application.UseCases;
using TDining.Api.Domain.Services;
using TDining.Api.Infrastructure.Outbox;
using TDining.Api.Infrastructure.Persistence;

namespace TDining.Api.Composition;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTDiningApplication(this IServiceCollection services)
    {
        services.AddSingleton<InventoryConsumptionService>();
        services.AddScoped<IOrderUseCases, OrderUseCases>();
        services.AddScoped<IReservationUseCases, ReservationUseCases>();
        services.AddScoped<IReportingUseCases, ReportingUseCases>();
        services.AddScoped<IRestaurantOperationsUseCases, RestaurantOperationsUseCases>();

        return services;
    }

    public static IServiceCollection AddTDiningInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<TDiningDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        services.AddScoped<ITableRepository, EfTableRepository>();
        services.AddScoped<IMenuRepository, EfMenuRepository>();
        services.AddScoped<IInventoryRepository, EfInventoryRepository>();
        services.AddScoped<IPaymentRepository, EfPaymentRepository>();
        services.AddScoped<IReservationRepository, EfReservationRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<IIntegrationEventPublisher, LoggingIntegrationEventPublisher>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
