using OsLog.Application.Common;
using OsLog.Application.Ports.ApplicationServices;
using OsLog.Application.Ports.Identity.Admin;
using OsLog.Application.Ports.Identity.Runtime;
using OsLog.Application.Ports.Persistence.Repositories;
using OsLog.Application.Ports.Security;
using OsLog.Application.Services;
using OsLog.Application.UseCases.Autenticacao.ChangePassword;
using OsLog.Application.UseCases.Autenticacao.Login;
using OsLog.Application.UseCases.Autenticacao.Logout;
using OsLog.Application.UseCases.Autenticacao.RefreshToken;
using OsLog.Application.UseCases.Autenticacao.ResetPassword;
using OsLog.Application.UseCases.Users;
using OsLog.Infrastructure.Identity;
using OsLog.Infrastructure.Identity.Gateway;
using OsLog.Infrastructure.Identity.Runtime;
using OsLog.Infrastructure.Repositories;
using OsLog.Infrastructure.Security.Jwt;
using OsLog.Infrastructure.UnitOfWork;

namespace OsLog.API.Configurations;

public static class DependencyInjectionConfiguration
{
    /// <summary>
    /// Registra as dependências da aplicação, incluindo repositórios,
    /// serviços, gateways, casos de uso e unidade de trabalho.
    /// </summary>
    /// <param name="services">Coleção de serviços utilizada para registrar as dependências da aplicação.</param>
    /// <returns>A própria coleção de serviços, permitindo o encadeamento das configurações.</returns>

    public static IServiceCollection AddDependencyInjectionConfiguration(this IServiceCollection services)
    {
        services.AddRepositories();
        services.AddApplicationServices();
        services.AddUseCases();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IOrcamentoItemRepository, OrcamentoItemRepository>();
        services.AddScoped<IPagamentoRepository, PagamentoRepository>();
        services.AddScoped<IStatusHistoricoRepository, StatusHistoricoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ITecnicoRepository, TecnicoRepository>();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IUnidadeRepository, UnidadeRepository>();
        services.AddScoped<IUsuarioAcessoRepository, UsuarioAcessoRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioAcessoService, UsuarioAcessoService>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityGateway, IdentityGateway>();
        services.AddScoped<IUsuarioAutenticadoResolver, UsuarioAutenticadoResolver>();

        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ITecnicoService, TecnicoService>();
        services.AddScoped<IUnidadeService, UnidadeService>();
        services.AddScoped<IIdentityAdminGateway, IdentityAdminGateway>();

        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
        services.AddScoped<ILogoutUseCase, LogoutUseCase>();
        services.AddScoped<IChangePasswordUseCase, ChangePasswordUseCase>();
        services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();

        services.AddScoped<CreateUserUseCase>();
        services.AddScoped<GetUserByIdUseCase>();

        return services;
    }
}