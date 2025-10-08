using Eventura.Application.DTOs;
using Eventura.Application.Services;
using Eventura.Infrastructure;
using Eventura.Infrastructure.Logging;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

#region Aprendizaje
// Inyeccion de dependencias: se registra Infrastructure para resolver puertos de Application.
// Anti-CSRF: AutoValidateAntiforgeryToken aplica tokens a todos los POST MVC por defecto.
// Rate limiting: configuracion global limita a 100 solicitudes por minuto para mitigar abuso.
// Correlation Id: middleware personalizado agrega cabecera para seguimiento extremo a extremo.
// Middleware de errores: UseExceptionHandler (prod) y DeveloperExceptionPage (dev) centralizan respuesta.
// TODO(aprendizaje): experimentar con un CustomExceptionMiddleware para diferenciar errores funcionales.
#endregion

builder.Services.AddInfrastructure();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
});

builder.Services.AddRateLimiter(options =>
{
    // Contexto: Configuracion de rate limiting para proteger endpoints.
    // Intencion: Limitar solicitudes por conexion u host.
    // Pasos: 1) Fijar codigo 429; 2) Definir particiones por ConnectionId/Host; 3) Asignar ventana de 1 minuto.
    // Validaciones: Ajustar PermitLimit segun requisitos; pruebas de carga recomendadas.
    // Manejo de errores: Solicitudes excedidas reciben 429 sin cuerpo sensible.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Request.Headers.Host.ToString() ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Eventura.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy("RequireOrganizer", policy => policy.RequireRole(Roles.Organizer, Roles.Admin));
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
});

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(120);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();
app.UseCookiePolicy();

app.Use(async (context, next) =>
{
    // Contexto: Middleware para propagar el identificador de correlacion.
    // Intencion: Alinear logs y respuestas con un ID unico para diagnositco.
    // Pasos: 1) Revisar cabecera entrante; 2) Generar GUID si falta; 3) Registrar en proveedor; 4) Escribir cabecera en respuesta; 5) Continuar pipeline.
    // Validaciones: Permite que clientes provean su ID pero controla generacion segura.
    // Manejo de errores: Excepciones subsecuentes incluyen el ID en logs, facilitando trazabilidad.
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    CorrelationIdProvider.SetCorrelationId(correlationId);
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    await next().ConfigureAwait(false);
});
#region Aprendizaje
// TODO(aprendizaje): envolver este middleware en una clase propia y agregar pruebas que verifiquen la cabecera.
#endregion

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    // Contexto: Inicializacion de datos al arrancar la aplicacion.
    // Intencion: Sembrar un usuario administrador para desarrollo seguro.
    // Pasos: 1) Resolver IAuthService; 2) Invocar EnsureSeedUserAsync; 3) Manejar resultados.
    // Validaciones: Los secretos reales no se guardan aqui; solo credenciales de desarrollo.
    // Manejo de errores: Excepcion detiene arranque para evitar estados inconsistentes.
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await EnsureSeedUserAsync(authService);
}

app.Run();

static async Task EnsureSeedUserAsync(IAuthService authService)
{
    // Contexto: Semilla de usuario administrador para entornos locales.
    // Intencion: Garantizar acceso inicial sin almacenar secretos en repo.
    // Pasos: 1) Construir RegisterUserRequest; 2) Llamar RegisterAsync; 3) Validar respuesta; 4) Lanzar si falla salvo email duplicado.
    // Validaciones: Evita crear duplicados comprobando mensaje de error conocido.
    // Manejo de errores: Lanza InvalidOperationException si la semilla no se puede crear.
    var adminResult = await authService.RegisterAsync(new Eventura.Application.DTOs.RegisterUserRequest
    {
        UserName = "admin",
        Email = "admin@eventura.local",
        Password = "AdminPass123!",
        Role = Roles.Admin
    });

    if (!adminResult.Succeeded && !string.Equals(adminResult.Error, "Email is already registered.", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Failed to seed admin user: {adminResult.Error}");
    }
}

public partial class Program
{
}
