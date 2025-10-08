namespace Eventura.Application.Abstractions;

/// <summary>
/// Capa: Application.
/// Propósito: Puerto para hashear y verificar contraseñas.
/// Responsabilidades: Asegurar independencia de la implementación concreta de hashing.
/// Dependencias/Puertos utilizados: Implementado por infraestructura (p.ej. PBKDF2).
/// Límites (lo que NO debe hacer): Persistir sal en memoria compartida o exponer secretos.
/// Errores comunes: Reutilizar implementaciones inseguras sin pasar por este contrato.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}
