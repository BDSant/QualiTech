using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OsLog.API.Configurations;

public static class SwaggerConfiguration
{
    /// <summary>
    /// Registra a configuração do Swagger/OpenAPI, incluindo versionamento da documentação
    /// e definição de segurança para autenticação Bearer.
    /// </summary>
    /// <param name="services">Coleção de serviços utilizada para registrar os recursos de documentação da API.</param>
    /// <returns>A própria coleção de serviços, permitindo o encadeamento das configurações.</returns>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe: Bearer {seu_token}"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }

    /// <summary>
    /// Habilita o Swagger e configura a interface Swagger UI com suporte às versões descobertas da API.
    /// </summary>
    /// <param name="app">Aplicação Web utilizada para configurar os recursos de documentação da API.</param>
    /// <returns>A própria aplicação Web, permitindo o encadeamento das configurações.</returns>
    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "docs/swagger/{documentName}.json";
        });

        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/docs/swagger/{description.GroupName}.json",
                    $"OsLog.API {description.GroupName.ToUpperInvariant()}");
            }

            options.RoutePrefix = "docs/swagger";
        });

        return app;
    }
}

internal sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "OsLog.API",
                Version = description.ApiVersion.ToString(),
                Description = $"Documentação da API OsLog - {description.GroupName}"
            });
        }
    }
}