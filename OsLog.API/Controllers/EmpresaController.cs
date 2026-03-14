using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OsLog.API.Extensions;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.ApplicationServices;

namespace OsLog.API.Controllers;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/empresas")]
public class EmpresaController : BaseApiController
{
    private readonly IEmpresaService _empresaService;

    public EmpresaController(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OsLogResponse<int>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] EmpresaCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return this.ValidationProblemOsLog(ModelState);

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Unauthorized(
                OsLogResponse.Critica(
                    codigo: CodigosOsLog.USUARIO_NAO_AUTENTICADO,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.USUARIO_NAO_AUTENTICADO)));
        }

        var id = await _empresaService.Create(dto, usuarioId.Value, ct);

        var versao = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";

        return CreatedAtAction(
            nameof(GetById),
            new { version = versao, id },
            OsLogResponse<int>.Ok(
                dados: id,
                mensagem: "Empresa criada com sucesso."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(OsLogResponse<IEnumerable<EmpresaListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var lista = await _empresaService.GetAll(ct);

        if (lista.Count <= 0)
        {
            return NotFound(
                OsLogResponse<EmpresaDetailDto>.Critica(
                    codigo: CodigosOsLog.EMPRESA_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.EMPRESA_NAO_ENCONTRADA)));
        }

        return Ok(
            OsLogResponse<IEnumerable<EmpresaListDto>>.Ok(
                dados: lista,
                mensagem: "Empresas retornadas com sucesso."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OsLogResponse<EmpresaDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var empresa = await _empresaService.GetById(id, ct);

        if (empresa is null)
        {
            return NotFound(
                OsLogResponse<EmpresaDetailDto>.Critica(
                    codigo: CodigosOsLog.EMPRESA_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.EMPRESA_NAO_ENCONTRADA)));
        }

        return Ok(
            OsLogResponse<EmpresaDetailDto>.Ok(
                dados: empresa,
                mensagem: "Empresa encontrada."));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(OsLogResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Unauthorized(
                OsLogResponse.Critica(
                    codigo: CodigosOsLog.USUARIO_NAO_AUTENTICADO,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.USUARIO_NAO_AUTENTICADO)));
        }

        var ok = await _empresaService.Delete(id, usuarioId.Value, ct);
        if (!ok)
        {
            return NotFound(
                OsLogResponse<object>.Critica(
                    codigo: CodigosOsLog.EMPRESA_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.EMPRESA_NAO_ENCONTRADA)));
        }

        return NoContent();
    }
}