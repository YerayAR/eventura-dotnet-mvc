using Eventura.Domain.Enums;
using Eventura.Domain.Exceptions;
using Eventura.Domain.ValueObjects;

namespace Eventura.Domain.Entities;

/// <summary>
/// Capa: Domain.
/// Propósito: Representa la raíz agregada de eventos y encapsula reglas de planificación.
/// Responsabilidades: Mantener invariantes sobre fechas, capacidad y reservas asociadas.
/// Dependencias/Puertos utilizados: Usa value objects como Location y colaboraciones con Reservation.
/// Límites (lo que NO debe hacer): No ejecuta acceso a datos ni depende de frameworks externos.
/// Errores comunes: Modificar propiedades directamente o exponer colecciones mutables hacia capas superiores.
/// </summary>
public sealed class Event
{
    private readonly List<Reservation> _reservations = new();

    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTimeOffset StartDateTime { get; private set; }
    public TimeSpan Duration { get; private set; }
    public Location Location { get; private set; }
    public int Capacity { get; private set; }
    public EventCategory Category { get; private set; }
    public bool IsCancelled { get; private set; }

    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Event()
    {
    }

    private Event(
        Guid id,
        string title,
        string description,
        DateTimeOffset startDateTime,
        TimeSpan duration,
        Location location,
        int capacity,
        EventCategory category)
    {
        Id = id;
        Title = title;
        Description = description;
        StartDateTime = startDateTime;
        Duration = duration;
        Location = location;
        Capacity = capacity;
        Category = category;
    }

#region Aprendizaje
// La entidad de dominio impone invariantes (capacidad, fechas, estado cancelado) manteniendo el dominio aislado.
// Este bloque destaca cómo la capa Domain permanece libre de dependencias de frameworks y sólo expone métodos
// que otros casos de uso pueden invocar. Se utiliza la colección interna para evitar modificaciones directas.
// TODO(aprendizaje): ampliar para soportar reglas de horarios solapados y escribir pruebas de concurrencia.
#endregion

    public static Event Create(
        string title,
        string description,
        DateTimeOffset startDateTime,
        TimeSpan duration,
        Location location,
        int capacity,
        EventCategory category)
    {
        // Contexto: Invocado por los servicios de aplicación cuando se crea un nuevo evento.
        // Intención: Instanciar un evento válido respetando todas las reglas de negocio de dominio.
        // Pasos: 1) Validar argumentos; 2) Generar identificador; 3) Normalizar textos; 4) Construir instancia.
        // Validaciones: Se apoya en el método estático Validate para asegurar fechas, capacidad y texto.
        // Manejo de errores: Lanza DomainValidationException en caso de violaciones; la capa superior captura y transforma.
        Validate(title, description, startDateTime, duration, capacity);

        return new Event(
            Guid.NewGuid(),
            title.Trim(),
            description.Trim(),
            startDateTime,
            duration,
            location,
            capacity,
            category);
    }

    public void UpdateDetails(
        string title,
        string description,
        DateTimeOffset startDateTime,
        TimeSpan duration,
        Location location,
        int capacity,
        EventCategory category)
    {
        // Contexto: Permite modificar eventos existentes cuando cambian los requisitos del organizador.
        // Intención: Actualizar los datos manteniendo las invariantes de capacidad, fechas y sanitización.
        // Pasos: 1) Revalidar entradas; 2) Comprobar capacidad respecto a reservas; 3) Normalizar textos; 4) Asignar nuevos valores.
        // Validaciones: Vuelve a usar Validate y verifica que la nueva capacidad cubra las reservas activas.
        // Manejo de errores: Lanza DomainValidationException ante datos inválidos o capacidad insuficiente.
        Validate(title, description, startDateTime, duration, capacity);

        if (capacity < _reservations.Count)
        {
            throw new DomainValidationException("Capacity cannot be below existing reservations.");
        }

        Title = title.Trim();
        Description = description.Trim();
        StartDateTime = startDateTime;
        Duration = duration;
        Location = location;
        Capacity = capacity;
        Category = category;
    }

