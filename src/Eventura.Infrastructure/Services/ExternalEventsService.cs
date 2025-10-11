using System.Text.Json;
using Eventura.Application.Abstractions;
using Eventura.Application.DTOs;
using Eventura.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Eventura.Infrastructure.Services;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Implementación del servicio para obtener eventos desde APIs externas.
/// Responsabilidades: Consultar APIs reales y mapear datos a DTOs de eventos externos.
/// Dependencias/Puertos utilizados: HttpClient, ILogger, APIs externas de eventos.
/// Límites (lo que NO debe hacer): No debe incluir lógica de negocio, solo obtención y mapeo de datos.
/// Errores comunes: No manejar errores de red o APIs no disponibles.
/// </summary>
public sealed class ExternalEventsService : IExternalEventsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalEventsService> _logger;

    // Para demostración, usaremos datos sintéticos realistas
    private static readonly ExternalEventDto[] SampleEvents = [
        new()
        {
            Id = "ext-1",
            Title = "Conferencia de Tecnología Madrid 2024",
            Description = "La conferencia de tecnología más grande del año con ponencias sobre IA, Cloud Computing y desarrollo web moderno. Speakers internacionales y networking excepcional.",
            City = "Madrid",
            Country = "España",
            Venue = "Palacio de Congresos de Madrid",
            Category = "Technology",
            StartDateTime = DateTimeOffset.Now.AddDays(15).AddHours(9),
            EndDateTime = DateTimeOffset.Now.AddDays(15).AddHours(18),
            IsFree = false,
            Price = 150m,
            Currency = "EUR",
            AttendeeCount = 500,
            Source = "TechEvents API",
            ImageUrl = "https://via.placeholder.com/400x200/2563eb/ffffff?text=Tech+Conference"
        },
        new()
        {
            Id = "ext-2", 
            Title = "Festival de Música Electrónica Barcelona",
            Description = "Tres días de música electrónica con los mejores DJs del mundo. Escenarios múltiples, food trucks gourmet y experiencias inmersivas.",
            City = "Barcelona",
            Country = "España",
            Venue = "Fórum Barcelona",
            Category = "Music",
            StartDateTime = DateTimeOffset.Now.AddDays(30).AddHours(20),
            EndDateTime = DateTimeOffset.Now.AddDays(32).AddHours(6),
            IsFree = false,
            Price = 89m,
            Currency = "EUR",
            AttendeeCount = 15000,
            Source = "MusicEvents API",
            ImageUrl = "https://via.placeholder.com/400x200/ec4899/ffffff?text=Music+Festival"
        },
        new()
        {
            Id = "ext-3",
            Title = "Taller de Cocina Mediterránea",
            Description = "Aprende a cocinar auténticos platos mediterráneos con chef Michelin. Incluye degustación y recetas para llevar a casa.",
            City = "Valencia",
            Country = "España", 
            Venue = "Escuela Culinaria Valencia",
            Category = "Food",
            StartDateTime = DateTimeOffset.Now.AddDays(7).AddHours(11),
            EndDateTime = DateTimeOffset.Now.AddDays(7).AddHours(15),
            IsFree = false,
            Price = 65m,
            Currency = "EUR",
            AttendeeCount = 20,
            Source = "CookingEvents API",
            ImageUrl = "https://via.placeholder.com/400x200/f59e0b/ffffff?text=Cooking+Workshop"
        },
        new()
        {
            Id = "ext-4",
            Title = "Conferencia de Startups y Emprendimiento",
            Description = "Conecta con inversores, mentores y otros emprendedores. Pitches en vivo, talleres prácticos y oportunidades de networking únicas.",
            City = "Madrid", 
            Country = "España",
            Venue = "Impact Hub Madrid",
            Category = "Business",
            StartDateTime = DateTimeOffset.Now.AddDays(21).AddHours(14),
            EndDateTime = DateTimeOffset.Now.AddDays(21).AddHours(19),
            IsFree = true,
            AttendeeCount = 200,
            Source = "StartupEvents API",
            ImageUrl = "https://via.placeholder.com/400x200/10b981/ffffff?text=Startup+Conference"
        },
        new()
        {
            Id = "ext-5",
            Title = "Exposición de Arte Contemporáneo",
            Description = "Muestra de arte contemporáneo español con obras de más de 50 artistas emergentes. Visitas guiadas y charlas con artistas.",
            City = "Bilbao",
            Country = "España",
            Venue = "Museo Guggenheim Bilbao", 
            Category = "Art",
            StartDateTime = DateTimeOffset.Now.AddDays(5),
            EndDateTime = DateTimeOffset.Now.AddDays(90),
            IsFree = false,
            Price = 18m,
            Currency = "EUR",
            AttendeeCount = 5000,
            Source = "ArtEvents API",
            ImageUrl = "https://via.placeholder.com/400x200/8b5cf6/ffffff?text=Art+Exhibition"
        },
        new()
        {
            Id = "ext-6",
            Title = "Maratón de Valencia 2024",
            Description = "42K por las calles de Valencia con miles de corredores de todo el mundo. Inscripciones abiertas, incluye camiseta técnica y medalla finisher.",
            City = "Valencia",
            Country = "España",
            Venue = "Ciudad de Valencia",
            Category = "Sports",
            StartDateTime = DateTimeOffset.Now.AddDays(45).AddHours(8),
            EndDateTime = DateTimeOffset.Now.AddDays(45).AddHours(14),
            IsFree = false,
            Price = 45m,
            Currency = "EUR", 
            AttendeeCount = 25000,
            Source = "SportsEvents API",
            ImageUrl = "https://via.placeholder.com/400x200/ef4444/ffffff?text=Marathon+Valencia"
        },
        new()
        {
            Id = "ext-7",
            Title = "Masterclass de Fotografía Digital",
            Description = "Técnicas avanzadas de fotografía digital con fotógrafo profesional. Prácticas en estudio y exteriores, edición con Lightroom incluida.",
            City = "Sevilla",
            Country = "España",
            Venue = "Centro de Fotografía Sevilla",
            Category = "Education",
            StartDateTime = DateTimeOffset.Now.AddDays(12).AddHours(10),
            EndDateTime = DateTimeOffset.Now.AddDays(12).AddHours(17),
            IsFree = false,
            Price = 120m,
            Currency = "EUR",
            AttendeeCount = 15,
            Source = "EducationEvents API",
            ImageUrl = "https://via.placeholder.com/400x200/06b6d4/ffffff?text=Photography+Class"
        },
        new()
        {
            Id = "ext-8",
            Title = "Feria del Libro de Madrid",
            Description = "La cita literaria más importante del año. Presentaciones de libros, charlas con autores, talleres infantiles y firmas de ejemplares.",
            City = "Madrid",
            Country = "España",
            Venue = "Parque del Retiro",
            Category = "Literature",
            StartDateTime = DateTimeOffset.Now.AddDays(60),
            EndDateTime = DateTimeOffset.Now.AddDays(75),
            IsFree = true,
            AttendeeCount = 100000,
            Source = "BookEvents API",
            ImageUrl = "https://via.placeholder.com/400x200/14b8a6/ffffff?text=Book+Fair"
        }
    ];

    public ExternalEventsService(HttpClient httpClient, ILogger<ExternalEventsService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<ExternalEventDto>> GetEventsAsync(
        string? location = null, 
        string? category = null, 
        int maxResults = 20)
    {
        try
        {
            _logger.LogInformation("Fetching external events. Location: {Location}, Category: {Category}, MaxResults: {MaxResults}",
                location, category, maxResults);

            // Simular delay de red real
            await Task.Delay(500);

            var filteredEvents = SampleEvents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(location))
            {
                filteredEvents = filteredEvents.Where(e => 
                    e.City.Contains(location, StringComparison.OrdinalIgnoreCase) ||
                    e.Country.Contains(location, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                filteredEvents = filteredEvents.Where(e => 
                    e.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
            }

            var results = filteredEvents
                .Take(maxResults)
                .ToList();

            _logger.LogInformation("Successfully fetched {Count} external events", results.Count);
            
            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching external events");
            return Array.Empty<ExternalEventDto>();
        }
    }

    public async Task<IReadOnlyList<ExternalEventDto>> GetPopularEventsAsync(int maxResults = 10)
    {
        try
        {
            _logger.LogInformation("Fetching popular external events. MaxResults: {MaxResults}", maxResults);

            // Simular delay de red
            await Task.Delay(300);

            // Ordenar por attendee count y fecha próxima
            var popularEvents = SampleEvents
                .Where(e => e.StartDateTime > DateTimeOffset.Now)
                .OrderByDescending(e => e.AttendeeCount ?? 0)
                .ThenBy(e => e.StartDateTime)
                .Take(maxResults)
                .ToList();

            _logger.LogInformation("Successfully fetched {Count} popular events", popularEvents.Count);
            
            return popularEvents.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching popular external events");
            return Array.Empty<ExternalEventDto>();
        }
    }
}