using OsLog.Application.Common;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.UnitOfWork;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public EfUnitOfWork(
        AppDbContext context,
        IOrdemServicoRepository ordensServico,
        IOrcamentoItemRepository orcamentoItens,
        IPagamentoRepository pagamentos,
        IStatusHistoricoRepository statusHistoricos,
        IOrdemServicoAcessorioRepository acessorios,
        IOrdemServicoFotoRepository fotos,
        IOrdemServicoComissaoRepository comissoes,
        IClienteRepository clientes,
        ITecnicoRepository tecnicos,
        IEmpresaRepository empresas,
        IUnidadeRepository unidades)
    {
        _context = context;
        OrdensServico = ordensServico;
        OrcamentoItens = orcamentoItens;
        Pagamentos = pagamentos;
        StatusHistoricos = statusHistoricos;
        Acessorios = acessorios;
        Fotos = fotos;
        Comissoes = comissoes;
        Clientes = clientes;
        Tecnicos = tecnicos;
        Empresas = empresas;
        Unidades = unidades;
    }

    public IOrdemServicoRepository OrdensServico { get; }
    public IOrcamentoItemRepository OrcamentoItens { get; }
    public IPagamentoRepository Pagamentos { get; }
    public IStatusHistoricoRepository StatusHistoricos { get; }
    public IOrdemServicoAcessorioRepository Acessorios { get; }
    public IOrdemServicoFotoRepository Fotos { get; }
    public IOrdemServicoComissaoRepository Comissoes { get; }
    public IClienteRepository Clientes { get; }
    public ITecnicoRepository Tecnicos { get; }
    public IEmpresaRepository Empresas { get; }
    public IUnidadeRepository Unidades { get; }

    public async Task<int> CommitAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public ValueTask DisposeAsync()
        => _context.DisposeAsync();
}
