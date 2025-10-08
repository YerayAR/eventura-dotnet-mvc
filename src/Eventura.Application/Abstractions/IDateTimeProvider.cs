namespace Eventura.Application.Abstractions;

/// <summary>
/// Capa: Application.
/// Propósito: Puerto para abstraer la obtención de tiempo UTC.
/// Responsabilidades: Permitir pruebas y control de tiempo en servicios.
/// Dependencias/Puertos utilizados: Implementado en infraestructura.
/// Límites (lo que NO debe hacer): Exponer fechas locales u otras fuentes sin control.
/// Errores comunes: Usar DateTime.UtcNow directamente en la capa de aplicación.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
