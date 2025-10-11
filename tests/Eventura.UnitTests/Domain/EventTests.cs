using Eventura.Application.DTOs;
using Eventura.Domain.Entities;
using Eventura.Domain.Enums;
using Eventura.Domain.Exceptions;
using Eventura.Domain.ValueObjects;
using Xunit;

namespace Eventura.UnitTests.Domain;

/// <summary>
/// Escenarios de aprendizaje para validar invariantes del agregado <see cref="Event"/>.
/// Cada prueba sigue la estructura pedagógica // Given // When // Then solicitada.
/// </summary>
public sealed class EventTests
{
    [Fact]
    public void Create_WithInvalidCapacity_Throws()
    {
        // Given: un value object Location válido y una capacidad en cero.
        var location = Location.Create("Madrid", "Gran Vía 1");

        // When: se crea el evento con una capacidad inválida.
        var act = () => Event.Create(
            "Test",
            "Desc",
            DateTimeOffset.UtcNow.AddDays(1),
            TimeSpan.FromHours(1),
            location,
            0,
            EventCategory.Music);

        // Then: la fábrica lanza DomainValidationException preservando la invariante.
        Assert.Throws<DomainValidationException>(act);
    }

    [Fact]
    public void Reserve_WithAvailableCapacity_Succeeds()
    {
        // Given: un evento de capacidad diez y un usuario válido.
        var @event = CreateSampleEvent(capacity: 10);
        var user = CreateSampleUser();

        // When: se reserva una cantidad de dos plazas.
        var reservation = @event.Reserve(user, 2);

        // Then: la reserva se registra y la capacidad restante refleja la suma de cantidades.
        Assert.Equal(8, @event.RemainingCapacity);
        Assert.Equal(2, reservation.Quantity);
    }

    [Fact]
    public void Reserve_WhenExceedingRemainingCapacity_Throws()
    {
        // Given: un evento con capacidad limitada y una reserva previa.
        var @event = CreateSampleEvent(capacity: 3);
        var user = CreateSampleUser();
        @event.Reserve(user, 2);

        // When: se intenta reservar más plazas de las disponibles.
        var act = () => @event.Reserve(user, 2);

        // Then: el agregado bloquea la sobreventa y lanza DomainValidationException.
        Assert.Throws<DomainValidationException>(act);
        Assert.Equal(1, @event.RemainingCapacity);
    }

    [Fact]
    public void UpdateDetails_WithCapacityBelowExistingReservations_Throws()
    {
        // Given: un evento con reservas confirmadas.
        var @event = CreateSampleEvent(capacity: 10);
        var user = CreateSampleUser();
        @event.Reserve(user, 4);

        // When: se intenta reducir la capacidad por debajo de las reservas activas.
        var act = () => @event.UpdateDetails(
            "New title",
            "Nueva descripción",
            DateTimeOffset.UtcNow.AddDays(2),
            TimeSpan.FromHours(2),
            Location.Create("Madrid", "Gran Vía 1"),
            3,
            EventCategory.Technology);

        // Then: se lanza DomainValidationException protegiendo la invariante de capacidad.
        Assert.Throws<DomainValidationException>(act);
    }

    [Fact]
    public void CancelReservation_ReplenishesRemainingCapacity()
    {
        // Given: un evento con una reserva de dos plazas.
        var @event = CreateSampleEvent(capacity: 5);
        var user = CreateSampleUser();
        var reservation = @event.Reserve(user, 2);

        // When: se cancela la reserva desde el agregado.
        @event.CancelReservation(reservation.Id);

        // Then: la capacidad vuelve a estar disponible para nuevos asistentes.
        Assert.Equal(5, @event.RemainingCapacity);
    }

    [Fact]
    public void Cancel_MarksEventAsCancelledAndBlocksReservations()
    {
        // Given: un evento publicado y reservable.
        var @event = CreateSampleEvent();
        var user = CreateSampleUser();

        // When: el organizador lo cancela e intenta registrarse alguien más.
        @event.Cancel();
        var act = () => @event.Reserve(user, 1);

        // Then: el estado cambia y ya no se aceptan nuevas reservas.
        Assert.True(@event.IsCancelled);
        Assert.Throws<DomainValidationException>(act);
    }

    private static Event CreateSampleEvent(int capacity = 10) =>
        Event.Create(
            "Evento de prueba",
            "Descripción de ejemplo",
            DateTimeOffset.UtcNow.AddDays(2),
            TimeSpan.FromHours(2),
            Location.Create("Madrid", "Gran Vía 1"),
            capacity,
            EventCategory.Music);

    private static User CreateSampleUser() =>
        User.Create(
            "usuario",
            EmailAddress.Create("user@test.com"),
            "hash",
            Roles.User);
}
