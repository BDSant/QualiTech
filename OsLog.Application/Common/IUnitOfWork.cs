using OsLog.Application.Ports.Persistence.Repositories;

namespace OsLog.Application.Common;

public interface IUnitOfWork : IAsyncDisposable
{
    IPagamentoRepository Pagamentos { get; }
    IStatusHistoricoRepository StatusHistoricos { get; }
    IClienteRepository Clientes { get; }
    ITecnicoRepository Tecnicos { get; }
    IEmpresaRepository Empresas { get; }
    IUnidadeRepository Unidades { get; }
    IUsuarioAcessoRepository UsuarioAcessos { get; }

    //IOrdemServicoRepository OrdensServico { get; }
    //IOrcamentoItemRepository OrcamentoItens { get; }
    //IOrdemServicoAcessorioRepository Acessorios { get; }
    //IOrdemServicoFotoRepository Fotos { get; }
    //IOrdemServicoComissaoRepository Comissoes { get; }

    Task<int> CommitAsync(CancellationToken ct = default);
}

