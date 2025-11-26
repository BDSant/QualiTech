using Microsoft.AspNetCore.Mvc;
using OsLog.Api.Extensions;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Interfaces.Services;

namespace OsLog.Api.Controllers;

[ApiController]
[Route("api/empresas")]
public class EmpresaController : ControllerBase
{
    private readonly IEmpresaService _empresaService;
    private readonly IUnidadeService _unidadeService;

    public EmpresaController(IEmpresaService empresaService, IUnidadeService unidadeService)
    {
        _empresaService = empresaService;
        _unidadeService = unidadeService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmpresaCreateDto dto, CancellationToken ct)
    {
        // Validação de modelo -> envelope padronizado
        if (!ModelState.IsValid)
            return this.ValidationProblemOsLog(ModelState);

        var usuarioId = 1; // TODO: pegar do usuário logado
        var id = await _empresaService.CriarEmpresaAsync(dto, usuarioId, ct);

        var payload = new { Id = id };

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            OsLogResponse<object>.Ok(
                dados: payload,
                mensagem: "Empresa criada com sucesso.")
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var lista = await _empresaService.ListarAsync(ct);

        return Ok(
            OsLogResponse<IEnumerable<EmpresaListDto>>.Ok(
                dados: lista,
                mensagem: "Empresas retornadas com sucesso.")
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var empresa = await _empresaService.ObterPorIdAsync(id, ct);

        if (empresa is null)
        {
            return NotFound(
                OsLogResponse<EmpresaDetailDto>.Critica(
                    codigo: CodigosOsLog.EMPRESA_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.EMPRESA_NAO_ENCONTRADA)
                )
            );
        }

        return Ok(
            OsLogResponse<EmpresaDetailDto>.Ok(
                dados: empresa,
                mensagem: "Empresa encontrada.")
        );
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: pegar do usuário logado

        var ok = await _empresaService.SoftDeleteAsync(id, usuarioId, ct);
        if (!ok)
        {
            return NotFound(
                OsLogResponse<object>.Critica(
                    codigo: CodigosOsLog.EMPRESA_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.EMPRESA_NAO_ENCONTRADA)
                )
            );
        }

        // Sem payload quando exclui com sucesso
        return NoContent();
    }

    [HttpPost("{empresaId:int}/unidades")]
    public async Task<IActionResult> CriarUnidade(
        int empresaId,
        [FromBody] UnidadeCreateDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return this.ValidationProblemOsLog(ModelState);

        var usuarioId = 1; // TODO: pegar do usuário logado

        var unidadeId = await _unidadeService.CriarUnidadeAsync(
            empresaId,
            dto,
            usuarioId,
            ct);

        var payload = new { Id = unidadeId, EmpresaId = empresaId };

        return CreatedAtAction(
            nameof(ListarUnidades),
            new { empresaId },
            OsLogResponse<object>.Ok(
                dados: payload,
                mensagem: "Unidade criada com sucesso.")
        );
    }

    [HttpGet("{empresaId:int}/unidades")]
    public async Task<IActionResult> ListarUnidades(int empresaId, CancellationToken ct)
    {
        var unidades = await _unidadeService.ListarPorEmpresaAsync(empresaId, ct);

        return Ok(
            OsLogResponse<IEnumerable<UnidadeDto>>.Ok(
                dados: unidades,
                mensagem: "Unidades retornadas com sucesso.")
        );
    }

    [HttpGet("~/api/unidades")]
    public async Task<IActionResult> ListarTodasUnidades(CancellationToken ct)
    {
        var unidades = await _unidadeService.ListarTodasAsync(ct);

        return Ok(
            OsLogResponse<IEnumerable<UnidadeDto>>.Ok(
                dados: unidades,
                mensagem: "Unidades retornadas com sucesso.")
        );
    }
}
