using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace OsLog.Mapping.Tests.Mapping;

public class Mapping_AutoMapperConfigurationTests
{
    private readonly MapperConfiguration _config;

    public Mapping_AutoMapperConfigurationTests()
    {
        _config = new MapperConfiguration(cfg =>
        {
            var appAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName != null 
                && a.FullName.StartsWith("OsLog."));

            cfg.AddMaps(appAssemblies);
        }, NullLoggerFactory.Instance);
    }

    [Fact(DisplayName = "[Mapping] Configuração global do AutoMapper deve ser válida")]
    [Trait("Category", "Mapping")]
    public void MappingConfigurationIsValid()
    {
        _config.AssertConfigurationIsValid();
    }
}
