using Eventura.Application.DTOs;
using Eventura.Application.Services;
using Eventura.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventura.Web.Controllers;

/// <summary>
/// Capa: Web.
/// Proposito: Orquestar flujos de autenticacion dentro del modelo MVC.
/// Responsabilidades: Procesar formularios, delegar en IAuthService y emitir cookies seguras.
/// Dependencias/Puertos utilizados: IAuthService, ILogger, autenticacion de cookies de ASP.NET.
/// Limites (lo que NO debe hacer): Hashear contrasenias, acceder a repositorios o exponer entidades de dominio.
/// Errores comunes: Olvidar atributos anti-CSRF o filtrar mensajes con informacion sensible.
/// </summary>
public sealed class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

#region Aprendizaje
// Flujo MVC: la peticion llega al controlador -> ModelState valida datos -> se crea DTO -> IAuthService ejecuta el caso de uso -> se construye ClaimsPrincipal -> respuesta HTTP.
// Seguridad aplicada: atributos [ValidateAntiForgeryToken] en POST, cookies HttpOnly/Secure configuradas en Program.cs y sanitizacion Razor en vistas.
// TODO(aprendizaje): agregar paso de MFA antes de emitir la cookie y cubrirlo con pruebas de autorizacion.
#endregion

    [AllowAnonymous]
    public IActionResult Register()
    {
        // Contexto: GET que prepara el formulario de registro.
        // Intencion: Entregar un modelo limpio a la vista.
        // Pasos: 1) Instanciar RegisterViewModel; 2) Retornar la vista.
        // Validaciones: No aplica; se evaluan en el POST.
        // Manejo de errores: N/A.
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        // Contexto: Postback del formulario de registro.
        // Intencion: Validar datos, delegar en AuthService y autenticar al nuevo usuario.
        // Pasos: 1) Revisar ModelState; 2) Mapear a RegisterUserRequest; 3) Invocar RegisterAsync; 4) Evaluar OperationResult; 5) Firmar cookie; 6) Redirigir.
        // Validaciones: DataAnnotations + reglas de AuthService (unicidad de correo y longitud de contrasenia).
        // Manejo de errores: ModelState.AddModelError y logging; el middleware global captura excepciones inesperadas.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new RegisterUserRequest
        {
            UserName = model.UserName,
            Email = model.Email,
            Password = model.Password,
            Role = model.Role
        };

        var result = await _authService.RegisterAsync(request, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded || result.Data is null)
        {
            _logger.LogWarning("Registration failed for {User}", model.UserName);
            ModelState.AddModelError(string.Empty, result.Error ?? "No fue posible completar el registro.");
            return View(model);
        }

        await SignInAsync(result.Data, true).ConfigureAwait(false);
        TempData["Success"] = "Registro completado.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Contexto: GET previo al inicio de sesion.
        // Intencion: Mostrar formulario y preservar returnUrl seguro.
        // Pasos: 1) Almacenar returnUrl en ViewData; 2) Crear LoginViewModel; 3) Retornar vista.
        // Validaciones: Diferidas al POST.
        // Manejo de errores: N/A.
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        // Contexto: Postback del formulario de login.
        // Intencion: Autenticar credenciales y emitir cookie con politicas seguras.
        // Pasos: 1) Verificar ModelState; 2) Mapear a LoginRequest incluyendo IP; 3) Llamar a LoginAsync; 4) Manejar resultado; 5) Firmar cookie; 6) Redirigir validando returnUrl.
        // Validaciones: Anti-CSRF, ModelState y Url.IsLocalUrl para evitar open redirects.
        // Manejo de errores: Mensaje generico en ModelState, logging y retorno de la misma vista.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = new LoginRequest
        {
            UserNameOrEmail = model.UserNameOrEmail,
            Password = model.Password,
            RememberMe = model.RememberMe,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        };

        var result = await _authService.LoginAsync(request, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded || result.Data is null)
        {
            _logger.LogWarning("Login failed for {User}", model.UserNameOrEmail);
            ModelState.AddModelError(string.Empty, result.Error ?? "Credenciales invalidas.");
            return View(model);
        }

        await SignInAsync(result.Data, model.RememberMe).ConfigureAwait(false);
        _logger.LogInformation("User {User} logged in", result.Data.UserName);
        // TODO(aprendizaje): forzar fallo de login y observar cómo RegisterFailedAttemptAsync incrementa bloqueo.
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Contexto: Cierre de sesion solicitado desde un formulario protegido.
        // Intencion: Invalidar la cookie y regresar al inicio.
        // Pasos: 1) Ejecutar SignOutAsync; 2) Redirigir a Home/Index.
        // Validaciones: Anti-CSRF evita solicitudes forjadas.
        // Manejo de errores: Delegado al middleware de errores.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        // Contexto: Usuarios sin permisos intentan acceder a rutas protegidas.
        // Intencion: Mostrar aviso generico sin filtrar detalles de seguridad.
        // Pasos: 1) Retornar vista AccessDenied.
        // Validaciones: N/A.
        // Manejo de errores: N/A.
        return View();
    }

    private async Task SignInAsync(AuthenticatedUserDto user, bool isPersistent)
    {
        // Contexto: Consolidacion de la firma de usuario tras autenticacion exitosa.
        // Intencion: Construir ClaimsPrincipal y emitir cookie conforme a configuracion de seguridad.
        // Pasos: 1) Construir claims via AuthService; 2) Ejecutar SignInAsync con opciones persistentes.
        // Validaciones: Se asume DTO consistente; se podria extender para roles multiples.
        // Manejo de errores: Cualquier excepcion se propaga al middleware global.
        var principal = _authService.BuildClaimsPrincipal(user, isPersistent);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                AllowRefresh = true,
                IssuedUtc = DateTimeOffset.UtcNow
            }).ConfigureAwait(false);
    }
}
