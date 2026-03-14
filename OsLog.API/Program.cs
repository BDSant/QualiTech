using OsLog.API.Configurations;
using OsLog.API.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiConfiguration(builder.Configuration);
builder.Services.AddInfrastructureConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddDependencyInjectionConfiguration();
builder.Services.UseSwaggerDocumentation();
builder.Services.UseOpenApiDocuments();

var app = builder.Build();

app.MapGet("/", () => "API OsLog rodando...");

app.UseAppConfiguration();

await IdentitySeed.SeedAsync(app.Services);

app.Run();