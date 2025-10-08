using Eventura.Domain.Exceptions;

namespace Eventura.Domain.ValueObjects;

/// <summary>
/// Capa: Domain.
/// Propósito: Value object que encapsula la ubicación de un evento.
/// Responsabilidades: Mantener consistencia en ciudad y dirección con igualdad basada en valores.
/// Dependencias/Puertos utilizados: No depende de infraestructura; solo de excepciones de dominio.
/// Límites (lo que NO debe hacer): No almacenar lógica de persistencia ni datos efímeros.
/// Errores comunes: Permitir mutaciones o aceptar cadenas sin sanitizar.
/// </summary>
public sealed class Location : ValueObject
{
    public string City { get; }
    public string AddressLine { get; }

    private Location(string city, string addressLine)
    {
        City = city;
        AddressLine = addressLine;
    }

#region Aprendizaje
// Este value object ilustra cómo el dominio usa tipos inmutables para garantizar consistencia.
// Comparte igualdad basada en componentes y evita propagar cadenas sin limpiar hacia otras capas.
// TODO(aprendizaje): añadir validaciones de longitud y normalización cultural con pruebas unitarias.
#endregion

    public static Location Create(string city, string addressLine)
    {
        // Contexto: Construcción de ubicaciones desde servicios o entidades de dominio.
        // Intención: Sanitizar y validar datos de dirección antes de asociarlos a eventos.
        // Pasos: 1) Verificar ciudad; 2) Verificar dirección; 3) Crear instancia inmutable.
        // Validaciones: Asegura que los campos no sean nulos ni vacíos.
        // Manejo de errores: Lanza DomainValidationException con mensajes específicos.
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new DomainValidationException("City is required.");
        }

        if (string.IsNullOrWhiteSpace(addressLine))
        {
            throw new DomainValidationException("Address is required.");
        }

        return new Location(city.Trim(), addressLine.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        // Contexto: Utilizado por ValueObject para evaluar igualdad estructural.
        // Intención: Exponer componentes que forman parte de la identidad.
        // Pasos: 1) Retornar city; 2) Retornar addressLine.
        // Validaciones: Implícitas, ya que los valores fueron validados en creación.
        // Manejo de errores: No aplica; yield determinista.
        yield return City;
        yield return AddressLine;
    }

    public override string ToString()
    {
        // Contexto: Representación legible para logs o vistas sin exponer estructura interna.
        // Intención: Componer una cadena amigable.
        // Pasos: 1) Formatear dirección y ciudad.
        // Validaciones: No aplica.
        // Manejo de errores: No lanza; concatenación segura.
        return $"{AddressLine}, {City}";
    }
}
