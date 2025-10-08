using Eventura.Application.Abstractions;
using Eventura.Application.DTOs;
using Eventura.Application.Validators;
using Eventura.Domain.Entities;
using Eventura.Domain.Enums;
using Eventura.Domain.Repositories;
using Eventura.Domain.ValueObjects;

namespace Eventura.Application.Services;

/// <summary>
/// Capa: Application.
/// Propósito: Define el contrato del caso de uso para gestionar eventos.
/// Responsabilidades: Exponer operaciones de creación, actualización, consulta y eliminación de eventos para la capa Web.
/// Dependencias/Puertos utilizados: Utiliza repositorios de eventos y unidades de trabajo a través de interfaces de dominio.
/// Límites (lo que NO debe hacer): No debe conocer detalles de infraestructura ni lógica de presentación.
/// Errores comunes: Confundir DTOs con entidades de dominio o realizar validaciones duplicadas con la capa Web.
/// </summary>
public interface IEventService
{
    Task<OperationResult<EventDto>> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<EventDto>> UpdateAsync(UpdateEventRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OperationResult<EventDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetUpcomingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventDto>> GetRecommendationsAsync(RecommendationRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Capa: Application.
/// Propósito: Implementa el caso de uso que orquesta la lógica de negocio de eventos.
/// Responsabilidades: Coordinar validaciones, invocar repositorios, mantener invariantes y mapear entidades a DTOs.
/// Dependencias/Puertos utilizados: IEventRepository, IUnitOfWork, IDateTimeProvider como puertos de dominio.
/// Límites (lo que NO debe hacer): No debe acceder a frameworks web ni exponer entidades de dominio hacia la capa de presentación.
/// Errores comunes: Olvidar confirmar la unidad de trabajo o no respetar la inmutabilidad de DTOs.
/// </summary>
public sealed class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public EventService(
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

#region Aprendizaje
// Esta clase ejemplifica la inversión de dependencias: recibe puertos (interfaces) definidos en Domain/Application
// y delega el acceso a datos a través de infraestructura. Actúa como puente entre controladores (Web) y entidades de dominio,
// garantizando un flujo DTO -> Caso de uso -> Entidad -> Persistencia. También encapsula la gestión de transacciones
// a través de IUnitOfWork para mantener consistencia (commit/rollback) y prepara datos serializables para la Web.
#endregion

    public async Task<OperationResult<EventDto>> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default)
    {
        // Contexto: Expuesto a la capa Web cuando un organizador solicita crear un evento.
        // Intención: Validar entrada, construir entidad consistente y persistirla atómicamente.
        // Pasos: 1) Validar DTO de entrada; 2) Mapear a value objects y entidad; 3) Guardar en repositorio; 4) Confirmar transacción; 5) Retornar DTO.
        // Validaciones: Reglas de CreateEventRequestValidator para fechas, capacidad y datos obligatorios.
        // Manejo de errores: Devuelve OperationResult de fallo con mensaje si la validación o persistencia no son correctas.
        var validationResult = CreateEventRequestValidator.Validate(request, _dateTimeProvider.UtcNow);
        if (!validationResult.Succeeded)
        {
            return OperationResult<EventDto>.Failure(validationResult.Error!);
        }

        var location = Location.Create(request.City, request.AddressLine);
        var @event = Event.Create(
            request.Title,
            request.Description,
            request.StartDateTime,
            request.Duration,
            location,
            request.Capacity,
            request.Category);

        await _eventRepository.AddAsync(@event, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return OperationResult<EventDto>.Success(MapToDto(@event));
    }

    public async Task<OperationResult<EventDto>> UpdateAsync(UpdateEventRequest request, CancellationToken cancellationToken = default)
    {
        // Contexto: La capa Web actualiza eventos existentes manteniendo la integridad del historial.
        // Intención: Validar cambios y aplicar modificaciones sin romper invariantes (capacidad, fechas, etc.).
        // Pasos: 1) Validar que el identificador exista; 2) Revalidar datos; 3) Recuperar evento; 4) Aplicar cambios; 5) Persistir; 6) Retornar DTO actualizado.
        // Validaciones: Comprueba GUID válido y reutiliza CreateEventRequestValidator para consistencia.
        // Manejo de errores: Respuestas OperationResult con mensajes específicos cuando el evento no existe o las reglas fallan.
        if (request is null || request.Id == Guid.Empty)
        {
            return OperationResult<EventDto>.Failure("Invalid event identifier.");
        }

        var validationResult = CreateEventRequestValidator.Validate(request, _dateTimeProvider.UtcNow);
        if (!validationResult.Succeeded)
        {
            return OperationResult<EventDto>.Failure(validationResult.Error!);
        }

        var existing = await _eventRepository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            return OperationResult<EventDto>.Failure("Event not found.");
        }

        var location = Location.Create(request.City, request.AddressLine);
        existing.UpdateDetails(
            request.Title,
            request.Description,
            request.StartDateTime,
            request.Duration,
            location,
            request.Capacity,
            request.Category);

        await _eventRepository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return OperationResult<EventDto>.Success(MapToDto(existing));
    }

    public async Task<OperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Contexto: Se invoca desde la capa Web para eliminar eventos mediante confirmación explícita.
        // Intención: Borrar un evento preservando consistencia en repositorio y unidad de trabajo.
        // Pasos: 1) Validar id; 2) Ejecutar eliminación en repositorio; 3) Guardar cambios; 4) Responder éxito.
        // Validaciones: Comprueba que el identificador no sea Guid.Empty.
        // Manejo de errores: Devuelve resultado fallido cuando la entrada no es válida, delega excepciones de repositorio a la capa superior.
        if (id == Guid.Empty)
        {
            return OperationResult.Failure("Invalid event identifier.");
        }

        await _eventRepository.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult.Success();
    }

    public async Task<OperationResult<EventDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Contexto: Consultas puntuales desde controladores para mostrar detalle de eventos.
        // Intención: Recuperar evento y mapearlo a DTO sin exponer la entidad.
        // Pasos: 1) Validar id; 2) Solicitar al repositorio; 3) Evaluar existencia; 4) Mapear a DTO; 5) Retornar resultado.
        // Validaciones: Identificador distinto de Guid.Empty y existencia en repositorio.
        // Manejo de errores: Devuelve OperationResult con mensajes cuando no se encuentra o id inválido.
        if (id == Guid.Empty)
        {
            return OperationResult<EventDto>.Failure("Invalid event identifier.");
        }

        var @event = await _eventRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (@event is null)
        {
            return OperationResult<EventDto>.Failure("Event not found.");
        }

        return OperationResult<EventDto>.Success(MapToDto(@event));
    }

