using Eventura.Application.DTOs;
using Eventura.Application.Validators;
using Eventura.Domain.Enums;
using Xunit;

namespace Eventura.UnitTests.Application;

/// <summary>
/// Escenario de aprendizaje: validaciones a nivel de casos de uso antes de llegar al dominio.
/// Conceptos: Validación previa, feedback funcional y protección contra datos inválidos.
/// </summary>
public class CreateEventRequestValidatorTests
{
    [Fact]
    public void Validate_WithPastDate_ReturnsFailure()
    {
        // Given: solicitud de evento cuya fecha está en el pasado (violando reglas temporales).
        // When: se ejecuta CreateEventRequestValidator.Validate.
        // Then: OperationResult indica fallo y evita que el flujo avance (Concepto: validación funcional).
        var request = new CreateEventRequest
        {
            Title = "Test",
            Description = "Desc",
            StartDateTime = DateTimeOffset.UtcNow.AddDays(-1),
            Duration = TimeSpan.FromHours(1),
            City = "Madrid",
            AddressLine = "Gran Via",
            Capacity = 10,
            Category = EventCategory.Music
        };

        var result = CreateEventRequestValidator.Validate(request, DateTimeOffset.UtcNow);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Validate_WithValidRequest_ReturnsSuccess()
    {
        // Given: solicitud de evento consistente con las reglas (fechas futuras, campos obligatorios).
        // When: el validador procesa la solicitud en un contexto actual.
        // Then: la operación es exitosa permitiendo continuar con el caso de uso (Concepto: validación previa a dominio).
        var request = new CreateEventRequest
        {
            Title = "Test",
            Description = "Desc",
            StartDateTime = DateTimeOffset.UtcNow.AddDays(1),
            Duration = TimeSpan.FromHours(1),
            City = "Madrid",
            AddressLine = "Gran Via",
            Capacity = 10,
            Category = EventCategory.Music
        };

        var result = CreateEventRequestValidator.Validate(request, DateTimeOffset.UtcNow);
        Assert.True(result.Succeeded);
    }
}

