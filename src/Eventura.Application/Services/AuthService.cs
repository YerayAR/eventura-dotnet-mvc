using System.Security.Claims;
using Eventura.Application.Abstractions;
using Eventura.Application.DTOs;
using Eventura.Domain.Entities;
using Eventura.Domain.Repositories;
using Eventura.Domain.ValueObjects;

namespace Eventura.Application.Services;

/// <summary>
/// Capa: Application.
/// Propósito: Definir el contrato de autenticación y gestión de acceso para la aplicación.
/// Responsabilidades: Registrar usuarios, iniciar sesión, gestionar intentos fallidos y emitir ClaimsPrincipal.
/// Dependencias/Puertos utilizados: Repositorios de usuarios, servicios de hashing y envío de correo.
/// Límites (lo que NO debe hacer): No manejar sesiones HTTP ni lógica de presentación.
/// Errores comunes: Olvidar resetear intentos fallidos o mezclar responsabilidades de infraestructura.
/// </summary>
public interface IAuthService
{
    Task<OperationResult<AuthenticatedUserDto>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<AuthenticatedUserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> RegisterFailedAttemptAsync(string userNameOrEmail, CancellationToken cancellationToken = default);
    Task<OperationResult> ResetAccessFailuresAsync(Guid userId, CancellationToken cancellationToken = default);
    ClaimsPrincipal BuildClaimsPrincipal(AuthenticatedUserDto user, bool isPersistent);
}

/// <summary>
/// Capa: Application.
/// Propósito: Ejecutar lógica de autenticación segura respetando principios de arquitectura limpia.
/// Responsabilidades: Validar credenciales, generar usuarios, emitir claims y coordinar repositorios.
/// Dependencias/Puertos utilizados: IUserRepository, IPasswordHasher, IEmailSender, IUnitOfWork.
/// Límites (lo que NO debe hacer): No persistir estado de sesión ni desarrollar lógica de UI.
/// Errores comunes: Retornar información sensible en mensajes de error o no hashear contraseñas.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
    }

