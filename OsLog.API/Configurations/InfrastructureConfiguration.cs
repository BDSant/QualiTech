using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OsLog.Application.Mapping;
using OsLog.Infrastructure.EntityFramework;
using OsLog.Infrastructure.Identity;

namespace OsLog.API.Configurations;

public static class InfrastructureConfiguration
{
    /// <summary>
    /// Registra os componentes de infraestrutura da aplicação, incluindo banco de dados,
    /// Identity, JWKS e AutoMapper.
    /// </summary>
    /// <param name="services">Coleção de serviços utilizada para registrar os componentes de infraestrutura.</param>
    /// <param name="configuration">Configuração da aplicação, utilizada para obter dados como a connection string.</param>
    /// <param name="environment">Ambiente atual da aplicação, utilizado para aplicar configurações específicas por ambiente.</param>
    /// <returns>A própria coleção de serviços, permitindo o encadeamento das configurações.</returns>
    public static IServiceCollection AddInfrastructureConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddDatabaseConfiguration(configuration, environment);
        services.AddIdentityConfiguration();
        services.AddJwksConfiguration();
        services.AddAutoMapperConfiguration();

        return services;
    }

    private static IServiceCollection AddDatabaseConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (environment.IsEnvironment("Testing"))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("OsLog_TestDb"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.EnableDetailedErrors();

                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });
        }

        return services;
    }

    private static IServiceCollection AddIdentityConfiguration(this IServiceCollection services)
    {
        services
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

        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        return services;
    }
    private static IServiceCollection AddJwksConfiguration(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services
            .AddJwksManager()
            .PersistKeysToDatabaseStore<AppDbContext>()
            .UseJwtValidation();

        return services;
    }

    private static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<MapperConfiguration>(mcf =>
        {
            var loggerFactory = mcf.GetRequiredService<ILoggerFactory>();

            var cfg = new MapperConfiguration(config =>
            {
                config.AddMaps(typeof(ClienteProfile).Assembly);
            }, loggerFactory);

            cfg.AssertConfigurationIsValid();

            return cfg;
        });

        services.AddScoped<IMapper>(mcf => mcf.GetRequiredService<MapperConfiguration>().CreateMapper());

        return services;
    }
}