using Eventura.Domain.Entities;

namespace Eventura.Domain.Repositories;

/// <summary>
/// Capa: Domain.
/// Propósito: Puerto de acceso a reservas para infraestructura.
/// Responsabilidades: Declarar operaciones de búsqueda y persistencia relacionadas con reservas.
/// Dependencias/Puertos utilizados: Maneja entidades Reservation.
/// Límites (lo que NO debe hacer): No debe contener lógica de mapeo ni dependencias a frameworks.
/// Errores comunes: Utilizar DTOs o mezclar conceptos de infraestructura en la firma.
/// </summary>
public interface IReservationRepository
{
#region Aprendizaje
// Este puerto permite aislar la persistencia de reservas. Las implementaciones deben garantizar consistencia con Event.
// TODO(aprendizaje): añadir métodos de consulta por estado y cubrir escenarios de concurrencia.
#endregion
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reservation>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Reservation>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
