using System.Diagnostics;
using Eventura.Application.Services;
using Eventura.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Eventura.Web.Controllers;

/// <summary>
/// Capa: Web.
/// Propósito: Controlador público para la página principal.
/// Responsabilidades: Recibir peticiones HTTP, invocar casos de uso y devolver vistas.
/// Dependencias/Puertos utilizados: Depende de IEventService y ILogger.
/// Límites (lo que NO debe hacer): Contener lógica de negocio o manipular entidades de dominio directamente.
/// Errores comunes: Omitir sanitización en vistas o no manejar errores con la vista Error.
/// </summary>
public sealed class HomeController : Controller
{
    private readonly IEventService _eventService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IEventService eventService, ILogger<HomeController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

#region Aprendizaje
// Flujo MVC: la petición llega al controlador -> se delega a IEventService -> se proyectan DTOs a ViewModels -> se devuelve la vista.
// Seguridad: la vista se beneficia de sanitización Razor y del middleware anti-CSRF configurado globalmente.
// TODO(aprendizaje): extender este controlador para soportar paginación y cubrirlo con prueba de integración.
#endregion

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Contexto: Ruta GET principal que lista eventos próximos.
        // Intención: Mostrar al usuario un catálogo de eventos usando DTOs proyectados.
        // Pasos: 1) Llamar al caso de uso; 2) Mapear DTO -> ViewModel; 3) Retornar vista.
        // Validaciones: Confiamos en validaciones previas; se podría añadir filtro de categoría como ejercicio.
        // Manejo de errores: Cualquier excepción es capturada por middleware de errores con correlación.
        var events = await _eventService.GetUpcomingAsync(cancellationToken).ConfigureAwait(false);
        var viewModel = events.Select(e => new EventListItemViewModel
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            StartDateTime = e.StartDateTime,
            Duration = e.Duration,
            City = e.City,
            AddressLine = e.AddressLine,
            RemainingCapacity = e.RemainingCapacity,
            Category = e.Category,
            IsCancelled = e.IsCancelled
        }).ToList();

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Contexto: Acción que responde ante errores no controlados.
        // Intención: Presentar información de diagnóstico sin filtrar datos sensibles.
        // Pasos: 1) Obtener requestId/correlationId; 2) Registrar error; 3) Devolver vista con modelo seguro.
        // Validaciones: No aplica; se basa en información de contexto.
        // Manejo de errores: Evita lanzar nuevas excepciones y se apoya en ResponseCache headers para evitar caching.
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogError("Unhandled error with correlation id {CorrelationId}", HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = requestId });
    }
}
