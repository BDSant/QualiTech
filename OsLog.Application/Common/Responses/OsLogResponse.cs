using Microsoft.AspNetCore.Http;

namespace OsLog.Application.Common.Responses;

public class OsLogResponse : OsLogResponse<object?>
{
    public OsLogResponse() { }

    private OsLogResponse(OsLogResponse<object?> origem)
    {
        Sucesso = origem.Sucesso;
        Codigo = origem.Codigo;
        Mensagem = origem.Mensagem;
        StatusHttp = origem.StatusHttp;
        Dados = origem.Dados;
        Erros = origem.Erros;
    }

    public new static OsLogResponse Ok(
        object? dados = null,
        string? mensagem = "OK",
        int? statusHttp = StatusCodes.Status200OK)
        => new(OsLogResponse<object?>.Ok(dados, mensagem, statusHttp));

    public new static OsLogResponse OkVazio(
        string? mensagem = "OK",
        int? statusHttp = StatusCodes.Status200OK)
        => new(OsLogResponse<object?>.OkVazio(mensagem, statusHttp));

    public new static OsLogResponse Critica(
        int codigo,
        string mensagem,
        object? erros = null,
        int? statusHttp = StatusCodes.Status400BadRequest)
        => new(OsLogResponse<object?>.Critica(codigo, mensagem, erros, statusHttp));

    public new static OsLogResponse ErroValidacao(
        int codigo,
        string mensagem,
        object erros,
        int? statusHttp = StatusCodes.Status400BadRequest)
        => new(OsLogResponse<object?>.ErroValidacao(codigo, mensagem, erros, statusHttp));

    public new static OsLogResponse ErroInterno(
        int codigo,
        string mensagem,
        object? detalhes = null,
        int? statusHttp = StatusCodes.Status500InternalServerError)
        => new(OsLogResponse<object?>.ErroInterno(codigo, mensagem, detalhes, statusHttp));
}
