using System.Collections.Generic;
using Eventura.Application.Services;
using Eventura.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventura.Web.Controllers;

/// <summary>
/// Capa: Web.
/// Proposito: Gestionar reservas de usuarios autenticados.
/// Responsabilidades: Orquestar llamadas a IReservationService y preparar ViewModels.
/// Dependencias/Puertos utilizados: IReservationService, IEventService.
/// Limites (lo que NO debe hacer): Modificar directamente entidades de dominio o manejar transacciones.
/// Errores comunes: Omitir verificacion del usuario autenticado o no proteger formularios con anti-CSRF.
/// </summary>
[Authorize]
public sealed class ReservationsController : Controller
{
    private readonly IReservationService _reservationService;
    private readonly IEventService _eventService;

    public ReservationsController(IReservationService reservationService, IEventService eventService)
    {
        _reservationService = reservationService;
        _eventService = eventService;
    }

#region Aprendizaje
// Flujo MVC: controlador recibe POST -> valida ModelState y usuario -> construye DTO -> caso de uso -> TempData -> redireccion.
// Seguridad: Acciones requieren autenticacion, uso de anti-CSRF y verificacion de NameIdentifier.
// TODO(aprendizaje): simular error de transaccion para observar como el middleware de errores formatea la respuesta.
#endregion

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationFormViewModel model, CancellationToken cancellationToken)
    {
        // Contexto: Creacion de reserva desde vista de detalles.
        // Intencion: Delegar en ReservationService la validacion y persistencia.
        // Pasos: 1) Revisar ModelState; 2) Resolver usuario autenticado; 3) Crear DTO; 4) Llamar a CreateAsync; 5) Guardar mensaje en TempData; 6) Redirigir a pagina de evento.
        // Validaciones: DataAnnotations, verificacion NameIdentifier y CreateReservationRequestValidator.
        // Manejo de errores: Almacena mensaje en TempData y usa redireccion para mostrar feedback; excepciones llegan al middleware global.
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Details", "Events", new { id = model.EventId });
        }

        if (!Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Forbid();
        }

        var request = new Eventura.Application.DTOs.CreateReservationRequest
        {
            EventId = model.EventId,
            UserId = userId,
            Quantity = model.Quantity
        };

        var result = await _reservationService.CreateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            TempData["Success"] = "Reserva creada.";
            // TODO(aprendizaje): añadir envio de correo real usando IEmailSender al confirmar reserva.
        }

        return RedirectToAction("Details", "Events", new { id = model.EventId });
    }

    public async Task<IActionResult> MyReservations(CancellationToken cancellationToken)
    {
        // Contexto: Vista que lista reservas del usuario autenticado.
        // Intencion: Combinar datos de reservas con titulos de eventos evitando exponer entidades.
        // Pasos: 1) Validar usuario; 2) Obtener reservas via servicio; 3) Resolver titulos de eventos con llamadas adicionales; 4) Construir ViewModel; 5) Retornar vista.
        // Validaciones: Requiere Claim NameIdentifier valido.
        // Manejo de errores: Devuelve Forbid si no se encuentra el claim; otras excepciones se manejan globalmente.
        if (!Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
        {
            return Forbid();
        }

        var reservations = await _reservationService.GetByUserAsync(userId, cancellationToken).ConfigureAwait(false);
        var eventLookup = new Dictionary<Guid, string>();

        foreach (var reservation in reservations)
        {
            if (eventLookup.ContainsKey(reservation.EventId))
            {
                continue;
            }

            var eventResult = await _eventService.GetByIdAsync(reservation.EventId, cancellationToken).ConfigureAwait(false);
            if (eventResult.Succeeded && eventResult.Data is not null)
            {
                eventLookup[reservation.EventId] = eventResult.Data.Title;
            }
            else
            {
                eventLookup[reservation.EventId] = "Evento";
            }
        }

        var model = reservations.Select(r => new ReservationListItemViewModel
        {
            Id = r.Id,
            EventTitle = eventLookup.TryGetValue(r.EventId, out var title) ? title : "Evento",
            ReservedAt = r.ReservedAt,
            Quantity = r.Quantity,
            IsCancelled = r.IsCancelled
        }).ToList();

        return View(model);
    }
}
