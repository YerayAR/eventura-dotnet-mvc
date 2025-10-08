using Eventura.Domain.Repositories;

namespace Eventura.Infrastructure.Persistence;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Implementar el contrato de unidad de trabajo para escenario in-memory.
/// Responsabilidades: Mantener compatibilidad con la interfaz IUnitOfWork.
/// Dependencias/Puertos utilizados: Ninguna adicional.
/// Límites (lo que NO debe hacer): Realizar operaciones de commit reales; en memoria no existe transacción.
/// Errores comunes: Olvidar reemplazar por implementación real cuando se introduce persistencia.
/// </summary>
public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Contexto: Confirmación de cambios en adaptadores in-memory.
        // Intención: Mantener contrato aun cuando no haya commits reales.
        // Pasos: 1) Retornar Task resuelto con valor cero.
        // Validaciones: N/A; solo se proporciona para compatibilidad.
        // Manejo de errores: Ninguno.
#region Aprendizaje
// En entornos con base de datos, aquí se manejarían transacciones y commits.
// TODO(aprendizaje): implementar unit of work real usando EF Core e incluir prueba de rollback.
#endregion
        // No-op for in-memory implementation but keeps contract consistent.
        return Task.FromResult(0);
    }
}
