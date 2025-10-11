using Eventura.Domain.Entities;

namespace Eventura.Domain.Repositories;

/// <summary>
/// Capa: Domain.
/// Propósito: Puerto de salida para persistencia de eventos.
/// Responsabilidades: Definir operaciones asíncronas para CRUD y búsquedas.
/// Dependencias/Puertos utilizados: Retorna entidades de dominio sin acceder a frameworks concretos.
/// Límites (lo que NO debe hacer): No implementar lógica; eso corresponde a infraestructura.
/// Errores comunes: Añadir dependencias específicas de EF o filtrar DTOs desde aquí.
/// </summary>
public interface IEventRepository
{
#region Aprendizaje
// Puerto de salida: la implementación vive en Infrastructure. Permite reemplazar adaptadores (InMemory, EF, Dapper).
// TODO(aprendizaje): crear implementación basada en EF Core con transacciones y pruebas de integración.
#endregion
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetUpcomingAsync(DateTimeOffset from, CancellationToken cancellationToken = default);
    Task AddAsync(Event @event, CancellationToken cancellationToken = default);
    Task UpdateAsync(Event @event, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> SearchAsync(
        string? city,
        Eventura.Domain.Enums.EventCategory? category,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
}
