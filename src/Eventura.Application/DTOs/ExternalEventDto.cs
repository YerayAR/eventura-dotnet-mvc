namespace Eventura.Application.DTOs;

/// <summary>
/// Capa: Application.
/// Propósito: DTO para eventos obtenidos desde APIs externas.
/// Responsabilidades: Representar datos de eventos externos antes de ser convertidos a entidades del dominio.
/// Dependencias/Puertos utilizados: Ninguna, DTO puro de transporte.
/// Límites (lo que NO debe hacer): No debe contener lógica de negocio ni validaciones complejas.
/// Errores comunes: Asumir que todos los campos estarán presentes desde la API externa.
/// </summary>
public sealed record ExternalEventDto
{
    public string? Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public DateTimeOffset? StartDateTime { get; init; }
    public DateTimeOffset? EndDateTime { get; init; }
    public string City { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public string Venue { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? TicketUrl { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public bool IsFree { get; init; }
    public int? AttendeeCount { get; init; }
    public string Source { get; init; } = string.Empty;
}