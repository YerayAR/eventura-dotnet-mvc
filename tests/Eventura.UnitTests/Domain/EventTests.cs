using Eventura.Domain.Entities;
using Eventura.Domain.Enums;
using Eventura.Domain.Exceptions;
using Eventura.Domain.ValueObjects;
using Xunit;

namespace Eventura.UnitTests.Domain;

/// <summary>
/// Escenario de aprendizaje: validar invariantes del agregado Event.
/// Responsabilidad: Asegurar que las reglas de dominio se mantienen al crear y reservar eventos.
/// Conceptos cubiertos: invariantes, manejo de excepciones de dominio y consistencia de capacidad.
/// </summary>
public class EventTests
{
    [Fact]
    public void Create_WithInvalidCapacity_Throws()
    {
        // Given: un value object Location válido y parámetros con capacidad cero (invariante a validar).
        // When: se invoca Event.Create con capacidad inválida.
        // Then: se espera DomainValidationException mostrando que el dominio protege sus reglas (Concepto: invariantes).
        var location = Location.Create("Madrid", "Gran Via 1");
        Assert.Throws<DomainValidationException>(() =>
            Event.Create("Test", "Desc", DateTimeOffset.UtcNow.AddDays(1), TimeSpan.FromHours(1), location, 0, EventCategory.Music));
    }

    [Fact]
    public void Reserve_WithAvailableCapacity_Succeeds()
    {
        // Given: un evento con capacidad 10 y un usuario válido (precondiciones).
        // When: se reserva una cantidad de 2 asientos.
        // Then: la reserva resulta exitosa y la capacidad restante se reduce (Concepto: invariantes + consistencia de agregados).
        var location = Location.Create("Madrid", "Gran Via 1");
        var @event = Event.Create("Test", "Desc", DateTimeOffset.UtcNow.AddDays(1), TimeSpan.FromHours(1), location, 10, EventCategory.Music);
        var user = User.Create("user", EmailAddress.Create("user@test.com"), "hash", "User");

        var reservation = @event.Reserve(user, 2);

        Assert.Equal(8, @event.RemainingCapacity);
        Assert.Equal(2, reservation.Quantity);
    }
}

