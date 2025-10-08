using Eventura.Domain.Entities;
using Eventura.Domain.ValueObjects;

namespace Eventura.Domain.Repositories;

/// <summary>
/// Capa: Domain.
/// Propósito: Definir operaciones de persistencia para usuarios.
/// Responsabilidades: Ofrecer métodos para consultar y modificar entidades User.
/// Dependencias/Puertos utilizados: Colabora con value object EmailAddress.
/// Límites (lo que NO debe hacer): No debe devolver tipos concretos de infraestructura ni manipular DTOs.
/// Errores comunes: Implementar lógica de autorización aquí en lugar de servicios de aplicación.
/// </summary>
public interface IUserRepository
{
#region Aprendizaje
// Puerto de repositorio para usuarios: evita que Application conozca detalles de almacenamiento (EF, Mongo, etc.).
// TODO(aprendizaje): implementar version cacheada y analizar impacto en login (concepto: caching y consistencia).
#endregion
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
