using Eventura.Domain.Entities;
using Eventura.Domain.Repositories;
using Eventura.Domain.ValueObjects;

namespace Eventura.Infrastructure.Persistence;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Adaptador in-memory para persistencia de usuarios.
/// Responsabilidades: Almacenar, actualizar y consultar entidades User.
/// Dependencias/Puertos utilizados: InMemoryDataStore, value object EmailAddress.
/// Límites (lo que NO debe hacer): Manejar lógica de autenticación o hashing.
/// Errores comunes: No comparar emails correctamente o no respetar case-insensitive para usernames.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly InMemoryDataStore _store;

    public InMemoryUserRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        // Contexto: Registro de usuario nuevo desde AuthService.
        // Intención: Persistir entidad User en el almacén.
        // Pasos: 1) Asignar usuario al diccionario por Id; 2) Completar tarea.
        // Validaciones: Entidad se asume valida por dominio.
        // Manejo de errores: ConcurrentDictionary trata condiciones de carrera.
        _store.Users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken = default)
    {
        // Contexto: Consulta para validar unicidad de correo o recuperar usuario.
        // Intención: Encontrar usuario cuyo EmailAddress coincide.
        // Pasos: 1) Iterar valores; 2) Comparar con Equals del value object; 3) Retornar coincidencia.
        // Validaciones: Se apoya en igualdad de EmailAddress.
        // Manejo de errores: Devuelve null cuando no existe.
        var user = _store.Users.Values.FirstOrDefault(u => u.Email.Equals(email));
        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Contexto: Acceso directo por identificador.
        // Intención: Recuperar entidad para operaciones como reset de fallos.
        // Pasos: 1) TryGetValue; 2) Retornar usuario o null.
        // Validaciones: N/A.
        // Manejo de errores: Sin excepciones; null indica ausencia.
        _store.Users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        // Contexto: Validaciones de registro e inicio de sesión.
        // Intención: Buscar usuario por nombre (case-insensitive).
        // Pasos: 1) Filtrar por UserName; 2) Retornar coincidencia.
        // Validaciones: Usa StringComparison.OrdinalIgnoreCase para evitar duplicidades por casing.
        // Manejo de errores: Retorna null cuando no encuentra coincidencias.
        var user = _store.Users.Values.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        // Contexto: Actualización tras cambios en contraseñas o roles.
        // Intención: Reemplazar entidad existente.
        // Pasos: 1) Asignar al diccionario; 2) Completar tarea.
        // Validaciones: Se asume entidad consistente.
        // Manejo de errores: Idempotente.
        _store.Users[user.Id] = user;
        return Task.CompletedTask;
    }
}
