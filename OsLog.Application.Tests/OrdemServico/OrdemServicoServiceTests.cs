using Moq;
using OsLog.Application.Common;
using OsLog.Application.DTOs.OrdemServico;
using OsLog.Application.Services;
using OsLog.Domain.Entities;

namespace OsLog.Tests.Application.Services;

public class OrdemServicoServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly OrdemServicoService _service;

    public OrdemServicoServiceTests()
    {
        // aqui você mocka somente o necessário
        _service = new OrdemServicoService(_uowMock.Object, mapper: null!);
    }

    [Fact(DisplayName = "[Application] Deve abrir OS com status e sinal corretos")]
    [Trait("Category", "Application")]
    [Trait("SubCategory", "OrdemServicoService")]
    public async Task Deve_Abrir_Os_Com_Sinal_Correto()
    {
        // Arrange
        var dto = new OrdemServicoCreateDto
        {
            EmpresaId = 1,
            UnidadeId = 1,
            ClienteId = 1,
            TecnicoId = null,
            DescricaoProblema = "Celular não liga",
            SinalObrigatorio = true,
            ValorSinal = 100m
        };

        OrdemServico? capturada = null;

        _uowMock.Setup(u => u.OrdensServico.AddAsync(It.IsAny<OrdemServico>(), It.IsAny<CancellationToken>()))
            .Callback<OrdemServico, CancellationToken>((os, _) =>
            {
                os.Id = 123;
                capturada = os;
            })
            .Returns(Task.CompletedTask);

        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var id = await _service.AbrirOsAsync(dto, usuarioId: 99, CancellationToken.None);

        // Assert
        Assert.Equal(123, id);
        Assert.NotNull(capturada);
        Assert.Equal("PENDENTE_ANALISE", capturada!.StatusOs);
        Assert.True(capturada.SinalObrigatorio);
        Assert.Equal(100m, capturada.ValorSinal);
    }
}
