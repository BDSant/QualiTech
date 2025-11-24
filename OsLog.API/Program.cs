using Microsoft.EntityFrameworkCore;
using OsLog.Application.Common;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Application.Interfaces.Services;
using OsLog.Application.Services;
using OsLog.Infrastructure.EntityFramework;
using OsLog.Infrastructure.Repositories;
using OsLog.Infrastructure.UnitOfWork;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// 1) String de conexão (ajusta o nome conforme seu appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("string de conexão 'DefaultConnection' não encontrada.");

// 2) Registro do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3) Repositórios
builder.Services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<IStatusHistoricoRepository, StatusHistoricoRepository>();
builder.Services.AddScoped<IOrcamentoItemRepository, OrcamentoItemRepository>();
builder.Services.AddScoped<IOrdemServicoAcessorioRepository, OrdemServicoAcessorioRepository>();
builder.Services.AddScoped<IOrdemServicoFotoRepository, OrdemServicoFotoRepository>();
builder.Services.AddScoped<IOrdemServicoComissaoRepository, OrdemServicoComissaoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ITecnicoRepository, TecnicoRepository>();

//builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
//builder.Services.AddScoped<IUnidadeRepository, UnidadeRepository>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IUnidadeService, UnidadeService>();



// 3.1) UnitOfWork
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// 4) Serviços de aplicação
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<OrdemServicoService>();
builder.Services.AddScoped<PagamentoService>();
builder.Services.AddScoped<TecnicoService>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<UnidadeService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper
//builder.Services.AddAutoMapper(typeof(OsLogProfile));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OsLog.API v1");
        c.RoutePrefix = "swagger";
    });
}

// 5) (Opcional) aplicar migrations automaticamente na inicialização
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Necessário para WebApplicationFactory<Program>
public partial class Program { }
