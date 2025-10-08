using Eventura.Application.DTOs;

namespace Eventura.Application.Validators;

/// <summary>
/// Capa: Application.
/// Propósito: Validar reglas de negocio previas a construir entidades de evento.
/// Responsabilidades: Prevenir entradas inválidas antes de llegar al dominio.
/// Dependencias/Puertos utilizados: Opera sobre DTOs y utiliza OperationResult.
/// Límites (lo que NO debe hacer): Aplicar efectos secundarios o acceder a repositorios.
/// Errores comunes: Duplicar reglas ya cubiertas en el dominio sin mantener consistencia.
/// </summary>
public static class CreateEventRequestValidator
{
    public static OperationResult Validate(CreateEventRequest request, DateTimeOffset utcNow)
    {
        // Contexto: Ejecutado por servicios de aplicación antes de crear/actualizar eventos.
        // Intención: Detener errores comunes de entrada sin lanzar excepciones.
        // Pasos: 1) Verificar request no nulo; 2) Validar campos obligatorios; 3) Chequear longitudes; 4) Revisar fechas, duración y capacidad; 5) Validar localización.
        // Validaciones: Usa OperationResult para describir cada fallo específico.
        // Manejo de errores: Devuelve OperationResult.Failure con mensaje para mostrar en UI.
        if (request is null)
        {
            return OperationResult.Failure("Request cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return OperationResult.Failure("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return OperationResult.Failure("Description is required.");
        }

        if (request.Title.Trim().Length > 200)
        {
            return OperationResult.Failure("Title is too long.");
        }

        if (request.StartDateTime < utcNow.AddMinutes(-5))
        {
            return OperationResult.Failure("Start date must be in the future.");
        }

        if (request.Duration.TotalMinutes < 15)
        {
            return OperationResult.Failure("Duration must be at least 15 minutes.");
        }

        if (request.Capacity <= 0)
        {
            return OperationResult.Failure("Capacity must be positive.");
        }

        if (string.IsNullOrWhiteSpace(request.City) || string.IsNullOrWhiteSpace(request.AddressLine))
        {
            return OperationResult.Failure("Location is required.");
        }

#region Aprendizaje
// Validar aquí evita viajes innecesarios al dominio. Se mantiene la regla de que el caso de uso
// es el puente: la Web entrega DTO -> validator -> dominio. Esto también apoya controladores delgados.
// TODO(aprendizaje): añadir validación para evitar solapamiento de fechas con prueba unitaria adicional.
#endregion

        return OperationResult.Success();
    }
}
