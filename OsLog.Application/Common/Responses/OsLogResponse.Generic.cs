using Microsoft.AspNetCore.Http;

namespace OsLog.Application.Common.Responses;

public class OsLogResponse<T>
{
    public bool Sucesso { get; init; }
    public int? Codigo { get; init; }
    public string? Mensagem { get; init; }
    public int? StatusHttp { get; init; }
    public T? Dados { get; init; }
    public object? Erros { get; init; }

    public static OsLogResponse<T> Ok(
        T dados,
        string? mensagem = "OK",
        int? statusHttp = StatusCodes.Status200OK)
        => new()
        {
            Sucesso = true,
            Codigo = 0,
            Mensagem = mensagem,
            StatusHttp = statusHttp,
            Dados = dados,
            Erros = null
        };

    public static OsLogResponse<T> OkVazio(
        string? mensagem = "OK",
        int? statusHttp = StatusCodes.Status200OK)
        => new()
        {
            Sucesso = true,
            Codigo = 0,
            Mensagem = mensagem,
            StatusHttp = statusHttp,
            Dados = default,
            Erros = null
        };

    public static OsLogResponse<T> Critica(
        int codigo,
        string mensagem,
        object? erros = null,
        int? statusHttp = StatusCodes.Status400BadRequest)
        => new()
        {
            Sucesso = false,
            Codigo = codigo,
            Mensagem = mensagem,
            StatusHttp = statusHttp,
            Dados = default,
            Erros = erros
        };

    public static OsLogResponse<T> ErroValidacao(
        int codigo,
        string mensagem,
        object erros,
        int? statusHttp = StatusCodes.Status400BadRequest)
        => new()
        {
            Sucesso = false,
            Codigo = codigo,
            Mensagem = mensagem,
            StatusHttp = statusHttp,
            Dados = default,
            Erros = erros
        };

    public static OsLogResponse<T> ErroInterno(
        int codigo,
        string mensagem,
        object? detalhes = null,
        int? statusHttp = StatusCodes.Status500InternalServerError)
        => new()
        {
            Sucesso = false,
            Codigo = codigo,
            Mensagem = mensagem,
            StatusHttp = statusHttp,
            Dados = default,
            Erros = detalhes
        };
}
