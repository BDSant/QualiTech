using Microsoft.EntityFrameworkCore;
using OsLog.Domain.Entities;

namespace OsLog.Infrastructure.EntityFramework;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Unidade> Unidades => Set<Unidade>();
    public DbSet<OrdemServico> OrdemServicos => Set<OrdemServico>();
    public DbSet<OrcamentoItem> OrcamentoItens => Set<OrcamentoItem>();
    public DbSet<PagamentoOs> PagamentosOs => Set<PagamentoOs>();
    public DbSet<StatusHistorico> StatusHistoricos => Set<StatusHistorico>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
    public DbSet<OrdemServicoAcessorio> OrdemServicoAcessorios => Set<OrdemServicoAcessorio>();
    public DbSet<OrdemServicoFoto> OrdemServicoFotos => Set<OrdemServicoFoto>();
    public DbSet<OrdemServicoComissao> OrdemServicoComissoes => Set<OrdemServicoComissao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}