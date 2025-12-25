using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OsLog.Domain.Entities;
using OsLog.Infrastructure.Identity;

namespace OsLog.Infrastructure.EntityFramework;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Empresa> Empresas { get; set; } = null!;
    public DbSet<Unidade> Unidades { get; set; } = null!;
    public DbSet<UsuarioAcesso> UsuarioAcessos { get; set; } = null!;
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Tecnico> Tecnicos => Set<Tecnico>();

    //public DbSet<OrdemServico> OrdemServicos => Set<OrdemServico>();
    //public DbSet<OrcamentoItem> OrcamentoItens => Set<OrcamentoItem>();
    //public DbSet<OrdemServicoAcessorio> OrdemServicoAcessorios => Set<OrdemServicoAcessorio>();
    //public DbSet<OrdemServicoFoto> OrdemServicoFotos => Set<OrdemServicoFoto>();
    //public DbSet<OrdemServicoComissao> OrdemServicoComissoes => Set<OrdemServicoComissao>();
    //public DbSet<PagamentoOs> PagamentosOs => Set<PagamentoOs>();
    //public DbSet<StatusHistorico> StatusHistoricos => Set<StatusHistorico>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}