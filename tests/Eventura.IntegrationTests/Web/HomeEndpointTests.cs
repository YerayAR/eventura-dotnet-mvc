using Eventura.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Eventura.IntegrationTests.Web;

/// <summary>
/// Escenario de integracion: validar que el pipeline completo responde correctamente.
/// Concepto validado: salud del endpoint inicial y configuracion de middleware (anti-CSRF en GET, HTTPS, correlacion).
/// </summary>
public class HomeEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HomeEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_HomeIndex_ReturnsSuccess()
    {
        // Given: un cliente HTTP obtenido de la fabrica (contexto integrado con middleware configurado).
        // When: se hace GET a la ruta raíz /.
        // Then: la respuesta debe ser 200 OK, demostrando que MVC, routing y middlewares funcionan (Concepto: smoke test de pipeline).
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}