    public async Task<IReadOnlyList<EventDto>> GetUpcomingAsync(CancellationToken cancellationToken = default)
    {
        // Contexto: Alimenta listados públicos de eventos futuros en la capa de presentación.
        // Intención: Obtener eventos a partir de la fecha actual sin lógica extra en el controlador.
        // Pasos: 1) Consultar repositorio con fecha actual; 2) Proyectar entidades a DTO; 3) Devolver lista inmutable.
        // Validaciones: No aplica adicionales; el repositorio ya filtra por fechas.
        // Manejo de errores: Excepciones burbujean para ser capturadas por middleware de errores global.
        var events = await _eventRepository.GetUpcomingAsync(_dateTimeProvider.UtcNow, cancellationToken).ConfigureAwait(false);
        return events.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<EventDto>> GetRecommendationsAsync(RecommendationRequest request, CancellationToken cancellationToken = default)
    {
        // Contexto: Función de recomendación básica consumida por la vista Recommendations.
        // Intención: Filtrar eventos según preferencias ligeras del usuario.
        // Pasos: 1) Normalizar parámetros; 2) Delegar búsqueda al repositorio; 3) Mapear resultado a DTOs.
        // Validaciones: Sanea ciudad eliminando espacios y permite categoría opcional.
        // Manejo de errores: Uso de OperationResult no aplica aquí; posibles fallos via excepciones.
        // TODO(aprendizaje): agregar ordenación al listado de recomendaciones e incluir prueba de integración.
        string? city = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
        EventCategory? category = request.Category;

        var events = await _eventRepository.SearchAsync(city, category, cancellationToken).ConfigureAwait(false);
        return events.Select(MapToDto).ToList();
    }

    private static EventDto MapToDto(Event @event)
    {
        // Contexto: Conversión necesaria para separar la capa Web de las entidades de dominio.
        // Intención: Producir un DTO plano seguro para la capa de presentación.
        // Pasos: 1) Leer propiedades de la entidad; 2) Armar un nuevo DTO; 3) Exponer solo datos necesarios.
        // Validaciones: Confía en que la entidad ya está validada.
        // Manejo de errores: No lanza; delega en la integridad previa de la entidad.
        return new EventDto
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartDateTime = @event.StartDateTime,
            Duration = @event.Duration,
            City = @event.Location.City,
            AddressLine = @event.Location.AddressLine,
            Capacity = @event.Capacity,
            RemainingCapacity = @event.RemainingCapacity,
            Category = @event.Category,
            IsCancelled = @event.IsCancelled
        };
    }
}
