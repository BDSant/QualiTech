namespace OsLog.Application.Common.Errors;

public static class CriticasOsLog
{
    public static string RetornaCritica(int codigo)
        => codigo switch
        {
            CodigosOsLog.ERRO_VALIDACAO => "Erro de validação nos dados enviados.",
            CodigosOsLog.EMPRESA_NAO_ENCONTRADA => "Empresa não encontrada.",
            CodigosOsLog.UNIDADE_NAO_ENCONTRADA => "Unidade não encontrada.",
            CodigosOsLog.OS_NAO_ENCONTRADA => "Ordem de serviço não encontrada.",
            CodigosOsLog.CLIENTE_NAO_ENCONTRADO => "Cliente não encontrado.",
            CodigosOsLog.SINAL_OBRIGATORIO_NAO_PAGO => "O sinal obrigatório ainda não foi confirmado.",
            _ => "Ocorreu um erro interno. Contate o suporte."
        };
}
