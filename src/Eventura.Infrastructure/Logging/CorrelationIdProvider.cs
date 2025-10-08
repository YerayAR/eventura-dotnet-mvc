using System.Diagnostics;
using System.Threading;
using Eventura.Application.Abstractions;

namespace Eventura.Infrastructure.Logging;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Mantener el identificador de correlación por contexto asincrónico.
/// Responsabilidades: Generar o recuperar correlation IDs usados en logging y tracing.
/// Dependencias/Puertos utilizados: AsyncLocal para almacenamiento y Activity.Current para compatibilidad con OpenTelemetry.
/// Límites (lo que NO debe hacer): Exponer IDs persistentes entre requests ni generar colisiones deliberadas.
/// Errores comunes: Olvidar limpiar valores al finalizar contexto o no propagar en middleware.
/// </summary>
public sealed class CorrelationIdProvider : ICorrelationIdProvider
{
    private static readonly AsyncLocal<string?> Storage = new();

    public string GetCorrelationId()
    {
        // Contexto: Invocado por servicios o middleware para identificar trazas.
        // Intención: Retornar un ID consistente dentro del flujo actual.
        // Pasos: 1) Revisar almacenamiento AsyncLocal; 2) Intentar Activity.Current; 3) Generar GUID si no existe; 4) Persistir en AsyncLocal.
        // Validaciones: Garantiza cadena no vacía.
        // Manejo de errores: No lanza; fallback siempre genera GUID.
        if (!string.IsNullOrEmpty(Storage.Value))
        {
            return Storage.Value!;
        }

        var correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
        Storage.Value = correlationId;
        return correlationId;
    }

    public static void SetCorrelationId(string correlationId)
    {
        // Contexto: Middleware establece el correlation ID al inicio del request.
        // Intención: Permitir que posteriores llamadas GetCorrelationId lo reutilicen.
        // Pasos: 1) Guardar valor en AsyncLocal.
        // Validaciones: Asume cadena válida; podría extenderse con verificación GUID.
        // Manejo de errores: No lanza; asignación simple.
        Storage.Value = correlationId;
    }
}
