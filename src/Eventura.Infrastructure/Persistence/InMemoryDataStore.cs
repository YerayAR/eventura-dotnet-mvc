using System.Collections.Concurrent;
using Eventura.Domain.Entities;

namespace Eventura.Infrastructure.Persistence;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Simular almacenamiento persistente en memoria para desarrollo y pruebas.
/// Responsabilidades: Mantener colecciones concurrentes para eventos, reservas y usuarios.
/// Dependencias/Puertos utilizados: Utiliza entidades de dominio y estructuras concurrentes de .NET.
/// Límites (lo que NO debe hacer): Aplicar lógica de dominio o exponer colecciones mutables fuera de infraestructura.
/// Errores comunes: Usar este almacén en producción o no sincronizar accesos concurrentes adecuadamente.
/// </summary>
public sealed class InMemoryDataStore
{
    private readonly ConcurrentDictionary<Guid, Event> _events = new();
    private readonly ConcurrentDictionary<Guid, Reservation> _reservations = new();
    private readonly ConcurrentDictionary<Guid, User> _users = new();

    public ConcurrentDictionary<Guid, Event> Events => _events;
    public ConcurrentDictionary<Guid, Reservation> Reservations => _reservations;
    public ConcurrentDictionary<Guid, User> Users => _users;
}
