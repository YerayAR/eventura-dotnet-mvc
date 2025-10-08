using Eventura.Domain.Enums;

namespace Eventura.Application.DTOs;

#region Aprendizaje
// Los DTOs existen en Application para aislar a la capa Web de las entidades de dominio.
// Se construyen a partir de entidades (Event) pero solo exponen datos serializables.
// Diferencia clave: los DTOs no contienen reglas de negocio ni referencias a repositorios.
// TODO(aprendizaje): crear un DTO resumido para listados masivos y medir impacto en rendimiento.
#endregion

/// <summary>
/// Capa: Application.
/// Propósito: DTO para transportar datos de eventos hacia la capa Web.
/// Responsabilidades: Representar información serializable, separando dominio de presentación.
/// Dependencias/Puertos utilizados: Usa EventCategory como enumeración compartida.
/// Límites (lo que NO debe hacer): No encapsular lógica de negocio ni exponer métodos mutadores.
/// Errores comunes: Mapear directamente entidades de dominio a vistas sin pasar por este DTO.
/// </summary>
public sealed record EventDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset StartDateTime { get; init; }
    public TimeSpan Duration { get; init; }
    public string City { get; init; } = string.Empty;
    public string AddressLine { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public int RemainingCapacity { get; init; }
    public EventCategory Category { get; init; }
    public bool IsCancelled { get; init; }
}

/// <summary>
/// Capa: Application.
/// Propósito: DTO de entrada para creación de eventos desde la Web.
/// Responsabilidades: Transportar datos validados por MVC y que serán procesados por casos de uso.
/// Dependencias/Puertos utilizados: Ninguno adicional.
/// Límites (lo que NO debe hacer): No incluir lógica de negocio ni validaciones complejas; se delega a validators.
/// Errores comunes: Enviar entidades de dominio en lugar de este request.
/// </summary>
public sealed record CreateEventRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset StartDateTime { get; init; }
    public TimeSpan Duration { get; init; }
    public string City { get; init; } = string.Empty;
    public string AddressLine { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public EventCategory Category { get; init; }
}

/// <summary>
/// Capa: Application.
/// Propósito: DTO para actualizar eventos existentes reutilizando campos de creación.
/// Responsabilidades: Añadir identificador a la solicitud de actualización.
/// Dependencias/Puertos utilizados: Hereda propiedades de CreateEventRequest.
/// Límites (lo que NO debe hacer): No mezclar entidades ni exponer lógica adicional.
/// Errores comunes: Olvidar validar que el Id no sea Guid.Empty antes de usarlo.
/// </summary>
public sealed record UpdateEventRequest : CreateEventRequest
{
    public Guid Id { get; init; }
}
