namespace Eventura.Application.Abstractions;

/// <summary>
/// Capa: Application.
/// Propósito: Definir puerto para envío de correos desde casos de uso.
/// Responsabilidades: Abstraer canal de notificación y permitir implementaciones fakes o reales.
/// Dependencias/Puertos utilizados: Implementado en infraestructura.
/// Límites (lo que NO debe hacer): Incluir detalles de transporte (SMTP, APIs) dentro de la capa de aplicación.
/// Errores comunes: Acoplar casos de uso a implementaciones concretas sin pasar por este puerto.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
