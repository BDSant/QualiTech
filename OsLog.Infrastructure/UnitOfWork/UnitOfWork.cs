using OsLog.Application.Common;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.UnitOfWork;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public EfUnitOfWork(
        AppDbContext context,
        IOrcamentoItemRepository orcamentoItens,
        IPagamentoRepository pagamentos,
        IStatusHistoricoRepository statusHistoricos,
        IClienteRepository clientes,
        ITecnicoRepository tecnicos,
        IEmpresaRepository empresas,
        IUnidadeRepository unidades,
        IUsuarioAcessoRepository usuarioAcessos)
    {
        _context = context;
        OrcamentoItens = orcamentoItens;
        Pagamentos = pagamentos;
        StatusHistoricos = statusHistoricos;
        Clientes = clientes;
        Tecnicos = tecnicos;
        Empresas = empresas;
        Unidades = unidades;
        UsuarioAcessos = usuarioAcessos;
    }

    public IOrcamentoItemRepository OrcamentoItens { get; }
    public IPagamentoRepository Pagamentos { get; }
    public IStatusHistoricoRepository StatusHistoricos { get; }
    public IClienteRepository Clientes { get; }
    public ITecnicoRepository Tecnicos { get; }
    public IEmpresaRepository Empresas { get; }
    public IUnidadeRepository Unidades { get; }
    public IUsuarioAcessoRepository UsuarioAcessos { get; }

    public async Task<int> CommitAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public ValueTask DisposeAsync()
        => _context.DisposeAsync();
}