#region Aprendizaje
// La autenticación aquí demuestra cómo los servicios de aplicación protegen la capa de dominio
// de detalles de hashing y correo (infraestructura). Se inyectan puertos para lograr testabilidad.
// El servicio convierte DTOs en entidades y viceversa, manteniendo controladores delgados y seguros.
// TODO(aprendizaje): implementar MFA opcional con un nuevo servicio y validar impacto en pruebas.
#endregion

    public async Task<OperationResult<AuthenticatedUserDto>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        // Contexto: Registro de usuario desde formularios MVC o seed inicial.
        // Intención: Crear usuarios válidos evitando duplicidades y enviando notificaciones.
        // Pasos: 1) Validar entrada y contraseña; 2) Crear value object Email; 3) Comprobar duplicados; 4) Hashear contraseña; 5) Crear entidad; 6) Persistir; 7) Notificar vía correo; 8) Retornar DTO.
        // Validaciones: Contraseña con longitud mínima, email válido, unicidad de email y username.
        // Manejo de errores: Responde con OperationResult de fallo y mensaje contextual sin exponer información sensible.
        if (request is null)
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Password must be at least 8 characters long.");
        }

        var email = EmailAddress.Create(request.Email);

        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (existingUser is not null)
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Email is already registered.");
        }

        existingUser = await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken).ConfigureAwait(false);
        if (existingUser is not null)
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Username is already taken.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var role = string.IsNullOrWhiteSpace(request.Role) ? Roles.User : request.Role.Trim();
        var user = User.Create(request.UserName, email, passwordHash, role);

        await _userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _emailSender.SendAsync(email.Value, "Welcome to Eventura", "Your account has been created.", cancellationToken)
            .ConfigureAwait(false);

        return OperationResult<AuthenticatedUserDto>.Success(MapToDto(user));
    }

    public async Task<OperationResult<AuthenticatedUserDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Contexto: Inicio de sesión procesado al enviar formulario en la capa Web.
        // Intención: Autenticar credenciales de forma segura sin revelar detalles internos.
        // Pasos: 1) Validar entrada; 2) Buscar usuario por email/username; 3) Verificar bloqueo; 4) Comprobar hash de contraseña; 5) Actualizar contadores; 6) Retornar DTO autenticado.
        // Validaciones: Campos requeridos y estado del usuario (bloqueado).
        // Manejo de errores: Incrementa intentos fallidos y devuelve mensajes genéricos para evitar enumeración de credenciales.
        if (request is null)
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Credentials are required.");
        }

        var user = await FindUserAsync(request.UserNameOrEmail, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Invalid credentials.");
        }

        if (user.IsLocked)
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Account is locked due to failed attempts.");
        }

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            user.RegisterAccessFailure();
            await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return OperationResult<AuthenticatedUserDto>.Failure("Invalid credentials.");
        }

        user.ResetAccessFailures();
        await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return OperationResult<AuthenticatedUserDto>.Success(MapToDto(user));
    }

    public async Task<OperationResult> RegisterFailedAttemptAsync(string userNameOrEmail, CancellationToken cancellationToken = default)
    {
        // Contexto: Utilizado cuando se detecta un intento fallido por otras capas (p.ej. rate limiting).
        // Intención: Contabilizar intentos incorrectos y activar bloqueo de cuenta.
        // Pasos: 1) Validar identificador; 2) Buscar usuario; 3) Registrar fallo; 4) Persistir y guardar.
        // Validaciones: Entrada no vacía y existencia del usuario.
        // Manejo de errores: Devuelve OperationResult con mensaje si no se localiza el usuario.
        if (string.IsNullOrWhiteSpace(userNameOrEmail))
        {
            return OperationResult.Failure("Username or email is required.");
        }

        var user = await FindUserAsync(userNameOrEmail, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return OperationResult.Failure("User not found.");
        }

        user.RegisterAccessFailure();
        await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult.Success();
    }

    public async Task<OperationResult> ResetAccessFailuresAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Contexto: Reinicio de contadores tras login exitoso o intervención administrativa.
        // Intención: Limpiar el estado de bloqueo ofreciendo experiencia segura.
        // Pasos: 1) Validar id; 2) Obtener usuario; 3) Reiniciar contadores; 4) Actualizar repositorio; 5) Guardar cambios.
        // Validaciones: GUID válido y existencia del usuario.
        // Manejo de errores: OperationResult con mensajes cuando hay entradas inválidas o usuario faltante.
        if (userId == Guid.Empty)
        {
            return OperationResult.Failure("Invalid user identifier.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return OperationResult.Failure("User not found.");
        }

        user.ResetAccessFailures();
        await _userRepository.UpdateAsync(user, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult.Success();
    }

    public ClaimsPrincipal BuildClaimsPrincipal(AuthenticatedUserDto user, bool isPersistent)
    {
        // Contexto: Construido tras autenticación y consumido por la capa Web para emitir cookies seguras.
        // Intención: Representar la identidad del usuario con claims estándar.
        // Pasos: 1) Crear lista de claims; 2) Generar ClaimsIdentity con esquema; 3) Retornar ClaimsPrincipal.
        // Validaciones: Asume DTO consistente; se podrían añadir roles múltiples como ejercicio.
        // Manejo de errores: No aplica; la creación de claims es determinista.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, "EventuraCookie");
        return new ClaimsPrincipal(identity);
    }

    private static AuthenticatedUserDto MapToDto(User user)
    {
        // Contexto: Conversión de entidad de dominio a DTO seguro.
        // Intención: Retornar datos mínimos para la capa Web y cookies.
        // Pasos: 1) Leer propiedades; 2) Crear DTO; 3) Retornar.
        // Validaciones: N/A, la entidad ya es consistente.
        // Manejo de errores: N/A, asignaciones directas.
        return new AuthenticatedUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email.Value,
            Role = user.Role
        };
    }

    private async Task<User?> FindUserAsync(string userNameOrEmail, CancellationToken cancellationToken)
    {
        // Contexto: Algoritmo compartido para localizar usuarios por email o username.
        // Intención: Centralizar la búsqueda y reutilizar value objects.
        // Pasos: 1) Detectar si hay '@'; 2) Crear EmailAddress cuando aplica; 3) Consultar repositorio correspondiente.
        // Validaciones: EmailAddress.Create asegura formato válido al buscar por email.
        // Manejo de errores: Delega a repositorios; excepciones suben al middleware de errores.
        if (userNameOrEmail.Contains('@', StringComparison.Ordinal))
        {
            var email = EmailAddress.Create(userNameOrEmail);
            return await _userRepository.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        }

        return await _userRepository.GetByUserNameAsync(userNameOrEmail, cancellationToken).ConfigureAwait(false);
    }
}
