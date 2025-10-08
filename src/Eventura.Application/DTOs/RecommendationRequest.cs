using Eventura.Domain.Enums;

namespace Eventura.Application.DTOs;

/// <summary>
/// Capa: Application.
/// Propósito: DTO para solicitar recomendaciones de eventos.
/// Responsabilidades: Transportar filtros opcionales (ciudad y categoría).
/// Dependencias/Puertos utilizados: Usa EventCategory de dominio.
/// Límites (lo que NO debe hacer): Incluir lógica de filtrado; eso vive en servicios y repositorios.
/// Errores comunes: No normalizar ciudad antes de enviar.
/// </summary>
public sealed record RecommendationRequest
{
    public string City { get; init; } = string.Empty;
    public EventCategory? Category { get; init; }
}
