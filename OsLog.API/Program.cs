using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OsLog.Api.Configuration;
using OsLog.Api.Identity;
using OsLog.Api.Services.Auth;
using OsLog.API.Services.Auth;
using OsLog.Application.Common;
using OsLog.Application.Interfaces.Repositories;
using OsLog.Application.Interfaces.Services;
using OsLog.Application.Mapping;
using OsLog.Application.Services;
using OsLog.Infrastructure.EntityFramework;
using OsLog.Infrastructure.Identity;
using OsLog.Infrastructure.Repositories;
using OsLog.Infrastructure.UnitOfWork;
using M = Microsoft.OpenApi;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Controllers / Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
//    {
//        Title = "OsLog.API",
//        Version = "v1"
//    });

//    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
//        Description = "Informe: Bearer {seu_token}"
//    });

//    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
//    {
//        {
//            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
//            {
//                Reference = new Microsoft.OpenApi.Models.OpenApiReference
//                {
//                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});


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

    // Forma compatível com o modelo atual (sem scheme.Reference = ...)
    options.AddSecurityRequirement(document => new M.OpenApiSecurityRequirement
    {
        [new M.OpenApiSecuritySchemeReference("Bearer", document)] = []
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
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IUsuarioAcessoService, UsuarioAcessoService>();
builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

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
