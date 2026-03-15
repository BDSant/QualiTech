using OsLog.API.Configurations;
using OsLog.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddApiConfiguration(builder.Configuration);
builder.Services.AddDependencyInjectionConfiguration();
builder.Services.UseSwaggerDocumentation();
builder.Services.UseOpenApiDocuments();

var app = builder.Build();

app.MapGet("/", () => "API OsLog rodando...");

app.UseAppConfiguration();

await IdentitySeeder.SeedAsync(app.Services);

app.Run();