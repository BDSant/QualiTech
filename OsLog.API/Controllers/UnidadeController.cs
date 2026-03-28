using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Unidade;
using OsLog.Application.Ports.ApplicationServices;

namespace OsLog.API.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/empresas/{empresaId:guid}/unidades")]
public class UnidadeController : BaseApiController
{
    private readonly IUnidadeService _unidadeService;

    public UnidadeController(IUnidadeService unidadeService)
    {
        _unidadeService = unidadeService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OsLogResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid empresaId,
        [FromBody] UnidadeCreateDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _unidadeService.Create(empresaId, dto, ct);

        if (id <= 0)
        {
            return NotFound(
                OsLogResponse.Critica(
                    codigo: CodigosOsLog.EMPRESA_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.EMPRESA_NAO_ENCONTRADA)));
        }

        var versao = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";

        return CreatedAtAction(
            nameof(GetById),
            new { version = versao, empresaId, id },
            OsLogResponse<int>.Ok(
                dados: id,
                mensagem: "Unidade criada com sucesso."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(OsLogResponse<IEnumerable<UnidadeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(Guid empresaId, CancellationToken ct)
    {
        var unidades = await _unidadeService.GetAllByEmpresa(empresaId, ct);

        if (unidades.Count == 0)
        {
            return NotFound(
                OsLogResponse.Critica(
                    codigo: CodigosOsLog.UNIDADE_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.UNIDADE_NAO_ENCONTRADA)));
        }

        return Ok(
            OsLogResponse<IEnumerable<UnidadeDto>>.Ok(
                dados: unidades,
                mensagem: "Unidades retornadas com sucesso."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OsLogResponse<UnidadeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid empresaId, int id, CancellationToken ct)
    {
        var unidade = await _unidadeService.GetById(id, empresaId, ct);

        if (unidade is null)
        {
            return NotFound(
                OsLogResponse.Critica(
                    codigo: CodigosOsLog.UNIDADE_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.UNIDADE_NAO_ENCONTRADA)));
        }

        return Ok(
            OsLogResponse<UnidadeDto>.Ok(
                dados: unidade,
                mensagem: "Unidade encontrada."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid empresaId, int id, CancellationToken ct)
    {
        var ok = await _unidadeService.Delete(id, empresaId, ct);

        if (!ok)
        {
            return NotFound(
                OsLogResponse.Critica(
                    codigo: CodigosOsLog.UNIDADE_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.UNIDADE_NAO_ENCONTRADA)));
        }

        return NoContent();
    }
}