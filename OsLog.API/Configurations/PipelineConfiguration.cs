//Esse arquivo limpa o pipeline HTTP e concentra middleware.

namespace OsLog.API.Configurations;

public static class PipelineConfiguration
{
    /// <summary>
    /// Configura o pipeline HTTP da aplicação, incluindo Swagger em ambiente de desenvolvimento,
    /// redirecionamento HTTPS, autenticação, autorização e mapeamento dos controllers.
    /// </summary>
    /// <param name="app">Aplicação Web utilizada para configurar o pipeline de execução.</param>
    /// <returns>A própria aplicação Web, permitindo o encadeamento das configurações.</returns>
    public static WebApplication UseAppConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwaggerDocumentation();
            app.UseOpenApiDocumentation();
            app.UseScalarUi();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}