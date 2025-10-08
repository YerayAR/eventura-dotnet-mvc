namespace Eventura.Application.DTOs;

#region Aprendizaje
// Estos DTOs encapsulan datos sensibles que viajan entre la capa Web y Application.
// Aquí se definen las estructuras para credenciales y roles sin exponer entidades de dominio.
// TODO(aprendizaje): agregar propiedades para MFA (token temporal) y actualizar pruebas de seguridad.
#endregion

/// <summary>
/// Capa: Application.
/// Propósito: Capturar la información de registro proveniente de la interfaz.
/// Responsabilidades: Transportar datos necesarios para que AuthService cree un usuario.
/// Dependencias/Puertos utilizados: Ninguna directa.
/// Límites (lo que NO debe hacer): Incluir contraseñas en logs o serializaciones no seguras.
/// Errores comunes: No limpiar espacios en la capa superior antes de enviar.
/// </summary>
public sealed record RegisterUserRequest
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = Roles.User;
}

/// <summary>
/// Capa: Application.
/// Propósito: DTO de entrada para inicio de sesión.
/// Responsabilidades: Entregar credenciales y metadatos mínimos al servicio de autenticación.
/// Dependencias/Puertos utilizados: Ninguna.
/// Límites (lo que NO debe hacer): Guardar contraseñas persistidas; sólo debe vivir en memoria transicional.
/// Errores comunes: No incluir dirección IP para auditoría o rate limiting.
/// </summary>
public sealed record LoginRequest
{
    public string UserNameOrEmail { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool RememberMe { get; init; }
    public string IpAddress { get; init; } = string.Empty;
}

/// <summary>
/// Capa: Application.
/// Propósito: DTO de salida tras una autenticación exitosa.
/// Responsabilidades: Suministrar datos mínimos para construir claims y cookies seguras.
/// Dependencias/Puertos utilizados: Ninguna adicional.
/// Límites (lo que NO debe hacer): Contener secretos como hashes o tokens.
/// Errores comunes: Enviar este DTO directamente a vistas sin sanitizar cuando se muestra email.
/// </summary>
public sealed record AuthenticatedUserDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}

/// <summary>
/// Capa: Application.
/// Propósito: Fuente única de roles reconocidos por la aplicación.
/// Responsabilidades: Evitar cadenas mágicas y facilitar políticas de autorización.
/// Dependencias/Puertos utilizados: Consumido por Web y Application.
/// Límites (lo que NO debe hacer): Persistir cambios en tiempo de ejecución ni incluir roles experimentales sin revisión.
/// Errores comunes: Añadir nuevos roles aquí sin actualizar políticas de autorización asociadas.
/// </summary>
public static class Roles
{
    public const string User = "User";
    public const string Organizer = "Organizer";
    public const string Admin = "Admin";
}
