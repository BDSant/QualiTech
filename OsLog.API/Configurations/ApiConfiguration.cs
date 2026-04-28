using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.Jwt.Core.Model;
using OsLog.API.Authorization;
using OsLog.Application.Common.Responses;
using AppJwtOptions = OsLog.Application.Common.Security.Jwt.JwtOptions;

namespace OsLog.API.Configurations;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersConfiguration();
        services.AddApiVersioningConfiguration();
        services.AddOptionsConfiguration(configuration);
        services.AddAuthenticationConfiguration(configuration);
        services.AddCorsConfiguration(configuration);

        return services;
    }

    private static IServiceCollection AddControllersConfiguration(this IServiceCollection services)
    {
        services
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(kvp => kvp.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                    var payload = new OsLogResponse
                    {
                        Sucesso = false,
                        Codigo = CodigosOsLog.ERRO_VALIDACAO,
                        Mensagem = "Dados inválidos.",
                        Dados = null,
                        Erros = errors
                    };

                    return new BadRequestObjectResult(payload);
                };
            });

        return services;
    }

    private static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    private static IServiceCollection AddOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppJwtOptions>(configuration.GetSection("Jwt"));
        return services;
    }

    private static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration
            .GetSection("Jwt")
            .Get<OsLog.Application.Common.Security.Jwt.JwtOptions>()
            ?? throw new InvalidOperationException("Configuração Jwt não encontrada.");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.IncludeErrorDetails = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience
                };

                // DEBUG
                //options.Events = new JwtBearerEvents
                //{
                //    OnAuthenticationFailed = context =>
                //    {
                //        var logger = context.HttpContext.RequestServices
                //            .GetRequiredService<ILoggerFactory>()
                //            .CreateLogger("JwtBearer");

                //        logger.LogError(context.Exception,
                //            "Falha na autenticação JWT. Tipo: {ExceptionType}. Mensagem: {Message}",
                //            context.Exception.GetType().Name,
                //            context.Exception.Message);

                //        return Task.CompletedTask;
                //    },
                //    OnTokenValidated = context =>
                //    {
                //        var logger = context.HttpContext.RequestServices
                //            .GetRequiredService<ILoggerFactory>()
                //            .CreateLogger("JwtBearer");

                //        logger.LogInformation("JWT validado com sucesso.");
                //        return Task.CompletedTask;
                //    },
                //    OnChallenge = context =>
                //    {
                //        var logger = context.HttpContext.RequestServices
                //            .GetRequiredService<ILoggerFactory>()
                //            .CreateLogger("JwtBearer");

                //        logger.LogWarning("JWT challenge. Error={Error}; Description={Description}",
                //            context.Error,
                //            context.ErrorDescription);

                //        return Task.CompletedTask;
                //    }
                //};

            });

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// CORS restrito
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}