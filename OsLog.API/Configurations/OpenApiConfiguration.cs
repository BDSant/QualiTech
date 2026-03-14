using Microsoft.AspNetCore.OpenApi;

namespace OsLog.API.Configurations;

public static class OpenApiConfiguration
{
    /// <summary>
    /// Registra os documentos OpenAPI da aplicação para cada versão exposta pela API.
    /// </summary>
    /// <param name="services">Coleção de serviços utilizada para registrar os documentos OpenAPI.</param>
    /// <returns>A própria coleção de serviços, permitindo o encadeamento das configurações.</returns>
    public static IServiceCollection UseOpenApiDocuments(this IServiceCollection services)
    {
        services.AddOpenApi("v1");
        services.AddOpenApi("v2");

        return services;
    }

    /// <summary>
    /// Expõe os documentos OpenAPI da aplicação em endpoints HTTP versionados.
    /// </summary>
    /// <param name="app">Aplicação Web utilizada para mapear os endpoints OpenAPI.</param>
    /// <returns>A própria aplicação Web, permitindo o encadeamento das configurações.</returns>
    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi("/docs/scalar/{documentName}.json");

        return app;
    }
}