    public void Cancel()
    {
        // Contexto: Usado cuando se suspende o cancela un evento desde la aplicación.
        // Intención: Marcar el estado del evento sin eliminarlo para auditar y liberar reservas futuras.
        // Pasos: 1) Establecer bandera IsCancelled en true.
        // Validaciones: No necesita adicionales; decisión tomada en capas superiores.
        // Manejo de errores: No arroja excepciones; la transición es segura.
        IsCancelled = true;
    }

    public int RemainingCapacity => Capacity - _reservations.Count(r => !r.IsCancelled);

    public Reservation Reserve(User user, int quantity)
    {
        // Contexto: Flujo de reserva cuando un usuario solicita plazas.
        // Intención: Validar disponibilidad y generar una reserva asociada al evento.
        // Pasos: 1) Comprobar si el evento está cancelado; 2) Validar usuario; 3) Validar cantidad; 4) Evaluar capacidad restante; 5) Crear reserva; 6) Guardarla internamente.
        // Validaciones: Protege contra reservas sin usuario, cantidades no positivas y sobreventa.
        // Manejo de errores: Lanza DomainValidationException con mensajes específicos si alguna regla falla.
        if (IsCancelled)
        {
            throw new DomainValidationException("Cannot reserve seats for a cancelled event.");
        }

        if (user is null)
        {
            throw new DomainValidationException("User is required for reservation.");
        }

        if (quantity <= 0)
        {
            throw new DomainValidationException("Reservation quantity must be positive.");
        }

        if (RemainingCapacity < quantity)
        {
            throw new DomainValidationException("Not enough availability.");
        }

        var reservation = Reservation.Create(Id, user.Id, quantity);
        _reservations.Add(reservation);
        return reservation;
    }

    public void CancelReservation(Guid reservationId)
    {
        // Contexto: Se ejecuta cuando un usuario anula su reserva o un administrador revoca plazas.
        // Intención: Liberar cupos y mantener trazabilidad de reservas canceladas.
        // Pasos: 1) Localizar la reserva en la colección interna; 2) Validar su existencia; 3) Invocar cancelación de la reserva.
        // Validaciones: Garantiza que el identificador exista antes de operar.
        // Manejo de errores: DomainValidationException si no se encuentra la reserva solicitada.
        var reservation = _reservations.SingleOrDefault(r => r.Id == reservationId);
        if (reservation is null)
        {
            throw new DomainValidationException("Reservation not found.");
        }

        reservation.Cancel();
    }

    private static void Validate(
        string title,
        string description,
        DateTimeOffset startDateTime,
        TimeSpan duration,
        int capacity)
    {
        // Contexto: Rutina interna reutilizada por creación y actualización.
        // Intención: Centralizar reglas de validación para mantenimiento y coherencia.
        // Pasos: 1) Verificar títulos y descripciones; 2) Comprobar fecha futura; 3) Validar duración mínima; 4) Asegurar capacidad dentro de límites.
        // Validaciones: Evalúa campos obligatorios, longitudes y umbrales permitidos.
        // Manejo de errores: Lanza DomainValidationException con mensajes descriptivos inmediatamente al fallar.
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Title is required.");
        }

        if (title.Trim().Length > 200)
        {
            throw new DomainValidationException("Title is too long.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainValidationException("Description is required.");
        }

        if (startDateTime < DateTimeOffset.UtcNow.AddMinutes(-5))
        {
            throw new DomainValidationException("Event start must be in the future.");
        }

        if (duration.TotalMinutes < 15)
        {
            throw new DomainValidationException("Event duration must be at least 15 minutes.");
        }

        if (capacity <= 0)
        {
            throw new DomainValidationException("Capacity must be greater than zero.");
        }

        if (capacity > 10000)
        {
            throw new DomainValidationException("Capacity exceeds the supported limit.");
        }
    }
}
