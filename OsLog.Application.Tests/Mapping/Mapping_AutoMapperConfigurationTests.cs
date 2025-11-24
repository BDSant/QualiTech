using AutoMapper;
using OsLog.Application.Mapping;

namespace OsLog.Mapping.Tests.Mapping;

public class Mapping_AutoMapperConfigurationTests
{
    private readonly MapperConfiguration _config;

    public Mapping_AutoMapperConfigurationTests()
    {
        _config = new MapperConfiguration(cfg =>
        {
            // Carrega todos os Profiles do assembly de Application
            cfg.AddMaps(typeof(ClienteProfile).Assembly);
        });
    }

    [Fact(DisplayName = "[Mapping] Configuração global do AutoMapper deve ser válida")]
    [Trait("Category", "Mapping")]
    public void MappingConfigurationIsValid()
    {
        _config.AssertConfigurationIsValid();
    }
}
