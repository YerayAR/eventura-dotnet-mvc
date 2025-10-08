using Eventura.Application.DTOs;
using Eventura.Application.Validators;
using Eventura.Domain.Entities;
using Eventura.Domain.Repositories;

namespace Eventura.Application.Services;

/// <summary>
/// Capa: Application.
/// Propósito: Define operaciones de negocio para reservas consumidas por la capa Web.
/// Responsabilidades: Formalizar la creación, cancelación y consulta de reservas.
/// Dependencias/Puertos utilizados: Repositorios de reservas, eventos y usuarios; unidad de trabajo.
/// Límites (lo que NO debe hacer): No debe manipular detalles de UI ni gestionar sesiones de usuario.
/// Errores comunes: Saltarse validaciones cruzadas o dejar reservas sin persistir en la misma transacción.
/// </summary>
public interface IReservationService
{
    Task<OperationResult<ReservationDto>> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> CancelAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReservationDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Capa: Application.
/// Propósito: Implementa la lógica de orquestación para reservar plazas en eventos.
/// Responsabilidades: Coordinar invariantes de dominio, persistencia y mapeos a DTO.
/// Dependencias/Puertos utilizados: IReservationRepository, IEventRepository, IUserRepository, IUnitOfWork.
/// Límites (lo que NO debe hacer): No exponer entidades ni depender de infraestructura concreta.
/// Errores comunes: No revertir cambios cuando ocurre una excepción o no actualizar el evento asociado.
/// </summary>
public sealed class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationService(
        IReservationRepository reservationRepository,
        IEventRepository eventRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

#region Aprendizaje
// Ejemplo de servicio de aplicación que garantiza la consistencia entre entidades de dominio
// y la persistencia. Aquí se aplican principios de controladores delgados: la Web transforma la
// entrada en CreateReservationRequest y delega la orquestación. También es donde se coordina la unidad
// de trabajo para asegurar transacciones lógicas y se mantiene la separación DTOs vs entidades.
#endregion

    public async Task<OperationResult<ReservationDto>> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        // Contexto: Flujo de reserva iniciado desde un controlador tras validar modelo MVC.
        // Intención: Garantizar que el usuario y el evento existen y crear la reserva respetando la capacidad.
        // Pasos: 1) Validar DTO; 2) Obtener evento; 3) Obtener usuario; 4) Ejecutar método de dominio Reserve; 5) Persistir y confirmar unidad de trabajo; 6) Mapear resultado.
        // Validaciones: Reglas de CreateReservationRequestValidator y chequeos de existencia en repositorios.
        // Manejo de errores: Captura excepciones de dominio y devuelve OperationResult con mensaje descriptivo.
        var validationResult = CreateReservationRequestValidator.Validate(request);
        if (!validationResult.Succeeded)
        {
            return OperationResult<ReservationDto>.Failure(validationResult.Error!);
        }

        var @event = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken).ConfigureAwait(false);
        if (@event is null)
        {
            return OperationResult<ReservationDto>.Failure("Event not found.");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            return OperationResult<ReservationDto>.Failure("User not found.");
        }

        Reservation reservation;
        try
        {
            reservation = @event.Reserve(user, request.Quantity);
        }
        catch (Exception ex)
        {
            return OperationResult<ReservationDto>.Failure(ex.Message);
        }

        await _reservationRepository.AddAsync(reservation, cancellationToken).ConfigureAwait(false);
        await _eventRepository.UpdateAsync(@event, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return OperationResult<ReservationDto>.Success(MapToDto(reservation));
    }

    public async Task<OperationResult> CancelAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        // Contexto: Acción invocada desde la Web cuando un usuario decide anular su reserva.
        // Intención: Marcar la reserva como cancelada y liberar capacidad en el evento.
        // Pasos: 1) Validar id; 2) Recuperar reserva; 3) Obtener evento; 4) Cancelar reserva en dominio; 5) Actualizar repositorios; 6) Confirmar unidad de trabajo.
        // Validaciones: Identificador válido y existencia de reserva/evento.
        // Manejo de errores: OperationResult con mensajes específicos ante identificadores inválidos o reservas inexistentes.
        if (reservationId == Guid.Empty)
        {
            return OperationResult.Failure("Invalid reservation identifier.");
        }

        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken).ConfigureAwait(false);
        if (reservation is null)
        {
            return OperationResult.Failure("Reservation not found.");
        }

        var @event = await _eventRepository.GetByIdAsync(reservation.EventId, cancellationToken).ConfigureAwait(false);
        if (@event is not null)
        {
            @event.CancelReservation(reservationId);
            await _eventRepository.UpdateAsync(@event, cancellationToken).ConfigureAwait(false);
        }

        reservation.Cancel();
        await _reservationRepository.UpdateAsync(reservation, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return OperationResult.Success();
    }

    public async Task<IReadOnlyList<ReservationDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Contexto: Consultas personales para que el usuario vea sus reservas.
        // Intención: Obtener un listado seguro y optimizado de reservas del usuario.
        // Pasos: 1) Validar id de usuario; 2) Consultar repositorio; 3) Mapear entidades a DTOs.
        // Validaciones: Se asegura de no ejecutar la consulta con Guid vacío.
        // Manejo de errores: Devuelve colección vacía en caso de entrada inválida; otras excepciones son manejadas por el middleware global.
        if (userId == Guid.Empty)
        {
            return Array.Empty<ReservationDto>();
        }

        var reservations = await _reservationRepository.GetByUserAsync(userId, cancellationToken).ConfigureAwait(false);
        return reservations.Select(MapToDto).ToList();
    }

    private static ReservationDto MapToDto(Reservation reservation)
    {
        // Contexto: Transformación interna antes de devolver datos a la capa Web.
        // Intención: Evitar filtrar entidades de dominio y exponer solamente la información necesaria.
        // Pasos: 1) Leer datos de la entidad; 2) Instanciar DTO; 3) Retornar resultado simple.
        // Validaciones: No requiere; la entidad preserva invariantes.
        // Manejo de errores: No aplica; la operación es determinística.
        return new ReservationDto
        {
            Id = reservation.Id,
            EventId = reservation.EventId,
            UserId = reservation.UserId,
            Quantity = reservation.Quantity,
            ReservedAt = reservation.ReservedAt,
            IsCancelled = reservation.IsCancelled
        };
    }
}
