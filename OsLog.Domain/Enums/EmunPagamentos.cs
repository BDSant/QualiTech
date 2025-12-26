namespace OsLog.Domain.Enums
{
    public enum TipoPagamento : byte
    {
        NaoDefinido = 1,
        Sinal,
        Restante,
        Total,
    }

    public enum FormaPagamento : byte
    {
        NaoDefinido = 1,
        Cartao,
        Pix,
        Dinheiro
    }

    public enum StatusRegistro : byte
    {
        Pendente = 1,
        Confirmado
    }
}
