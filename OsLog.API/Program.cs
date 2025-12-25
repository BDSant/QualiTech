using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OsLog.Api.Configuration;
using OsLog.Api.Middlewares;
using OsLog.API.Services.Auth;
using OsLog.Application.Common;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Application.Interfaces.Services;
using OsLog.Application.Services;
using OsLog.Infrastructure.EntityFramework;
using OsLog.Infrastructure.Identity;
using OsLog.Infrastructure.Repositories;
using OsLog.Infrastructure.UnitOfWork;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1) String de conexão (ajusta o nome conforme seu appsettings.json)
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    // Banco em memória para testes de integração
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("OsLogTestsDb"));
}
else if (builder.Environment.IsDevelopment())
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("string de conexão 'DefaultConnection' não encontrada.");

    // 2) Registro do DbContext - Contexto de acesso ao Banco de Dados
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services
        .AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();
}

// 3) Repositórios
//builder.Services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<IStatusHistoricoRepository, StatusHistoricoRepository>();
builder.Services.AddScoped<IOrcamentoItemRepository, OrcamentoItemRepository>();
//builder.Services.AddScoped<IOrdemServicoAcessorioRepository, OrdemServicoAcessorioRepository>();
//builder.Services.AddScoped<IOrdemServicoFotoRepository, OrdemServicoFotoRepository>();
//builder.Services.AddScoped<IOrdemServicoComissaoRepository, OrdemServicoComissaoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ITecnicoRepository, TecnicoRepository>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<IUnidadeRepository, UnidadeRepository>();
builder.Services.AddScoped<IUsuarioAcessoRepository, UsuarioAcessoRepository>();

// 3.1) Services de aplicação
builder.Services.AddScoped<ITecnicoService, TecnicoService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IUnidadeService, UnidadeService>();
builder.Services.AddScoped<IUsuarioAcessoService, UsuarioAcessoService>();

// 3.2) UnitOfWork
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// AutoMapper - Carrega todos os profiles do assembly
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

/// 4) Configurações de JWT (suas opções)
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

// Auth JWT (validação do token nos endpoints)
var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Serviço de geração de tokens
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

    OsLog.Api.Identity.IdentitySeed.SeedAsync(app.Services).GetAwaiter().GetResult();
}

app.UseHttpsRedirection();
app.UseOsLogExceptionHandling();

app.UseAuthentication(); // 👈 OBRIGATÓRIO antes do UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();


// Necessário para WebApplicationFactory<Program>
public partial class Program { }
