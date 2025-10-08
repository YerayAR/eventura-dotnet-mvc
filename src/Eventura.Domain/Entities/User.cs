using Eventura.Domain.Exceptions;
using Eventura.Domain.ValueObjects;

namespace Eventura.Domain.Entities;

/// <summary>
/// Capa: Domain.
/// Propósito: Modelar al usuario del sistema con estado de autenticación y bloqueo.
/// Responsabilidades: Mantener hash de contraseña, rol y control de intentos fallidos.
/// Dependencias/Puertos utilizados: Value object EmailAddress para garantizar formato.
/// Límites (lo que NO debe hacer): No acceder a servicios de hashing ni bases de datos.
/// Errores comunes: Mutar directamente propiedades o almacenar contraseñas en texto plano.
/// </summary>
public sealed class User
{
    public Guid Id { get; private set; }
    public string UserName { get; private set; }
    public EmailAddress Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }
    public bool IsLocked { get; private set; }
    public int AccessFailedCount { get; private set; }

    private User()
    {
        UserName = string.Empty;
        Email = EmailAddress.Create("placeholder@example.com");
        PasswordHash = string.Empty;
        Role = string.Empty;
    }

    private User(Guid id, string userName, EmailAddress email, string passwordHash, string role)
    {
        Id = id;
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

#region Aprendizaje
// La entidad User no conoce detalles de hashing ni infraestructura: recibe valores ya procesados
// desde Application. Protege invariantes de roles y bloqueo. Es parte de la capa Domain y se integra
// con repositorios a través de interfaces. Mantiene control sobre intentos fallidos.
// TODO(aprendizaje): implementar sistema de roles múltiples y escribir pruebas de autorización por rol.
#endregion

    public static User Create(string userName, EmailAddress email, string passwordHash, string role)
    {
        // Contexto: Creación de usuarios desde servicio de autenticación.
        // Intención: Garantizar que todos los parámetros obligatorios sean provistos y normalizados.
        // Pasos: 1) Validar username; 2) Validar hash; 3) Validar rol; 4) Instanciar entidad con Guid nuevo.
        // Validaciones: Comprueba que los campos no estén vacíos y normaliza cadenas.
        // Manejo de errores: Lanza DomainValidationException indicando el campo faltante.
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new DomainValidationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("Password hash is required.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new DomainValidationException("Role is required.");
        }

        return new User(Guid.NewGuid(), userName.Trim(), email, passwordHash, role.Trim());
    }

    public void SetPasswordHash(string passwordHash)
    {
        // Contexto: Cambio de contraseña desde servicio de autenticación.
        // Intención: Actualizar hash garantizando no persistir valores vacíos.
        // Pasos: 1) Validar cadena; 2) Asignar nuevo hash.
        // Validaciones: Comprueba que el hash no sea nulo ni vacío.
        // Manejo de errores: Lanza DomainValidationException si el hash es inválido.
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("Password hash is required.");
        }

        PasswordHash = passwordHash;
    }

    public void SetRole(string role)
    {
        // Contexto: Cambio de rol administrado por políticas de negocio.
        // Intención: Ajustar rol asegurando que sea válido y sin espacios extra.
        // Pasos: 1) Validar texto; 2) Normalizar con Trim; 3) Asignar.
        // Validaciones: Exige rol no vacío.
        // Manejo de errores: Lanza DomainValidationException cuando rol es inválido.
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new DomainValidationException("Role is required.");
        }

        Role = role.Trim();
    }

    public void RegisterAccessFailure()
    {
        // Contexto: Cada intento fallido de login incrementa el contador.
        // Intención: Bloquear cuenta tras múltiples fallos para proteger contra fuerza bruta.
        // Pasos: 1) Incrementar contador; 2) Evaluar umbral; 3) Marcar bloqueo si supera límite.
        // Validaciones: El umbral actual es 5 intentos consecutivos.
        // Manejo de errores: No lanza; actualiza estado interno.
        AccessFailedCount++;
        if (AccessFailedCount >= 5)
        {
            IsLocked = true;
        }
    }

    public void ResetAccessFailures()
    {
        // Contexto: Después de login exitoso o desbloqueo manual.
        // Intención: Restablecer estado seguro para permitir intentos futuros.
        // Pasos: 1) Resetear contador; 2) Desbloquear usuario.
        // Validaciones: No requiere.
        // Manejo de errores: No lanza; cambios directos.
        AccessFailedCount = 0;
        IsLocked = false;
    }
}
