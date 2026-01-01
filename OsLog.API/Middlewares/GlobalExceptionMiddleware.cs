using OsLog.Application.Common.Responses;
using System.Net;
using System.Text.Json;

namespace OsLog.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, _logger);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception ex,
        ILogger logger)
    {
        // Log completo para troubleshooting
        logger.LogError(ex, "Erro não tratado na pipeline HTTP.");

        // Default: erro interno 500
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var codigo = CodigosOsLog.ERRO_INTERNO;
        var mensagem = CriticasOsLog.RetornaCritica(CodigosOsLog.ERRO_INTERNO);
        object? detalhesExtras = null;

        // Mapeamento de tipos de exceção
        switch (ex)
        {
            case InvalidOperationException invEx:
                MapInvalidOperationException(invEx, ref statusCode, ref codigo, ref mensagem);
                break;

            // Aqui você pode tratar outros tipos específicos:
            // case DbUpdateException dbEx:
            //     ...
            //     break;

            default:
                // Mantém 500 + mensagem genérica
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = OsLogResponse.Critica(codigo, mensagem, detalhesExtras);

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Mapeia InvalidOperationException para códigos e HTTP adequados.
    /// </summary>
    private static void MapInvalidOperationException(
        InvalidOperationException ex,
        ref int statusCode,
        ref int codigo,
        ref string mensagem)
    {
        var msg = ex.Message ?? string.Empty;
        var msgLower = msg.ToLowerInvariant();

        // Exemplo 1: Ordem de serviço não encontrada
        if (msgLower.Contains("ordem de serviço") && msgLower.Contains("não encontrada"))
        {
            statusCode = (int)HttpStatusCode.NotFound;
            codigo = CodigosOsLog.OS_NAO_ENCONTRADA;
            mensagem = CriticasOsLog.RetornaCritica(CodigosOsLog.OS_NAO_ENCONTRADA);
            return;
        }

        // Exemplo 2: Acessório não encontrado
        if (msgLower.Contains("acessório") && msgLower.Contains("não encontrado"))
        {
            statusCode = (int)HttpStatusCode.NotFound;
            codigo = CodigosOsLog.ACESSORIO_NAO_ENCONTRADO;
            mensagem = CriticasOsLog.RetornaCritica(CodigosOsLog.ACESSORIO_NAO_ENCONTRADO);
            return;
        }

        // Exemplo 3: Item não pertence à OS informada → erro de negócio 400
        if (msgLower.Contains("não pertence à os informada"))
        {
            statusCode = (int)HttpStatusCode.BadRequest;
            codigo = CodigosOsLog.RELACAO_INCONSISTENTE;
            mensagem = CriticasOsLog.RetornaCritica(CodigosOsLog.RELACAO_INCONSISTENTE);
            return;
        }

        // Default para InvalidOperationException: erro de negócio genérico 400
        statusCode = (int)HttpStatusCode.BadRequest;
        codigo = CodigosOsLog.ERRO_NEGOCIO;
        mensagem = msg; // Aqui pode ser a própria mensagem da exception
    }
}
