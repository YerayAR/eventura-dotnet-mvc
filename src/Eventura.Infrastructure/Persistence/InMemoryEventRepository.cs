using Eventura.Domain.Entities;
using Eventura.Domain.Enums;
using Eventura.Domain.Repositories;

namespace Eventura.Infrastructure.Persistence;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Implementación in-memory del repositorio de eventos.
/// Responsabilidades: Persistir entidades Event en estructuras concurrentes y ejecutar consultas básicas.
/// Dependencias/Puertos utilizados: InMemoryDataStore como adaptador almacenado en memoria.
/// Límites (lo que NO debe hacer): Incluir lógica de negocio o exponer colecciones internas directamente.
/// Errores comunes: No respetar el estado IsCancelled en las consultas o no ordenar resultados.
/// </summary>
public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly InMemoryDataStore _store;

    public InMemoryEventRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        // Contexto: Persistencia de evento nuevo creada por EventService.
        // Intención: Guardar la entidad en el almacén in-memory.
        // Pasos: 1) Asignar entidad al diccionario por Id; 2) Completar tarea.
        // Validaciones: Se asume que el dominio ya validó la entidad.
        // Manejo de errores: ConcurrentDictionary maneja condiciones de carrera.
        _store.Events[@event.Id] = @event;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Contexto: Eliminación solicitada por el caso de uso.
        // Intención: Remover la entidad del almacén.
        // Pasos: 1) Intentar eliminar del diccionario; 2) Ignorar resultado.
        // Validaciones: Operación idempotente, sin excepciones si no existe.
        // Manejo de errores: Ninguno adicional; TryRemove controla concurrencia.
        _store.Events.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Contexto: Consultas puntuales desde servicios.
        // Intención: Devolver entidad si existe.
        // Pasos: 1) TryGetValue; 2) Retornar Task con resultado.
        // Validaciones: N/A, solo lectura.
        // Manejo de errores: Retorna null cuando no existe.
        _store.Events.TryGetValue(id, out var @event);
        return Task.FromResult(@event);
    }

    public Task<IReadOnlyList<Event>> GetUpcomingAsync(DateTimeOffset from, CancellationToken cancellationToken = default)
    {
#region Aprendizaje
// Repositorio como adaptador: mapea queries de alto nivel a filtros in-memory.
// En implementaciones reales se aplican proyecciones y transacciones unitarias.
// TODO(aprendizaje): implementar paginacion y pruebas de integracion para asegurar orden cronologico.
#endregion
        var events = _store.Events.Values
            .Where(e => e.StartDateTime >= from && !e.IsCancelled)
            .OrderBy(e => e.StartDateTime)
            .ToList();
        return Task.FromResult<IReadOnlyList<Event>>(events);
    }

    public Task<IReadOnlyList<Event>> SearchAsync(string? city, EventCategory? category, CancellationToken cancellationToken = default)
    {
        // Contexto: Consulta para recomendaciones basada en filtros ligeros.
        // Intención: Filtrar por ciudad y categoría.
        // Pasos: 1) Iniciar con todos los eventos; 2) Aplicar filtro de ciudad; 3) Filtro de categoría; 4) Excluir cancelados; 5) Ordenar.
        // Validaciones: Comparaciones case-insensitive para ciudad.
        // Manejo de errores: No lanza; retorna lista vacía cuando no hay coincidencias.
        IEnumerable<Event> query = _store.Events.Values;

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(e => e.Location.City.Equals(city, StringComparison.OrdinalIgnoreCase));
        }

        if (category.HasValue)
        {
            query = query.Where(e => e.Category == category);
        }

        var result = query
            .Where(e => !e.IsCancelled)
            .OrderBy(e => e.StartDateTime)
            .ToList();

        return Task.FromResult<IReadOnlyList<Event>>(result);
    }

    public Task UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        // Contexto: Persistencia de cambios en un evento existente.
        // Intención: Reemplazar la entidad almacenada con la versión actualizada.
        // Pasos: 1) Asignar entidad al diccionario; 2) Completar tarea.
        // Validaciones: Se confía en invariantes del dominio antes de llegar aquí.
        // Manejo de errores: Operación idempotente respecto a versiones previas.
        _store.Events[@event.Id] = @event;
        return Task.CompletedTask;
    }
}
