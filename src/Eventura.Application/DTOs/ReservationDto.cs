namespace Eventura.Application.DTOs;

#region Aprendizaje
// Estos DTOs viajan hacia/desde la capa Web. Permiten exponer datos de reserva sin filtrar la entidad.
// Mantienen controladores delgados: el controlador recibe CreateReservationRequest y entrega al caso de uso.
// TODO(aprendizaje): extender el DTO para incluir detalles de evento solo cuando sea necesario (evitar over-fetching).
#endregion

/// <summary>
/// Capa: Application.
/// Propósito: Representar datos de una reserva consumidos por la capa Web.
/// Responsabilidades: Transportar información inmutable sobre la reserva.
/// Dependencias/Puertos utilizados: Ninguno.
/// Límites (lo que NO debe hacer): Incluir lógica de cancelación o validaciones complejas.
/// Errores comunes: Usar esta clase para persistencia directa en infraestructura.
/// </summary>
public sealed record ReservationDto
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public int Quantity { get; init; }
    public DateTimeOffset ReservedAt { get; init; }
    public bool IsCancelled { get; init; }
}

/// <summary>
/// Capa: Application.
/// Propósito: DTO de entrada para crear reservas desde la Web.
/// Responsabilidades: Enviar datos mínimos al servicio de aplicación.
/// Dependencias/Puertos utilizados: Ninguno.
/// Límites (lo que NO debe hacer): Realizar validaciones; delegar a validators especializados.
/// Errores comunes: Aceptar cantidades negativas sin pasar por validación.
/// </summary>
public sealed record CreateReservationRequest
{
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public int Quantity { get; init; }
}
