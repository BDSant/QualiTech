using System.Text.Json.Serialization;

namespace OsLog.Application.Common.Responses;

public class OsLogResponse
{
    public bool Sucesso { get; set; }
    public string Codigo { get; set; } = "0";
    public string Mensagem { get; set; } = "OK";
    public object? Dados { get; set; }
    public object? Erros { get; set; }

    public static OsLogResponse Ok(object? dados = null, string mensagem = "OK")
        => new()
        {
            Sucesso = true,
            Codigo = "0",
            Mensagem = mensagem,
            Dados = dados
        };

    public static OsLogResponse Critica(int codigo, string mensagem, object? erros = null)
        => new()
        {
            Sucesso = false,
            Codigo = codigo.ToString(),
            Mensagem = mensagem,
            Erros = erros
        };
}


