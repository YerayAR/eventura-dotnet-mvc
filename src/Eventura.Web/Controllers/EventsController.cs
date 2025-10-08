using Eventura.Application.DTOs;
using Eventura.Application.Services;
using Eventura.Domain.Enums;
using Eventura.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventura.Web.Controllers;

/// <summary>
/// Capa: Web.
/// Proposito: Gestionar el ciclo de vida de eventos a traves de peticiones MVC.
/// Responsabilidades: Recibir entrada de usuario, delegar en IEventService y preparar ViewModels.
/// Dependencias/Puertos utilizados: IEventService, ILogger.
/// Limites (lo que NO debe hacer): Contener logica de dominio, acceder a repositorios o exponer entidades Event.
/// Errores comunes: Mezclar validaciones de dominio aqui o omitir atributos de seguridad en formularios.
/// </summary>
[Authorize]
public sealed class EventsController : Controller
{
    private readonly IEventService _eventService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IEventService eventService, ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _logger = logger;
    }

#region Aprendizaje
// Controlador delgado: entrada -> ModelState -> DTO -> Caso de uso -> ViewModel -> Vista.
// Seguridad: Politicas RequireOrganizer protegen acciones mutadoras, anti-CSRF en POST y sanitizacion Razor.
// DTOs vs entidades: nunca se pasan objetos Event al front, solo EventDto/EventFormViewModel.
// TODO(aprendizaje): agregar ordenacion y busqueda al listado Index con prueba de integracion.
#endregion

    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Contexto: Listado publico de eventos proximos.
        // Intencion: Mostrar coleccion de eventos usando DTOs proyectados.
        // Pasos: 1) Obtener eventos via IEventService; 2) Mapear a ViewModel; 3) Retornar vista.
        // Validaciones: Se asume que el servicio ya filtro datos validos.
        // Manejo de errores: Excepciones se manejan por middleware y vista Error compartida.
        var events = await _eventService.GetUpcomingAsync(cancellationToken).ConfigureAwait(false);
        var model = events.Select(MapToListItem).ToList();
        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        // Contexto: Visualizacion de detalles de un evento especifico.
        // Intencion: Recuperar un evento por su identificador y mostrarlo en la vista.
        // Pasos: 1) Llamar a GetByIdAsync; 2) Validar OperationResult; 3) Mapear a ViewModel; 4) Retornar vista o NotFound.
        // Validaciones: Comprueba existencia y resultado exitoso.
        // Manejo de errores: Retorna NotFound para id inexistente; otras excepciones suben al middleware.
        var result = await _eventService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded || result.Data is null)
        {
            return NotFound();
        }

        return View(MapToListItem(result.Data));
    }

    [Authorize(Policy = "RequireOrganizer")]
    public IActionResult Create()
    {
        // Contexto: GET protegido para mostrar formulario de crear evento.
        // Intencion: Proveer ViewModel vacio a organizadores autorizados.
        // Pasos: 1) Instanciar EventFormViewModel; 2) Retornar vista.
        // Validaciones: Roles aplicados via politicas.
        // Manejo de errores: N/A.
        return View(new EventFormViewModel());
    }

    [Authorize(Policy = "RequireOrganizer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model, CancellationToken cancellationToken)
    {
        // Contexto: Postback del formulario de creacion.
        // Intencion: Validar datos, delegar en el caso de uso y manejar respuesta.
        // Pasos: 1) Verificar ModelState (validaciones MVC); 2) Mapear ViewModel -> CreateEventRequest; 3) Invocar CreateAsync; 4) Manejar OperationResult; 5) Registrar logs; 6) Redirigir.
        // Validaciones: DataAnnotations + validaciones en CreateEventRequestValidator; anti-CSRF protege POST.
        // Manejo de errores: ModelState.AddModelError y mensajes en TempData; excepciones manejadas por middleware.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = MapToCreateRequest(model);
        var result = await _eventService.CreateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Event creation failed: {Error}", result.Error);
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al crear el evento.");
            return View(model);
        }

        _logger.LogInformation("Event {EventId} created by {User}", result.Data!.Id, User.Identity?.Name);
        TempData["Success"] = "Evento creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "RequireOrganizer")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        // Contexto: GET para editar un evento existente.
        // Intencion: Recuperar datos y rellenar el formulario de edicion.
        // Pasos: 1) Solicitar evento via servicio; 2) Validar resultado; 3) Mapear a EventFormViewModel; 4) Mostrar vista.
        // Validaciones: Politica RequireOrganizer y comprobacion de existencia.
        // Manejo de errores: Retorna NotFound cuando no se encuentra.
        var result = await _eventService.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded || result.Data is null)
        {
            return NotFound();
        }

        var model = MapToForm(result.Data);
        return View(model);
    }

    [Authorize(Policy = "RequireOrganizer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EventFormViewModel model, CancellationToken cancellationToken)
    {
        // Contexto: Postback para persistir cambios.
        // Intencion: Actualizar un evento existente manteniendo invariantes de dominio.
        // Pasos: 1) Validar que id de ruta coincida con modelo; 2) Revisar ModelState; 3) Construir UpdateEventRequest; 4) Invocar UpdateAsync; 5) Gestionar resultado; 6) Redirigir.
        // Validaciones: Comparacion id/model.Id, DataAnnotations y validators del caso de uso.
        // Manejo de errores: Retorna BadRequest si los ids no alinean y reutiliza ModelState para feedback.
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updateRequest = new UpdateEventRequest
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            StartDateTime = model.StartDateTime,
            Duration = TimeSpan.FromMinutes(model.DurationMinutes),
            City = model.City,
            AddressLine = model.AddressLine,
            Capacity = model.Capacity,
            Category = model.Category
        };

        var result = await _eventService.UpdateAsync(updateRequest, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Event update failed: {Error}", result.Error);
            ModelState.AddModelError(string.Empty, result.Error ?? "Error al actualizar el evento.");
            return View(model);
        }

        _logger.LogInformation("Event {EventId} updated by {User}", updateRequest.Id, User.Identity?.Name);
        TempData["Success"] = "Evento actualizado.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "RequireOrganizer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        // Contexto: Eliminacion logica de un evento iniciada por organizador.
        // Intencion: Delegar en el caso de uso y reportar resultado.
        // Pasos: 1) Invocar DeleteAsync; 2) Evaluar OperationResult; 3) Registrar logs; 4) Escribir mensaje en TempData; 5) Redirigir.
        // Validaciones: Politica de rol y validators en servicio que revisan Guid.
        // Manejo de errores: TempData almacena mensaje para la vista posterior.
        var result = await _eventService.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            _logger.LogInformation("Event {EventId} deleted by {User}", id, User.Identity?.Name);
            TempData["Success"] = "Evento eliminado.";
        }

        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Recommendations(string? city, EventCategory? category, CancellationToken cancellationToken)
    {
        // Contexto: Busqueda de eventos recomendados con filtros opcionales.
        // Intencion: Delegar en el caso de uso y devolver ViewModel enriquecido.
        // Pasos: 1) Construir RecommendationRequest; 2) Invocar GetRecommendationsAsync; 3) Mapear a RecommendationViewModel; 4) Retornar vista.
        // Validaciones: Normalizacion de parametros se hace en el servicio.
        // Manejo de errores: Excepciones se tratan en middleware compartido.
        var request = new RecommendationRequest
        {
            City = city ?? string.Empty,
            Category = category
        };

        var events = await _eventService.GetRecommendationsAsync(request, cancellationToken).ConfigureAwait(false);
        var model = new RecommendationViewModel
        {
            City = city ?? string.Empty,
            Category = category,
            Events = events.Select(MapToListItem).ToList()
        };

        return View(model);
    }

    private static EventListItemViewModel MapToListItem(EventDto dto)
    {
        // Contexto: Conversión de DTO a ViewModel para vistas Razor.
        // Intencion: Aislar la capa Web de detalles de dominio.
        // Pasos: 1) Copiar propiedades relevantes; 2) Crear EventListItemViewModel.
        // Validaciones: Se asume DTO valido.
        // Manejo de errores: N/A.
        return new EventListItemViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            StartDateTime = dto.StartDateTime,
            Duration = dto.Duration,
            City = dto.City,
            AddressLine = dto.AddressLine,
            RemainingCapacity = dto.RemainingCapacity,
            Category = dto.Category,
            IsCancelled = dto.IsCancelled
        };
    }

    private static EventFormViewModel MapToForm(EventDto dto)
    {
        // Contexto: Rellenar formulario de edicion desde datos existentes.
        // Intencion: Preparar ViewModel con unidad de minutos para inputs amigables.
        // Pasos: 1) Copiar propiedades; 2) Convertir duracion a minutos redondeados.
        // Validaciones: N/A, datos ya validados.
        // Manejo de errores: N/A.
        return new EventFormViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            StartDateTime = dto.StartDateTime,
            DurationMinutes = (int)Math.Round(dto.Duration.TotalMinutes),
            City = dto.City,
            AddressLine = dto.AddressLine,
            Capacity = dto.Capacity,
            Category = dto.Category
        };
    }

    private static CreateEventRequest MapToCreateRequest(EventFormViewModel model)
    {
        // Contexto: Transformacion previa a enviar datos al caso de uso.
        // Intencion: Construir DTO que respeta contratos de Application.
        // Pasos: 1) Leer ViewModel; 2) Convertir duracion a TimeSpan; 3) Retornar CreateEventRequest.
        // Validaciones: ModelState ya reviso valores; el servicio aplica validaciones adicionales.
        // Manejo de errores: N/A.
        return new CreateEventRequest
        {
            Title = model.Title,
            Description = model.Description,
            StartDateTime = model.StartDateTime,
            Duration = TimeSpan.FromMinutes(model.DurationMinutes),
            City = model.City,
            AddressLine = model.AddressLine,
            Capacity = model.Capacity,
            Category = model.Category
        };
    }
}

