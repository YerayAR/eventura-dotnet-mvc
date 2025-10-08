using System.Text.RegularExpressions;
using Eventura.Domain.Exceptions;

namespace Eventura.Domain.ValueObjects;

/// <summary>
/// Capa: Domain.
/// Propósito: Representar direcciones de correo con validación y normalización.
/// Responsabilidades: Garantizar formato correcto y comportamiento de valor.
/// Dependencias/Puertos utilizados: Usa Regex para validación y excepciones de dominio para feedback.
/// Límites (lo que NO debe hacer): No almacenar metadatos de infraestructura ni enviar correos.
/// Errores comunes: Permitir strings en blanco o no normalizar espacios.
/// </summary>
public sealed class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = new(
        "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

#region Aprendizaje
// Value object esencial para mantener consistencia de emails en todo el dominio.
// Se crea a partir de strings pero expone validación centralizada, evitando duplicar expresiones regulares.
// TODO(aprendizaje): añadir validaciones de dominios bloqueados y probar reglas de internacionalización.
#endregion

    public static EmailAddress Create(string value)
    {
        // Contexto: Creación de emails desde servicios o entidades.
        // Intención: Validar formato y sanitizar entrada.
        // Pasos: 1) Verificar cadena no vacía; 2) Recortar espacios; 3) Validar con regex; 4) Instanciar value object.
        // Validaciones: Regex verifica estructura básica; más reglas pueden añadirse según negocio.
        // Manejo de errores: DomainValidationException con mensajes específicos ante formato inválido.
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException("Email cannot be empty.");
        }

        var trimmed = value.Trim();
        if (!EmailRegex.IsMatch(trimmed))
        {
            throw new DomainValidationException("Email format is invalid.");
        }

        return new EmailAddress(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        // Contexto: Igualdad estructural requerida por ValueObject.
        // Intención: Asegurar comparación por contenido.
        // Pasos: 1) Retornar el valor normalizado.
        // Validaciones: N/A, se realizó en creación.
        // Manejo de errores: N/A, yield simple.
        yield return Value;
    }

    public override string ToString()
    {
        // Contexto: Representación de texto para logs o vistas.
        // Intención: Exponer valor subyacente sin mutaciones.
        // Pasos: 1) Retornar string almacenado.
        // Validaciones: No aplica.
        // Manejo de errores: No lanza.
        return Value;
    }
}
