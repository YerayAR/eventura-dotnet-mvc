using Eventura.Application.Services;
using Eventura.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventura.Web.Controllers;

/// <summary>
/// Capa: Web.
/// Proposito: Exponer funcionalidades administrativas a usuarios con rol adecuado.
/// Responsabilidades: Invocar IAdminDashboardService y proyectar resultados a ViewModels.
/// Dependencias/Puertos utilizados: IAdminDashboardService.
/// Limites (lo que NO debe hacer): Ejecutar consultas directas a repositorios ni mezclar logica de agregacion.
/// Errores comunes: No proteger la accion con politicas de rol o no manejar datos agregados nulos.
/// </summary>
[Authorize(Policy = "RequireAdmin")]
public sealed class AdminController : Controller
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

#region Aprendizaje
// Flujo MVC: autorizacion por rol -> controlador -> caso de uso -> DTO -> ViewModel.
// Destaca como la capa Web permanece agnostica de persistencia y se centra en presentar datos.
// TODO(aprendizaje): anadir grafico en la vista y cubrirlo con prueba que asegure autorizacion.
#endregion

    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        // Contexto: Vista principal del panel administrador.
        // Intencion: Obtener metricas agregadas y mostrarlas en la vista.
        // Pasos: 1) Llamar a GetMetricsAsync; 2) Mapear DashboardMetricsDto -> AdminDashboardViewModel; 3) Retornar vista.
        // Validaciones: Politica RequireAdmin impide acceso no autorizado.
        // Manejo de errores: Excepciones se capturan por middleware y se redirigen a Error.
        var metrics = await _dashboardService.GetMetricsAsync(cancellationToken).ConfigureAwait(false);
        var model = new AdminDashboardViewModel
        {
            TotalEvents = metrics.TotalEvents,
            UpcomingEvents = metrics.UpcomingEvents,
            TotalReservations = metrics.TotalReservations,
            CancelledReservations = metrics.CancelledReservations
        };

        // TODO(aprendizaje): extender métricas con tasa de cancelación y analizar cache per-request.
        return View(model);
    }
}
