using Scalar.AspNetCore;

namespace OsLog.API.Configurations;

public static class ScalarConfiguration
{
    /// <summary>
    /// Habilita a documentação interativa do Scalar utilizando uma rota própria
    /// para os documentos OpenAPI JSON da aplicação.
    /// </summary>
    /// <param name="app">Aplicação Web utilizada para mapear os endpoints do Scalar.</param>
    /// <returns>A própria aplicação Web, permitindo o encadeamento das configurações.</returns>
    public static WebApplication UseScalarInterface(this WebApplication app)
    {
        app.MapScalarApiReference("/docs/scalar", options =>
        {
            options.Title = "OsLog.API";
            options.OpenApiRoutePattern = "/docs/scalar/{documentName}.json";
        });

        return app;
    }
}