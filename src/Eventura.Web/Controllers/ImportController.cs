using Eventura.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventura.Web.Controllers;

/// <summary>
/// Capa: Web.
/// Propósito: Controlador para importar eventos externos a la aplicación.
/// Responsabilidades: Coordinar importación de datos externos y proporcionar feedback al usuario.
/// Dependencias/Puertos utilizados: ExternalEventImportService para lógica de importación.
/// Límites (lo que NO debe hacer): No debe contener lógica de mapeo ni acceso directo a APIs.
/// Errores comunes: No validar permisos de usuario o no manejar errores de importación.
/// </summary>
[Authorize(Policy = "RequireAdmin")]
public class ImportController : Controller
{
    private readonly ExternalEventImportService _importService;
    private readonly ILogger<ImportController> _logger;

    public ImportController(
        ExternalEventImportService importService,
        ILogger<ImportController> logger)
    {
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Página principal de importación de eventos.
    /// </summary>
    /// <returns>Vista con opciones de importación</returns>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Importa eventos populares desde fuentes externas.
    /// </summary>
    /// <param name="maxEvents">Número máximo de eventos a importar</param>
    /// <returns>Redirección con resultado de la operación</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportPopular(int maxEvents = 5)
    {
        try
        {
            _logger.LogInformation("Admin {User} requested import of {MaxEvents} popular events", 
                User.Identity?.Name, maxEvents);

            var result = await _importService.ImportPopularEventsAsync(maxEvents);
            
            if (result.Succeeded)
            {
                TempData["Success"] = $"✅ Se importaron {result.Data} eventos populares exitosamente.";
                _logger.LogInformation("Successfully imported {Count} popular events", result.Data);
            }
            else
            {
                TempData["Error"] = $"❌ Error al importar eventos: {result.Error}";
                _logger.LogWarning("Failed to import popular events: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing popular events");
            TempData["Error"] = "❌ Error inesperado al importar eventos. Inténtalo de nuevo.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Importa eventos de una ubicación específica.
    /// </summary>
    /// <param name="location">Ubicación para filtrar eventos</param>
    /// <param name="maxEvents">Número máximo de eventos a importar</param>
    /// <returns>Redirección con resultado de la operación</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportByLocation(string location, int maxEvents = 10)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            TempData["Error"] = "❌ Debes especificar una ubicación para importar eventos.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _logger.LogInformation("Admin {User} requested import of {MaxEvents} events for location {Location}", 
                User.Identity?.Name, maxEvents, location);

            var result = await _importService.ImportEventsByLocationAsync(location.Trim(), maxEvents);
            
            if (result.Succeeded)
            {
                if (result.Data > 0)
                {
                    TempData["Success"] = $"✅ Se importaron {result.Data} eventos de {location} exitosamente.";
                }
                else
                {
                    TempData["Warning"] = $"⚠️ No se encontraron eventos nuevos en {location} para importar.";
                }
                _logger.LogInformation("Imported {Count} events for location {Location}", result.Data, location);
            }
            else
            {
                TempData["Error"] = $"❌ Error al importar eventos de {location}: {result.Error}";
                _logger.LogWarning("Failed to import events for location {Location}: {Error}", location, result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing events for location {Location}", location);
            TempData["Error"] = $"❌ Error inesperado al importar eventos de {location}. Inténtalo de nuevo.";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// API endpoint para obtener vista previa de eventos disponibles para importar.
    /// </summary>
    /// <param name="location">Ubicación opcional para filtrar</param>
    /// <param name="category">Categoría opcional para filtrar</param>
    /// <returns>JSON con eventos disponibles</returns>
    [HttpGet]
    public async Task<IActionResult> Preview(string? location = null, string? category = null)
    {
        try
        {
            // Para demostración, devolvemos información simplificada
            var result = new
            {
                success = true,
                message = "Vista previa de eventos disponibles",
                totalAvailable = location?.ToLowerInvariant() switch
                {
                    "madrid" => 3,
                    "barcelona" => 2,
                    "valencia" => 2,
                    "sevilla" => 1,
                    "bilbao" => 1,
                    _ => 8
                },
                categories = new[] { "Technology", "Music", "Food", "Business", "Art", "Sports", "Education", "Literature" },
                locations = new[] { "Madrid", "Barcelona", "Valencia", "Sevilla", "Bilbao" }
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting import preview");
            return Json(new { success = false, message = "Error obteniendo vista previa" });
        }
    }
}