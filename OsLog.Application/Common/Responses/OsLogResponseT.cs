using System.Text.Json.Serialization;

namespace OsLog.Application.Common.Responses;

public class OsLogResponse<T> : OsLogResponse
{
    [JsonPropertyName("dados")]
    public new T? Dados
    {
        get => (T?)base.Dados!;
        set => base.Dados = value!;
    }

    public static OsLogResponse<T> Ok(T dados, string mensagem = "OK")
        => new()
        {
            Sucesso = true,
            Codigo = "0",
            Mensagem = mensagem,
            Dados = dados
        };
}
