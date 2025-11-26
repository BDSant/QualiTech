using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OsLog.Infrastructure.EntityFramework;

namespace OsLog.Tests.Integration;

public class OsLogApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Só garante que o banco InMemory foi criado
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // se quiser, aqui você pode popular dados de seed para alguns testes
        });
    }


    //protected override void ConfigureWebHost(IWebHostBuilder builder)
    //{
    //    builder.UseEnvironment("Testing");

    //    builder.ConfigureServices(services =>
    //    {
    //        // Remove o DbContext configurado no Program (SQL Server)
    //        var descriptor = services.SingleOrDefault(
    //            d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

    //        if (descriptor is not null)
    //            services.Remove(descriptor);

    //        // Adiciona SQLite em memória para os testes
    //        services.AddDbContext<AppDbContext>(options =>
    //        {
    //            options.UseSqlite("DataSource=:memory:");
    //        });

    //        // Constrói o provider e garante criação/migrations
    //        var sp = services.BuildServiceProvider();

    //        using var scope = sp.CreateScope();
    //        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    //        db.Database.OpenConnection();      // mantém a conexão viva
    //        db.Database.Migrate();             // aplica migrations
    //    });
    //}
}
