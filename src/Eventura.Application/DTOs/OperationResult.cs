namespace Eventura.Application.DTOs;

/// <summary>
/// Capa: Application.
/// Propósito: Representar el resultado estándar de operaciones de aplicación.
/// Responsabilidades: Transportar estado de éxito y mensaje de error opcional.
/// Dependencias/Puertos utilizados: Ninguna; diseñado para ser serializable y simple.
/// Límites (lo que NO debe hacer): Incluir lógica de negocio ni excepciones.
/// Errores comunes: Usarlo en dominio en lugar de excepciones específicas.
/// </summary>
public sealed class OperationResult
{
    public bool Succeeded { get; }
    public string? Error { get; }

    private OperationResult(bool succeeded, string? error = null)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public static OperationResult Success()
    {
        // Contexto: Utilizado por casos de uso para indicar operaciones exitosas sin payload.
        // Intención: Crear resultado de éxito consistente.
        // Pasos: 1) Instanciar OperationResult con succeeded=true.
        // Validaciones: No aplica; se asume éxito.
        // Manejo de errores: No lanza.
        return new OperationResult(true);
    }

    public static OperationResult Failure(string error)
    {
        // Contexto: Cuando una operación falla y se necesita mensaje descriptivo.
        // Intención: Proveer resultado con estado negativo y mensaje.
        // Pasos: 1) Instanciar OperationResult con succeeded=false y error.
        // Validaciones: Se espera mensaje no nulo; validarlo en capas superiores.
        // Manejo de errores: No lanza; encierra detalle.
        return new OperationResult(false, error);
    }
}

/// <summary>
/// Capa: Application.
/// Propósito: Resultado genérico que transporta datos en caso de éxito.
/// Responsabilidades: Combinar estado, mensaje y payload.
/// Dependencias/Puertos utilizados: Ninguna específica.
/// Límites (lo que NO debe hacer): Contener lógica de negocio ni manipular entidades directamente.
/// Errores comunes: Retornar Data nula sin revisar Succeeded.
/// </summary>
public sealed class OperationResult<T>
{
    public bool Succeeded { get; }
    public string? Error { get; }
    public T? Data { get; }

    private OperationResult(bool succeeded, T? data = default, string? error = null)
    {
        Succeeded = succeeded;
        Data = data;
        Error = error;
    }

    public static OperationResult<T> Success(T data)
    {
        // Contexto: Caso de uso completado con información a retornar.
        // Intención: Entregar payload junto con bandera de éxito.
        // Pasos: 1) Instanciar con succeeded=true y data proporcionada.
        // Validaciones: Data puede ser nula según T; validar en capas superiores si es obligatorio.
        // Manejo de errores: No lanza.
        return new OperationResult<T>(true, data);
    }

    public static OperationResult<T> Failure(string error)
    {
        // Contexto: Falla de operación que incluye mensaje de error.
        // Intención: Retornar estado negativo y mensaje para diagnóstico.
        // Pasos: 1) Instanciar con succeeded=false, data por defecto, error proporcionado.
        // Validaciones: Mensaje proporcionado por capa llamante.
        // Manejo de errores: No lanza.
        return new OperationResult<T>(false, default, error);
    }
}
