using TDining.Api.Application.DTOs;
using TDining.Api.Application.Ports.Out;
using TDining.Api.Application.UseCases;
using TDining.Api.Domain.Entities;

namespace TDining.Api.Tests.Application.UseCases;

public sealed class ReservationUseCasesTests
{
    [Fact]
    public async Task CreateReservationAsync_WhenCapacityExceeded_ReturnsValidationFailure()
    {
        var bookingTimeUtc = new DateTime(2026, 7, 1, 19, 0, 0, DateTimeKind.Utc);
        var existingReservation = new Reservation(
            Guid.NewGuid(),
            "Linh",
            "0900000000",
            68,
            bookingTimeUtc.AddHours(-1),
            ReservationStatus.Confirmed,
            null);
        var reservationRepository = new InMemoryReservationRepository(existingReservation);
        var unitOfWork = new CountingUnitOfWork();
        var useCases = CreateUseCases(reservationRepository, unitOfWork);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCases.CreateReservationAsync(
                new CreateReservationCommand("Mai", "0911111111", 3, bookingTimeUtc, null),
                CancellationToken.None));

        Assert.Equal("Reservation exceeds configured seating capacity.", exception.Message);
        Assert.Single(reservationRepository.Reservations);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenInputHasWhitespace_TrimsCustomerAndPhone()
    {
        var reservationRepository = new InMemoryReservationRepository();
        var unitOfWork = new CountingUnitOfWork();
        var useCases = CreateUseCases(reservationRepository, unitOfWork);

        var result = await useCases.CreateReservationAsync(
            new CreateReservationCommand("  Linh Nguyen  ", "  0900000000  ", 4, new DateTime(2026, 7, 1, 19, 0, 0, DateTimeKind.Utc), "  Window seat  "),
            CancellationToken.None);

        Assert.Equal("Linh Nguyen", result.CustomerName);
        Assert.Equal("0900000000", result.PhoneNumber);
        Assert.Equal("Window seat", result.Note);
        var reservation = Assert.Single(reservationRepository.Reservations);
        Assert.Equal("Linh Nguyen", reservation.CustomerName);
        Assert.Equal("0900000000", reservation.PhoneNumber);
        Assert.Equal("Window seat", reservation.Note);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    private static ReservationUseCases CreateUseCases(
        InMemoryReservationRepository? reservationRepository = null,
        CountingUnitOfWork? unitOfWork = null) =>
        new(reservationRepository ?? new InMemoryReservationRepository(), unitOfWork ?? new CountingUnitOfWork());

    private sealed class InMemoryReservationRepository(params Reservation[] reservations) : IReservationRepository
    {
        private readonly List<Reservation> _reservations = [.. reservations];
        public IReadOnlyCollection<Reservation> Reservations => _reservations;

        public Task AddAsync(Reservation reservation, CancellationToken cancellationToken)
        {
            _reservations.Add(reservation);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Reservation>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<Reservation>>(_reservations);
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
