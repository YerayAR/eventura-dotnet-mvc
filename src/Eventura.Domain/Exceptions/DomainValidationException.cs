namespace Eventura.Domain.Exceptions;

/// <summary>
/// Capa: Domain.
/// Propósito: Excepción específica para violaciones de reglas de negocio.
/// Responsabilidades: Comunicar fallos de validación a capas superiores sin filtrar detalles de infraestructura.
/// Dependencias/Puertos utilizados: Basada en Exception estándar.
/// Límites (lo que NO debe hacer): No debe encapsular lógica de recuperación ni mensajes con datos sensibles.
/// Errores comunes: Abusar de esta excepción para errores de infraestructura o lógica de aplicación.
/// </summary>
public sealed class DomainValidationException : Exception
{
    public DomainValidationException()
    {
        // Contexto: Constructor por defecto requerido para compatibilidad.
        // Intención: Permitir instanciación sin mensaje específico.
        // Pasos: Invoca constructor base implícitamente.
        // Validaciones: No aplica.
        // Manejo de errores: No lanza; construcción directa.
    }

    public DomainValidationException(string message)
        : base(message)
    {
        // Contexto: Construcción común con mensaje descriptivo.
        // Intención: Personalizar la causa para diagnóstico.
        // Pasos: Delegar en constructor base con mensaje.
        // Validaciones: Mensaje no se valida aquí; debe venir controlado desde dominio.
        // Manejo de errores: No lanza; inicializa instancia.
    }

    public DomainValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        // Contexto: Encapsular excepciones internas manteniendo mensaje de dominio.
        // Intención: Anidar excepción original para trazabilidad.
        // Pasos: Pasar mensaje e inner al constructor base.
        // Validaciones: No aplica.
        // Manejo de errores: No lanza adicionalmente.
    }

}
