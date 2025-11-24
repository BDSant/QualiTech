using AutoMapper;
using OsLog.Application.DTOs.Tecnicos;
using OsLog.Application.Mapping;
using OsLog.Domain.Entities;
using Xunit;

namespace OsLog.Mapping.Tests.Mapping;

public class Mapping_TecnicoTests
{
    private readonly IMapper _mapper;

    public Mapping_TecnicoTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TecnicoProfile>();
        });

        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "[Mapping] TecnicoDto -> Tecnico -> TecnicoDto (RoundTrip)")]
    [Trait("Category", "Mapping")]
    public void Tecnico_RoundTrip_Should_PreserveFields()
    {
        var dto = new TecnicoDto
        {
            Id = 5,
            Nome = "Maria Técnica"
            // se tiver mais propriedades simples no DTO, pode preencher aqui
            // Ex: Especialidade, Ativo, etc.
        };

        var entity = _mapper.Map<Tecnico>(dto);
        var result = _mapper.Map<TecnicoDto>(entity);

        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.Nome, result.Nome);
        // Adicione asserts extras para outras propriedades que fizerem sentido
    }
}
