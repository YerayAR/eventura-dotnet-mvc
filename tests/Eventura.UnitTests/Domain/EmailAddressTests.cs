using Eventura.Domain.Exceptions;
using Eventura.Domain.ValueObjects;
using Xunit;

namespace Eventura.UnitTests.Domain;

/// <summary>
/// Escenario: reforzar reglas del value object EmailAddress.
/// Concepto validado: invariante de formato y normalización de strings.
/// </summary>
public class EmailAddressTests
{
    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    public void Create_InvalidEmail_Throws(string email)
    {
        // Given: entradas nulas o con formato incorrecto.
        // When: se intenta construir EmailAddress.
        // Then: se lanza DomainValidationException protegiendo la integridad del value object (Concepto: invariantes).
        Assert.Throws<DomainValidationException>(() => EmailAddress.Create(email));
    }

    [Fact]
    public void Create_ValidEmail_ReturnsValue()
    {
        // Given: correo válido según reglas del dominio.
        // When: se crea el value object.
        // Then: el valor normalizado se almacena correctamente (Concepto: igualdad por valor).
        var email = EmailAddress.Create("test@example.com");
        Assert.Equal("test@example.com", email.Value);
    }
}

