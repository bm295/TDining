using Microsoft.Extensions.DependencyInjection;
using TDining.Api.Application.Ports.In;
using TDining.Api.Application.UseCases;
using TDining.Api.Composition;

namespace TDining.Api.Tests.Composition;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTDiningApplication_RegistersEveryInboundPortWithItsUseCase()
    {
        var services = new ServiceCollection();

        services.AddTDiningApplication();

        AssertScoped<IOrderUseCases, OrderUseCases>(services);
        AssertScoped<IReservationUseCases, ReservationUseCases>(services);
        AssertScoped<IReportingUseCases, ReportingUseCases>(services);
        AssertScoped<IRestaurantOperationsUseCases, RestaurantOperationsUseCases>(services);
    }

    private static void AssertScoped<TPort, TImplementation>(IServiceCollection services)
    {
        var registration = Assert.Single(services, descriptor => descriptor.ServiceType == typeof(TPort));
        Assert.Equal(typeof(TImplementation), registration.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
    }
}
