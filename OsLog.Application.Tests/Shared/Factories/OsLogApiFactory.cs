using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Tests.Shared.Factories;

/// <summary>
/// WebApplicationFactory para testes de integração.
///
/// Importante: a solução usa o JWKS/Keys persistido em banco (NetDevPack). O provider
/// InMemory do EF Core frequentemente causa falhas de validação de token (401) porque
/// não implementa comportamentos relacionais esperados pelo store. Por isso, usamos
/// SQLite InMemory (relacional) mantendo a conexão aberta durante os testes.
/// </summary>
public sealed class OsLogApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, cfg) =>
        {
            // Credenciais usadas nos testes para autenticar e exercer papel Master/Admin
            var overrides = new Dictionary<string, string?>
            {
                ["IdentitySeed:MasterEmail"] = "admin2@oslog.local",
                ["IdentitySeed:MasterPassword"] = "Admin@123",
                ["IdentitySeed:MasterUserName"] = "admin2",
            };

            cfg.AddInMemoryCollection(overrides!);
        });

        builder.ConfigureServices(services =>
        {
            // Substitui AppDbContext pelo SQLite InMemory relacional
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor);

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlite(_connection);
            });

            // Cria o esquema antes de subir a aplicação
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        try
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        catch
        {
            // Ignorar
        }
        finally
        {
            _connection = null;
        }
    }
}
