namespace Eventura.Domain.Repositories;

/// <summary>
/// Capa: Domain.
/// Propósito: Puerto genérico para coordinar transacciones entre repositorios.
/// Responsabilidades: Confirmar cambios pendientes como una unidad atómica.
/// Dependencias/Puertos utilizados: Implementado en infraestructura sobre almacenamiento específico.
/// Límites (lo que NO debe hacer): No exponer detalles de transacción propios de un motor en particular.
/// Errores comunes: Ignorar su uso en casos de uso que requieren consistencia entre agregados.
/// </summary>
public interface IUnitOfWork
{
#region Aprendizaje
// El caso de uso controla la transacción a través de este puerto, permitiendo que infraestructura defina los límites (EF DbContext, transacciones SQL).
// TODO(aprendizaje): crear implementación que soporte transacciones distribuidas y cubrirlas con pruebas de rollback.
#endregion
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
