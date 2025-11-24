using AutoMapper;
using OsLog.Application.DTOs.OrdemServico;
using OsLog.Application.Mapping;
using OsLog.Domain.Entities;

namespace OsLog.Mapping.Tests.Mapping;

public class Mapping_OrdemServicoTests
{
    private readonly IMapper _mapper;

    public Mapping_OrdemServicoTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrdemServicoProfile>();
            cfg.AddProfile<ClienteProfile>();
            cfg.AddProfile<TecnicoProfile>();
        });

        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "[Mapping] OrdemServico -> OrdemServicoDetailDto")]
    [Trait("Category", "Mapping")]
    public void Mapping_OrdemServico_To_DetailDto_Should_MapCorrectly()
    {
        // Arrange
        var entidade = new OrdemServico
        {
            Id = 1,
            Cliente = new Cliente { Nome = "João da Silva" },
            Tecnico = new Tecnico { Nome = "Maria Técnica" },
            Itens = new List<OrcamentoItem>
            {
                new OrcamentoItem { DescricaoItem = "MOTO G9, com riscos" }
            },
            Pagamentos = new List<PagamentoOs>(),
            Historicos = new List<StatusHistorico>(),
            Acessorios = new List<OrdemServicoAcessorio>(),
            Fotos = new List<OrdemServicoFoto>(),
            Comissoes = new List<OrdemServicoComissao>()
        };

        // Act
        var dto = _mapper.Map<OrdemServicoDetailDto>(entidade);

        // Assert
        Assert.Equal(entidade.Cliente.Nome, dto.ClienteNome);
        Assert.Equal(entidade.Tecnico.Nome, dto.TecnicoNome);
        Assert.NotNull(dto.Itens);
        Assert.Single(dto.Itens);
    }

    [Fact(DisplayName = "[Mapping] OrdemServico -> OrdemServicoListDto (Flattening)")]
    [Trait("Category", "Mapping")]
    public void OrdemServico_To_ListDto_Should_Map_NavigationMembers()
    {
        var entity = new OrdemServico
        {
            Cliente = new Cliente { Nome = "João" },
            Tecnico = new Tecnico { Nome = "Maria" }
        };

        var dto = _mapper.Map<OrdemServicoListDto>(entity);

        Assert.Equal("João", dto.ClienteNome);
        Assert.Equal("Maria", dto.TecnicoNome);
    }

    [Fact(DisplayName = "[Mapping] OrdemServicoCreateDto -> OrdemServico (Campos de domínio protegidos)")]
    [Trait("Category", "Mapping")]
    public void OrdemServicoCreateDto_Should_Not_Map_InternalFields()
    {
        var dto = new OrdemServicoCreateDto
        {
            DescricaoProblema = "Não liga"
        };

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrdemServicoProfile>();
        }).CreateMapper();

        var entity = mapper.Map<OrdemServico>(dto);

        Assert.Equal(dto.DescricaoProblema, entity.DescricaoProblema);
        Assert.Null(entity.Cliente);
        Assert.Equal(0, entity.Id);
    }

}
