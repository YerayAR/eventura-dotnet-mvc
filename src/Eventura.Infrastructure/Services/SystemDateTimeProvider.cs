using Eventura.Application.Abstractions;

namespace Eventura.Infrastructure.Services;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Implementar IDateTimeProvider usando la hora del sistema.
/// Responsabilidades: Proveer DateTimeOffset.UtcNow a la capa de aplicación.
/// Dependencias/Puertos utilizados: Ninguno adicional, usa API base de .NET.
/// Límites (lo que NO debe hacer): Introducir lógica de negocio ni valores de hora configurables sin tests.
/// Errores comunes: Usar DateTime.Now causando errores de zona horaria.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow
    {
        get
        {
            // Contexto: Consultado por servicios para comparar fechas.
            // Intención: Centralizar la obtención de hora UTC.
            // Pasos: 1) Retornar DateTimeOffset.UtcNow.
            // Validaciones: N/A.
            // Manejo de errores: N/A; llamado es determinista.
            return DateTimeOffset.UtcNow;
        }
    }
}
