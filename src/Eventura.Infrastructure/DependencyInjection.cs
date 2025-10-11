using Eventura.Application.Abstractions;
using Eventura.Application.Services;
using Eventura.Domain.Repositories;
using Eventura.Infrastructure.Logging;
using Eventura.Infrastructure.Persistence;
using Eventura.Infrastructure.Security;
using Eventura.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Eventura.Infrastructure;

/// <summary>
/// Capa: Infrastructure.
/// Propósito: Configurar inyección de dependencias concreta para los puertos definidos en Application/Domain.
/// Responsabilidades: Registrar repositorios, servicios compartidos y casos de uso.
/// Dependencias/Puertos utilizados: Implementaciones in-memory, servicios de seguridad y logging.
/// Límites (lo que NO debe hacer): Contener lógica de negocio o acceder a la capa Web.
/// Errores comunes: Registrar implementaciones concretas con estilos de vida incorrectos o duplicar servicios.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Contexto: Configuración central consumida por Program.cs.
        // Intención: Unir interfaces (puertos) con implementaciones específicas de infraestructura.
        // Pasos: 1) Registrar almacén in-memory y repositorios; 2) Registrar servicios compartidos; 3) Registrar casos de uso.
        // Validaciones: Revisar estilos de vida (Singleton/Scoped) y dependencias transitivas.
        // Manejo de errores: Errores de registro se manifiestan en tiempo de ejecución; agregar pruebas de arranque cuando sea posible.
#region Aprendizaje
// Diferencia puertos/adaptadores: Repositorios implementan interfaces del dominio y se conectan aquí.
// Servicios como SystemDateTimeProvider permiten aislar infraestructura y facilitar tests.
// TODO(aprendizaje): sustituir repos in-memory por implementación persistente y evaluar transacciones reales.
#endregion
        services.AddSingleton<InMemoryDataStore>();
        services.AddSingleton<IEventRepository, InMemoryEventRepository>();
        services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();
        services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();

        services.AddHttpClient<IExternalEventsService, ExternalEventsService>();
        services.AddScoped<ExternalEventImportService>();
        
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();

        return services;
    }
}
