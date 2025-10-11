using Eventura.Application.DTOs;

namespace Eventura.Application.Abstractions;

/// <summary>
/// Capa: Application.
/// Propósito: Definir contrato para obtener eventos desde APIs externas.
/// Responsabilidades: Abstraer la obtención de datos de eventos reales para enriquecer la aplicación.
/// Dependencias/Puertos utilizados: Trabaja con DTOs de eventos externos.
/// Límites (lo que NO debe hacer): No debe incluir lógica específica de APIs o transformaciones complejas.
/// Errores comunes: Asumir estructura específica de APIs externas en la interfaz.
/// </summary>
public interface IExternalEventsService
{
    /// <summary>
    /// Obtiene eventos desde una fuente externa.
    /// </summary>
    /// <param name="location">Ubicación para filtrar eventos (opcional)</param>
    /// <param name="category">Categoría para filtrar eventos (opcional)</param>
    /// <param name="maxResults">Número máximo de resultados</param>
    /// <returns>Lista de eventos externos</returns>
    Task<IReadOnlyList<ExternalEventDto>> GetEventsAsync(
        string? location = null, 
        string? category = null, 
        int maxResults = 20);

    /// <summary>
    /// Obtiene eventos populares/destacados.
    /// </summary>
    /// <param name="maxResults">Número máximo de resultados</param>
    /// <returns>Lista de eventos populares</returns>
    Task<IReadOnlyList<ExternalEventDto>> GetPopularEventsAsync(int maxResults = 10);
}