namespace OsLog.Application.Common.Responses;

public static class CriticasOsLog
{
    public static string RetornaCritica(int codigo)
        => codigo switch
        {
            CodigosOsLog.ERRO_VALIDACAO => "Erro de validação nos dados enviados.",
            CodigosOsLog.EMPRESA_NAO_ENCONTRADA => "Empresa não encontrada.",
            CodigosOsLog.UNIDADE_NAO_ENCONTRADA => "Unidade não encontrada.",
            CodigosOsLog.CLIENTE_NAO_ENCONTRADO => "Cliente não encontrado.",
            CodigosOsLog.SINAL_OBRIGATORIO_NAO_PAGO => "O sinal obrigatório ainda não foi confirmado.",
            CodigosOsLog.OS_NAO_ENCONTRADA => "Ordem de serviço não encontrada.",
            CodigosOsLog.ACESSORIO_NAO_ENCONTRADO => "Acessorio não encontrado.",
            CodigosOsLog.RELACAO_INCONSISTENTE => "Item não pertence à Ordem de serviço informada.",
            CodigosOsLog.ERRO_NEGOCIO => "Erro negocio.",
            CodigosOsLog.ERRO_INTERNO => "Erro interno.",
            CodigosOsLog.USUARIO_NAO_AUTENTICADO => "Não foi possível identificar o usuário autenticado.",

            _ => "Ocorreu um erro interno. Contate o suporte."
        };
}
