using Eventura.Application.Abstractions;
using Eventura.Application.DTOs;
using Eventura.Domain.Entities;
using Eventura.Domain.Enums;
using Eventura.Domain.Repositories;
using Eventura.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Eventura.Application.Services;

/// <summary>
/// Capa: Application.
/// Propósito: Servicio para importar eventos externos al dominio de la aplicación.
/// Responsabilidades: Mapear eventos externos a entidades de dominio y coordinar su persistencia.
/// Dependencias/Puertos utilizados: IExternalEventsService, IEventRepository, IUnitOfWork.
/// Límites (lo que NO debe hacer): No debe acceder directamente a APIs ni manejar persistencia específica.
/// Errores comunes: No validar datos externos antes de crear entidades de dominio.
/// </summary>
public sealed class ExternalEventImportService
{
    private readonly IExternalEventsService _externalEventsService;
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExternalEventImportService> _logger;

    public ExternalEventImportService(
        IExternalEventsService externalEventsService,
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork,
        ILogger<ExternalEventImportService> logger)
    {
        _externalEventsService = externalEventsService ?? throw new ArgumentNullException(nameof(externalEventsService));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Importa eventos populares desde fuentes externas.
    /// </summary>
    /// <param name="maxEvents">Número máximo de eventos a importar</param>
    /// <returns>Resultado de la operación con eventos importados</returns>
    public async Task<OperationResult<int>> ImportPopularEventsAsync(int maxEvents = 5)
    {
        try
        {
            _logger.LogInformation("Starting import of popular external events. MaxEvents: {MaxEvents}", maxEvents);

            var externalEvents = await _externalEventsService.GetPopularEventsAsync(maxEvents);
            var importedCount = 0;

            foreach (var externalEvent in externalEvents)
            {
                try
                {
                    var domainEvent = await MapToDomainEventAsync(externalEvent);
                    if (domainEvent != null)
                    {
                        await _eventRepository.AddAsync(domainEvent);
                        importedCount++;
                        _logger.LogDebug("Mapped and added event: {Title}", externalEvent.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import event: {Title}", externalEvent.Title);
                    // Continuar con los demás eventos
                }
            }

            if (importedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully imported {Count} events", importedCount);
            }

            return OperationResult<int>.Success(importedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing external events");
            return OperationResult<int>.Failure("Error al importar eventos externos.");
        }
    }

    /// <summary>
    /// Importa eventos de una ubicación específica.
    /// </summary>
    /// <param name="location">Ubicación para filtrar eventos</param>
    /// <param name="maxEvents">Número máximo de eventos a importar</param>
    /// <returns>Resultado de la operación</returns>
    public async Task<OperationResult<int>> ImportEventsByLocationAsync(string location, int maxEvents = 10)
    {
        try
        {
            _logger.LogInformation("Importing events for location: {Location}, MaxEvents: {MaxEvents}", 
                location, maxEvents);

            var externalEvents = await _externalEventsService.GetEventsAsync(location, null, maxEvents);
            var importedCount = 0;

            foreach (var externalEvent in externalEvents)
            {
                try
                {
                    var domainEvent = await MapToDomainEventAsync(externalEvent);
                    if (domainEvent != null)
                    {
                        await _eventRepository.AddAsync(domainEvent);
                        importedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to import event: {Title}", externalEvent.Title);
                }
            }

            if (importedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return OperationResult<int>.Success(importedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing events by location: {Location}", location);
            return OperationResult<int>.Failure("Error al importar eventos por ubicación.");
        }
    }

    private async Task<Event?> MapToDomainEventAsync(ExternalEventDto externalEvent)
    {
        try
        {
            // Validar datos mínimos requeridos
            if (string.IsNullOrWhiteSpace(externalEvent.Title) || 
                !externalEvent.StartDateTime.HasValue ||
                string.IsNullOrWhiteSpace(externalEvent.City))
            {
                _logger.LogWarning("External event missing required data: {Title}", externalEvent.Title);
                return null;
            }

            // Verificar si ya existe un evento similar para evitar duplicados
            var existingEvents = await _eventRepository.GetByTitleAsync(externalEvent.Title);
            if (existingEvents.Any())
            {
                _logger.LogDebug("Event already exists, skipping: {Title}", externalEvent.Title);
                return null;
            }

            // Mapear categoría externa a categoría del dominio
            var category = MapCategory(externalEvent.Category);
            
            // Crear ubicación
            var location = Location.Create(externalEvent.City, externalEvent.Venue ?? "Por determinar");
            
            // Calcular duración (default 2 horas si no se especifica)
            var duration = externalEvent.EndDateTime.HasValue 
                ? externalEvent.EndDateTime.Value - externalEvent.StartDateTime.Value
                : TimeSpan.FromHours(2);

            // Asegurar que la duración mínima sea de 15 minutos
            if (duration.TotalMinutes < 15)
            {
                duration = TimeSpan.FromHours(2);
            }

            // Generar descripción enriquecida
            var description = BuildDescription(externalEvent);

            // Crear evento de dominio
            var domainEvent = Event.Create(
                title: externalEvent.Title,
                description: description,
                startDateTime: externalEvent.StartDateTime.Value,
                duration: duration,
                location: location,
                capacity: CalculateCapacity(externalEvent),
                category: category);

            return domainEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping external event to domain: {Title}", externalEvent.Title);
            return null;
        }
    }

    private static EventCategory MapCategory(string externalCategory)
    {
        return externalCategory.ToLowerInvariant() switch
        {
            "technology" or "tech" => EventCategory.Technology,
            "music" or "concert" or "festival" => EventCategory.Music,
            "food" or "cooking" or "culinary" => EventCategory.Community,
            "business" or "startup" or "entrepreneur" => EventCategory.Community,
            "art" or "exhibition" or "gallery" => EventCategory.Art,
            "sports" or "marathon" or "fitness" => EventCategory.Sports,
            "education" or "learning" or "workshop" => EventCategory.Education,
            "literature" or "book" or "reading" => EventCategory.Art,
            _ => EventCategory.Other
        };
    }

    private static string BuildDescription(ExternalEventDto externalEvent)
    {
        var description = externalEvent.Description;
        
        if (!string.IsNullOrWhiteSpace(externalEvent.Venue))
        {
            description += $"\n\n📍 Ubicación: {externalEvent.Venue}";
        }

        if (externalEvent.Price.HasValue && !externalEvent.IsFree)
        {
            description += $"\n💰 Precio: {externalEvent.Price:F2} {externalEvent.Currency ?? "EUR"}";
        }
        else if (externalEvent.IsFree)
        {
            description += "\n🆓 Evento gratuito";
        }

        if (externalEvent.AttendeeCount.HasValue)
        {
            description += $"\n👥 Capacidad esperada: {externalEvent.AttendeeCount:N0} asistentes";
        }

        if (!string.IsNullOrWhiteSpace(externalEvent.Source))
        {
            description += $"\n\n📡 Fuente: {externalEvent.Source}";
        }

        return description;
    }

    private static int CalculateCapacity(ExternalEventDto externalEvent)
    {
        // Usar attendee count como base, con límites razonables
        if (externalEvent.AttendeeCount.HasValue)
        {
            var capacity = externalEvent.AttendeeCount.Value;
            
            // Limitar capacidad entre 10 y 10000
            return Math.Max(10, Math.Min(capacity, 10000));
        }

        // Capacidad default basada en categoría
        return externalEvent.Category.ToLowerInvariant() switch
        {
            "music" or "festival" => 5000,
            "sports" or "marathon" => 1000,
            "business" or "conference" => 200,
            "education" or "workshop" => 50,
            "art" or "exhibition" => 500,
            _ => 100
        };
    }
}