using Microsoft.EntityFrameworkCore.Storage;
using OsLog.Application.Common;
using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
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

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        try
        {
            await _context.SaveChangesAsync(ct);

            if (_transaction is not null)
            {
                await _transaction.CommitAsync(ct);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync(ct);
                await _transaction.DisposeAsync();
                _transaction = null;
            }

            throw;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public ValueTask DisposeAsync() => _context.DisposeAsync();
}