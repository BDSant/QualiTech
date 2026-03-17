using Microsoft.AspNetCore.Mvc;
using OsLog.API.Extensions;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Unidade;
using OsLog.Application.Ports.ApplicationServices;

namespace OsLog.API.Controllers;

[ApiController]
[Route("api/unidades")]
public class UnidadeController : ControllerBase
{
    private readonly IUnidadeService _unidadeService;

    public UnidadeController(IUnidadeService unidadeService)
    {
        _unidadeService = unidadeService;
    }

    [HttpPost("{empresaId:guid}/unidades")]
    public async Task<IActionResult> Create(Guid empresaId, [FromBody] UnidadeCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return this.ValidationProblemOsLog(ModelState);

        var usuarioId = 1; // TODO: pegar do usuário logado

        var unidadeId = await _unidadeService.Create(
            empresaId,
            dto,
            usuarioId,
            ct);

        var payload = new { Id = unidadeId, EmpresaId = empresaId };

        return CreatedAtAction(nameof(GetById),
                               new { empresaId },
                               OsLogResponse<object>.Ok(dados: payload,
                                                        mensagem: "Unidade criada com sucesso."));
    }

    [HttpGet("~/api/unidades")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var unidades = await _unidadeService.GetAll(ct);

        if (unidades.Count <= 0)
        {
            return NotFound(
                OsLogResponse<UnidadeCreateDto>.Critica(
                    codigo: CodigosOsLog.UNIDADE_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.UNIDADE_NAO_ENCONTRADA)
                )
            );
        }

        return Ok(
            OsLogResponse<IEnumerable<UnidadeDto>>.Ok(
                dados: unidades,
                mensagem: "Unidades retornadas com sucesso.")
        );
    }

    [HttpGet("{empresaId:guid}/unidades")]
    public async Task<IActionResult> GetById(Guid empresaId, CancellationToken ct)
    {
        var unidades = await _unidadeService.GetById(empresaId, ct);

        if (unidades.Count <= 0)
        {
            return NotFound(
                OsLogResponse<UnidadeCreateDto>.Critica(
                    codigo: CodigosOsLog.UNIDADE_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.UNIDADE_NAO_ENCONTRADA)
                )
            );
        }

        return Ok(
            OsLogResponse<IEnumerable<UnidadeDto>>.Ok(
                dados: unidades,
                mensagem: "Unidades retornadas com sucesso.")
        );
    }

    [HttpDelete("{empresaId:guid}/{unidadeId:guid}")]
    public async Task<IActionResult> Delete(Guid empresaId, int unidadeId, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: pegar do usuário logado

        var ok = await _unidadeService.Delete(empresaId, unidadeId, usuarioId, ct);
        if (!ok)
        {
            return NotFound(
                OsLogResponse<object>.Critica(
                    codigo: CodigosOsLog.UNIDADE_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.UNIDADE_NAO_ENCONTRADA)
                )
            );
        }

        // Sem payload quando exclui com sucesso
        return NoContent();
    }

}
