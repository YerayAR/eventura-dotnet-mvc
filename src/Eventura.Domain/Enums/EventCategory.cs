namespace Eventura.Domain.Enums;

/// <summary>
/// Capa: Domain.
/// Propósito: Enumeración de categorías para clasificar eventos.
/// Responsabilidades: Servir como vocabulario controlado compartido entre capas.
/// Dependencias/Puertos utilizados: Ninguna.
/// Límites (lo que NO debe hacer): No representar lógica compleja; usar en combinación con validaciones.
/// Errores comunes: Persistir valores mágicos sin sincronizar con esta enumeración.
/// </summary>
public enum EventCategory
{
    Music = 1,
    Art = 2,
    Sports = 3,
    Education = 4,
    Community = 5,
    Technology = 6,
    Other = 99
}
