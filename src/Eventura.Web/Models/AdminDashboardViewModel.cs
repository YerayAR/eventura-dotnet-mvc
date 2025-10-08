namespace Eventura.Web.Models;

/// <summary>
/// Capa: Web.
/// Proposito: Presentar metricas resumidas en el panel administrativo.
/// Responsabilidades: Contener valores listos para ser renderizados en la vista Dashboard.
/// Dependencias/Puertos utilizados: Se llena a partir de DashboardMetricsDto.
/// Limites (lo que NO debe hacer): Ejecutar calculos o consultas adicionales.
/// Errores comunes: No sincronizar propiedades con los datos devueltos por la capa Application.
/// </summary>
public sealed class AdminDashboardViewModel
{
    public int TotalEvents { get; init; }
    public int UpcomingEvents { get; init; }
    public int TotalReservations { get; init; }
    public int CancelledReservations { get; init; }
}
