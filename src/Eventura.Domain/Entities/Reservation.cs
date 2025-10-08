using Eventura.Domain.Exceptions;

namespace Eventura.Domain.Entities;

/// <summary>
/// Capa: Domain.
/// Propósito: Representar la reserva de plazas para un evento.
/// Responsabilidades: Mantener relaciones con evento y usuario, controlar cantidad y estado.
/// Dependencias/Puertos utilizados: Ninguno directo; interactúa con Event y User a nivel de dominio.
/// Límites (lo que NO debe hacer): No debe realizar persistencia ni validar políticas de seguridad web.
/// Errores comunes: Saltarse la fábrica Create y modificar propiedades sin respetar invariantes.
/// </summary>
public sealed class Reservation
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public int Quantity { get; private set; }
    public DateTimeOffset ReservedAt { get; private set; }
    public bool IsCancelled { get; private set; }

    private Reservation()
    {
    }

    private Reservation(Guid id, Guid eventId, Guid userId, int quantity)
    {
        Id = id;
        EventId = eventId;
        UserId = userId;
        Quantity = quantity;
        ReservedAt = DateTimeOffset.UtcNow;
    }

#region Aprendizaje
// Las reservas son entidades hijas del agregado Event; se crean mediante métodos del agregado para
// preservar consistencia. Mantienen invariantes simples (identificadores y cantidad). Evitan
// dependencias a frameworks y se mantienen persistente-agnósticas.
// TODO(aprendizaje): agregar control de concurrencia optimista y pruebas de idempotencia en cancelaciones.
#endregion

    public static Reservation Create(Guid eventId, Guid userId, int quantity)
    {
        // Contexto: Ejecutado por Event.Reserve para materializar una nueva reserva.
        // Intención: Asegurar datos válidos y establecer una marca temporal de creación.
        // Pasos: 1) Validar identificadores; 2) Verificar cantidad positiva; 3) Instanciar objeto con nueva Guid.
        // Validaciones: Garantiza que eventId y userId no sean vacíos y que quantity sea > 0.
        // Manejo de errores: Lanza DomainValidationException ante entradas inválidas.
        if (eventId == Guid.Empty)
        {
            throw new DomainValidationException("Event identifier is invalid.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("User identifier is invalid.");
        }

        if (quantity <= 0)
        {
            throw new DomainValidationException("Quantity must be positive.");
        }

        return new Reservation(Guid.NewGuid(), eventId, userId, quantity);
    }

    public void Cancel()
    {
        // Contexto: Reservas anuladas por usuario o sistema para liberar cupo.
        // Intención: Marcar la reserva como cancelada manteniendo el histórico.
        // Pasos: 1) Ajustar bandera IsCancelled a true.
        // Validaciones: No requiere adicionales; la lógica de negocio decide cuándo invocar.
        // Manejo de errores: No lanza; el estado cambia de forma determinista.
        IsCancelled = true;
    }
}
