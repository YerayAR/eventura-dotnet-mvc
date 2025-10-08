using Eventura.Application.DTOs;

namespace Eventura.Application.Validators;

/// <summary>
/// Capa: Application.
/// Propósito: Validar datos de entrada para reservas antes de llegar al dominio.
/// Responsabilidades: Garantizar que identificadores y cantidades sean válidos.
/// Dependencias/Puertos utilizados: Opera sobre CreateReservationRequest y OperationResult.
/// Límites (lo que NO debe hacer): Consultar repositorios o modificar estado.
/// Errores comunes: Omitir controles de cantidad y permitir Guid vacíos.
/// </summary>
public static class CreateReservationRequestValidator
{
    public static OperationResult Validate(CreateReservationRequest request)
    {
        // Contexto: Llamado por ReservationService antes de invocar Event.Reserve.
        // Intención: Atajar errores de entrada antes de generar excepciones de dominio.
        // Pasos: 1) Validar request; 2) Comprobar EventId; 3) Comprobar UserId; 4) Validar cantidad.
        // Validaciones: Usa OperationResult para devolver mensajes claros a la capa Web.
        // Manejo de errores: Devuelve Failure con detalle; evita lanzar excepciones en capas superiores.
        if (request is null)
        {
            return OperationResult.Failure("Request cannot be null.");
        }

        if (request.EventId == Guid.Empty)
        {
            return OperationResult.Failure("Event identifier is required.");
        }

        if (request.UserId == Guid.Empty)
        {
            return OperationResult.Failure("User identifier is required.");
        }

        if (request.Quantity <= 0)
        {
            return OperationResult.Failure("Quantity must be positive.");
        }

#region Aprendizaje
// Este validator refuerza la regla de controladores delgados: la Web delega validación al caso de uso,
// que a su vez consulta este helper. Mantiene el patrón DTO -> Validator -> Dominio.
// TODO(aprendizaje): añadir verificación de límites máximos por usuario para explorar reglas antisaturación.
#endregion

        return OperationResult.Success();
    }
}
