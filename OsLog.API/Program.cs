using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OsLog.Api.Identity;
using OsLog.Application.Abstractions.Identity;
using OsLog.Application.Abstractions.Security;
using OsLog.Application.Common;
using OsLog.Application.Common.Security.Jwt;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Application.Interfaces.Services;
using OsLog.Application.Mapping;
using OsLog.Application.Services;
using OsLog.Application.UseCases.Autenticacao.ChangePassword;
using OsLog.Application.UseCases.Autenticacao.Login;
using OsLog.Application.UseCases.Autenticacao.Logout;
using OsLog.Application.UseCases.Autenticacao.RefreshToken;
using OsLog.Application.UseCases.Autenticacao.ResetPassword;
using OsLog.Infrastructure.EntityFramework;
using OsLog.Infrastructure.Identity;
using OsLog.Infrastructure.Identity.Gateway;
using OsLog.Infrastructure.Repositories;
using OsLog.Infrastructure.Security.Jwt;
using OsLog.Infrastructure.UnitOfWork;
using M = Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Controllers / Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new M.OpenApiInfo
    {
        Title = "OsLog.API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new M.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = M.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = M.ParameterLocation.Header,
        Description = "Informe: Bearer {seu_token}"
    });

    // Uso do modelo compatível com Microsoft.OpenApi package
    options.AddSecurityRequirement(document => new M.OpenApiSecurityRequirement
    {
        [new M.OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

// Options
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// DbContext (mesma lógica para todos os ambientes; muda apenas provider)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("OsLog_TestDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// JWT Validation (Issuer/Audience aqui; signing keys via JWKS)
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience
            // Signing keys serão resolvidas via UseJwtValidation() (JWKS)
        };
    });

builder.Services.AddAuthorization();

// Memory cache requerido por NetDevPack (JWKS stores)
builder.Services.AddMemoryCache();

// NetDevPack - JWKS + validação automática
builder.Services
    .AddJwksManager()
    .PersistKeysToDatabaseStore<AppDbContext>()
    .UseJwtValidation();

// AutoMapper (sem AutoMapper.Extensions.Microsoft.DependencyInjection)
builder.Services.AddSingleton<MapperConfiguration>(sp =>
{
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    var cfg = new MapperConfiguration(config =>
    {
        config.AddMaps(typeof(ClienteProfile).Assembly);
    }, loggerFactory);

    cfg.AssertConfigurationIsValid();
    return cfg;
});

builder.Services.AddScoped<IMapper>(sp =>
    sp.GetRequiredService<MapperConfiguration>().CreateMapper());

// Repositories / UoW
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Registre os repositórios concretos referenciados por UnitOfWork
builder.Services.AddScoped<IOrcamentoItemRepository, OrcamentoItemRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<IStatusHistoricoRepository, StatusHistoricoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ITecnicoRepository, TecnicoRepository>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<IUnidadeRepository, UnidadeRepository>();
builder.Services.AddScoped<IUsuarioAcessoRepository, UsuarioAcessoRepository>();

// UnitOfWork deve vir depois para que todas as dependências já estejam registradas
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IUsuarioAcessoService, UsuarioAcessoService>();
builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IIdentityGateway, IdentityGateway>();

// Auth UseCases
builder.Services.AddScoped<ILoginUseCase, LoginUseCase>();
builder.Services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
builder.Services.AddScoped<ILogoutUseCase, LogoutUseCase>();
builder.Services.AddScoped<IChangePasswordUseCase, ChangePasswordUseCase>();
builder.Services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();

// Application services
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ITecnicoService, TecnicoService>();
builder.Services.AddScoped<IUnidadeService, UnidadeService>();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed (roles/usuário master)
await IdentitySeed.SeedAsync(app.Services);

app.Run();
