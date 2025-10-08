using Eventura.Domain.Entities;
using Eventura.Domain.Repositories;

namespace Eventura.Infrastructure.Persistence;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Implementar el repositorio de reservas en memoria.
/// Responsabilidades: Guardar, recuperar y actualizar entidades Reservation.
/// Dependencias/Puertos utilizados: InMemoryDataStore como almacenamiento subyacente.
/// Límites (lo que NO debe hacer): Aplicar lógica de dominio o transacciones distribuidas.
/// Errores comunes: No filtrar por usuario/evento correctamente o crear referencias circulares.
/// </summary>
public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly InMemoryDataStore _store;

    public InMemoryReservationRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        // Contexto: Registro de reserva desde el caso de uso.
        // Intención: Persistir la reserva en la colección in-memory.
        // Pasos: 1) Asignar reserva al diccionario por Id; 2) Completar tarea.
        // Validaciones: Se asume reserva validada por el dominio.
        // Manejo de errores: ConcurrentDictionary controla condiciones de carrera.
        _store.Reservations[reservation.Id] = reservation;
        return Task.CompletedTask;
    }

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Contexto: Recuperación individual utilizada en cancelaciones.
        // Intención: Obtener reserva por identificador.
        // Pasos: 1) TryGetValue; 2) Retornar resultado.
        // Validaciones: N/A.
        // Manejo de errores: Retorna null cuando no existe.
        _store.Reservations.TryGetValue(id, out var reservation);
        return Task.FromResult(reservation);
    }

    public Task<IReadOnlyList<Reservation>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
#region Aprendizaje
// Este método admite Guid.Empty como comodín para devolver todas las reservas, útil para métricas.
// En implementaciones reales se aplicaría paginación y filtros adicionales, potencialmente con transacciones.
// TODO(aprendizaje): incluir filtro por fecha de reserva y pruebas para escenarios de concurrencia.
#endregion
        IEnumerable<Reservation> query = _store.Reservations.Values;
        if (eventId != Guid.Empty)
        {
            query = query.Where(r => r.EventId == eventId);
        }

        return Task.FromResult<IReadOnlyList<Reservation>>(query.ToList());
    }

    public Task<IReadOnlyList<Reservation>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Contexto: Listado de reservas de un usuario.
        // Intención: Obtener colecciones filtradas por UserId.
        // Pasos: 1) Filtrar valores por UserId; 2) Convertir a lista; 3) Retornar como IReadOnlyList.
        // Validaciones: N/A; si userId no existe se retorna lista vacía.
        // Manejo de errores: Determinista; sin excepciones.
        var reservations = _store.Reservations.Values
            .Where(r => r.UserId == userId)
            .ToList();
        return Task.FromResult<IReadOnlyList<Reservation>>(reservations);
    }

    public Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        // Contexto: Actualización tras cancelar o modificar una reserva.
        // Intención: Reemplazar estado existente por la versión actual.
        // Pasos: 1) Reasignar reserva en diccionario; 2) Completar tarea.
        // Validaciones: El dominio garantiza consistencia previa.
        // Manejo de errores: Idempotente frente a múltiples llamadas.
        _store.Reservations[reservation.Id] = reservation;
        return Task.CompletedTask;
    }
}
