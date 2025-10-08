using Eventura.Application.Abstractions;
using Eventura.Application.DTOs;
using Eventura.Domain.Repositories;

namespace Eventura.Application.Services;

/// <summary>
/// Capa: Application.
/// Propósito: DTO utilizado para transportar métricas al panel de administración.
/// Responsabilidades: Representar recuentos agregados sin lógica adicional.
/// Dependencias/Puertos utilizados: No interactúa directamente con puertos; recibe datos del servicio.
/// Límites (lo que NO debe hacer): No debe contener lógica de negocio ni exponer entidades.
/// Errores comunes: Añadir comportamientos aquí en lugar de mantenerlo como DTO inmutable.
/// </summary>
public sealed record DashboardMetricsDto
{
    public int TotalEvents { get; init; }
    public int UpcomingEvents { get; init; }
    public int TotalReservations { get; init; }
    public int CancelledReservations { get; init; }
}

/// <summary>
/// Capa: Application.
/// Propósito: Contrato para proveer métricas de administración a la capa Web.
/// Responsabilidades: Calcular estadísticas agregadas basadas en repositorios de dominio.
/// Dependencias/Puertos utilizados: Repositorios de eventos y reservas, proveedor de fecha y hora.
/// Límites (lo que NO debe hacer): No debe renderizar vistas ni acceder a infraestructura concreta.
/// Errores comunes: Realizar consultas pesadas fuera de esta capa o duplicar lógica de dominio.
/// </summary>
public interface IAdminDashboardService
{
    Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Capa: Application.
/// Propósito: Implementar la agregación de métricas para panel administrativo.
/// Responsabilidades: Orquestar repositorios, aplicar filtros temporales y devolver DTO de métricas.
/// Dependencias/Puertos utilizados: IEventRepository, IReservationRepository, IDateTimeProvider.
/// Límites (lo que NO debe hacer): No caché propio; delega esa responsabilidad a infraestructura.
/// Errores comunes: No contemplar zonas horarias o mezclar lógica de presentación.
/// </summary>
public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly IEventRepository _eventRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AdminDashboardService(
        IEventRepository eventRepository,
        IReservationRepository reservationRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _eventRepository = eventRepository;
        _reservationRepository = reservationRepository;
        _dateTimeProvider = dateTimeProvider;
    }

#region Aprendizaje
// Este caso de uso muestra cómo la capa Application agrega datos utilizando puertos.
// Aquí podría añadirse caching en Infrastructure para optimizar. El servicio evita referencias a
// frameworks UI y mantiene DTOs separados de entidades, reforzando controladores delgados.
// TODO(aprendizaje): incorporar métricas de tasa de conversión y persistirlas en caché para comparar rendimiento.
#endregion

    public async Task<DashboardMetricsDto> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        // Contexto: Panel de administración solicitando métricas para dashboards.
        // Intención: Calcular métricas centrales de eventos y reservas en tiempo real.
        // Pasos: 1) Obtener fecha actual; 2) Consultar eventos próximos; 3) Consultar reservas; 4) Calcular totales; 5) Construir DTO.
        // Validaciones: Usa proveedores para garantizar consistencia temporal; asume repositorios válidos.
        // Manejo de errores: Cualquier excepción de repositorio es capturada por middleware global.
        var now = _dateTimeProvider.UtcNow;
        var upcoming = await _eventRepository.GetUpcomingAsync(now, cancellationToken).ConfigureAwait(false);
        var reservations = await _reservationRepository.GetByEventAsync(Guid.Empty, cancellationToken).ConfigureAwait(false);

        // When Guid.Empty is passed, repository should return all reservations.
        var totalReservations = reservations.Count;
        var cancelledReservations = reservations.Count(r => r.IsCancelled);

        return new DashboardMetricsDto
        {
            TotalEvents = upcoming.Count,
            UpcomingEvents = upcoming.Count(e => e.StartDateTime >= now),
            TotalReservations = totalReservations,
            CancelledReservations = cancelledReservations
        };
    }
}
