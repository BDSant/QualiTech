using OsLog.Application.Interfaces.Repositories;

namespace OsLog.Application.Common;

public interface IUnitOfWork : IAsyncDisposable
{
    IOrdemServicoRepository OrdensServico { get; }
    IOrcamentoItemRepository OrcamentoItens { get; }
    IPagamentoRepository Pagamentos { get; }
    IStatusHistoricoRepository StatusHistoricos { get; }
    IOrdemServicoAcessorioRepository Acessorios { get; }
    IOrdemServicoFotoRepository Fotos { get; }
    IOrdemServicoComissaoRepository Comissoes { get; }
    IClienteRepository Clientes { get; }
    ITecnicoRepository Tecnicos { get; }
    IEmpresaRepository Empresas { get; }
    IUnidadeRepository Unidades { get; }

    Task<int> CommitAsync(CancellationToken ct = default);
}

