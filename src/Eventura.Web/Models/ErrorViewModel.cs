namespace Eventura.Web.Models;

/// <summary>
/// Capa: Web.
/// Proposito: Proveer informacion minima para la vista de errores.
/// Responsabilidades: Exponer RequestId y determinar si debe mostrarse.
/// Dependencias/Puertos utilizados: Ninguna.
/// Limites (lo que NO debe hacer): Incluir stack traces o datos sensibles.
/// Errores comunes: Mostrar identificadores cuando contienen informacion privada.
/// </summary>
public sealed class ErrorViewModel
{
    public string RequestId { get; init; } = string.Empty;
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
