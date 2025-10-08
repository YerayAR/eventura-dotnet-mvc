using Eventura.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Eventura.Infrastructure.Services;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Implementar envío de correo simulado mediante logging para entornos de desarrollo.
/// Responsabilidades: Registrar los intentos de envío sin exponer credenciales reales.
/// Dependencias/Puertos utilizados: ILogger para trazar envíos.
/// Límites (lo que NO debe hacer): Enviar correos reales ni almacenar mensajes sensibles en logs permanentes.
/// Errores comunes: Utilizar esta implementación en producción sin reemplazo adecuado.
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Contexto: Llamado por AuthService al registrar usuarios u otras notificaciones.
        // Intención: Registrar en logs que se enviaría un correo sin exponer contenido sensible.
        // Pasos: 1) Registrar destinatario y asunto; 2) Finalizar la tarea.
        // Validaciones: Se recomienda anonimizar datos sensibles antes de loguearlos.
        // Manejo de errores: Al ser in-memory no lanza; en implementaciones reales deben manejar fallos de red.
        _logger.LogInformation("Sending email to {Recipient} with subject {Subject}.", to, subject);
        return Task.CompletedTask;
    }
}
