namespace Eventura.Application.Abstractions;

/// <summary>
/// Capa: Application.
/// Propósito: Puerto para obtener el identificador de correlación actual.
/// Responsabilidades: Permitir trazabilidad en logs y servicios.
/// Dependencias/Puertos utilizados: Implementado en infraestructura (logging/middleware).
/// Límites (lo que NO debe hacer): Generar nuevos IDs arbitrariamente en cada llamada.
/// Errores comunes: Usar GUIDs ad-hoc en lugar del proveedor centralizado.
/// </summary>
public interface ICorrelationIdProvider
{
    string GetCorrelationId();
}
