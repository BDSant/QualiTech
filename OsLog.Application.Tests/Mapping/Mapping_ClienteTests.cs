using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using OsLog.Application.DTOs.Clientes;
using OsLog.Application.Mapping;
using OsLog.Domain.Entities;

namespace OsLog.Mapping.Tests.Mapping;

public class Mapping_ClienteTests
{
    private readonly IMapper _mapper;

    public Mapping_ClienteTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ClienteProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "[Mapping] ClienteDto -> Cliente -> ClienteDto (RoundTrip)")]
    [Trait("Category", "Mapping")]
    public void Cliente_RoundTrip_Should_PreserveFields()
    {
        var dto = new ClienteDto
        {
            Id = 10,
            Nome = "José"
        };

        var entity = _mapper.Map<Cliente>(dto);
        var result = _mapper.Map<ClienteDto>(entity);

        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.Nome, result.Nome);
    }
}
