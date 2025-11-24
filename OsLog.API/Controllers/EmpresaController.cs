using Microsoft.AspNetCore.Mvc;
using OsLog.Application.Common.Errors;
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
    public async Task<ActionResult> Create([FromBody] EmpresaCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var usuarioId = 1; // TODO: pegar do usuário logado (Claims/Identity)

        var id = await _empresaService.CriarEmpresaAsync(dto, usuarioId, ct);

        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmpresaListDto>>> GetAll(CancellationToken ct)
    {
        var lista = await _empresaService.ListarAsync(ct);
        return Ok(lista);
    }

    //[HttpGet("{id:int}")]
    //public async Task<ActionResult<EmpresaDetailDto>> GetById(int id, CancellationToken ct)
    //{
    //    var empresa = await _empresaService.ObterPorIdAsync(id, ct);
    //    if (empresa is null)
    //        return NotFound();

    //    return Ok(empresa);
    //}

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var empresa = await _empresaService.ObterPorIdAsync(id, ct);

        if (empresa is null)
            return NotFound(
                OsLogResponse.Critica(
                    CodigosOsLog.EMPRESA_NAO_ENCONTRADA,
                    CriticasOsLog.RetornaCritica(CodigosOsLog.EMPRESA_NAO_ENCONTRADA)
                )
            );

        return Ok(OsLogResponse.Ok(empresa));
    }








    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: pegar do usuário logado

        var ok = await _empresaService.SoftDeleteAsync(id, usuarioId, ct);
        if (!ok)
            return NotFound();

        return NoContent();
    }



    [HttpPost("{empresaId:int}/unidades")]
    public async Task<ActionResult> CriarUnidade(int empresaId, [FromBody] UnidadeCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var usuarioId = 1; // TODO: pegar do usuário logado

        var unidadeId = await _unidadeService.CriarUnidadeAsync(empresaId, dto, usuarioId, ct);

        return CreatedAtAction(nameof(ListarUnidades), new { empresaId }, null);
    }

    [HttpGet("{empresaId:int}/unidades")]
    public async Task<ActionResult<IEnumerable<UnidadeDto>>> ListarUnidades(int empresaId, CancellationToken ct)
    {
        var unidades = await _unidadeService.ListarPorEmpresaAsync(empresaId, ct);
        return Ok(unidades);
    }

    //[ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("~/api/unidades")]
    public async Task<ActionResult<IEnumerable<UnidadeDto>>> ListarTodasUnidades(CancellationToken ct)
    {
        var unidades = await _unidadeService.ListarTodasAsync(ct);
        return Ok(unidades);
    }
}
