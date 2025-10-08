using System.ComponentModel.DataAnnotations;

namespace Eventura.Web.Models;

/// <summary>
/// Capa: Web.
/// Proposito: Recibir datos del formulario de reserva.
/// Responsabilidades: Transportar EventId y cantidad desde la vista hacia el controlador.
/// Dependencias/Puertos utilizados: Ninguna adicional.
/// Limites (lo que NO debe hacer): Implementar reglas de negocio; se delega a servicios de Application.
/// Errores comunes: No marcar EventId como Required ocasionando errores silenciosos.
/// </summary>
public sealed class ReservationFormViewModel
{
    [Required]
    public Guid EventId { get; init; }

    [Required]
    public int Quantity { get; init; } = 1;
}

/// <summary>
/// Capa: Web.
/// Proposito: Mostrar reservas en listados personales.
/// Responsabilidades: Exponer datos amigables (titulo de evento, fecha, cantidad).
/// Dependencias/Puertos utilizados: Se rellena con datos de IReservationService e IEventService.
/// Limites (lo que NO debe hacer): Guardar referencias a entidades de dominio.
/// Errores comunes: No mostrar IsCancelled y confundir a usuarios sobre el estado de su reserva.
/// </summary>
public sealed class ReservationListItemViewModel
{
    public Guid Id { get; init; }
    public string EventTitle { get; init; } = string.Empty;
    public DateTimeOffset ReservedAt { get; init; }
    public int Quantity { get; init; }
    public bool IsCancelled { get; init; }
}
