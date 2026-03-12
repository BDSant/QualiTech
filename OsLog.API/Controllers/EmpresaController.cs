using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OsLog.API.Extensions;
using OsLog.Application.Common.Responses;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.ApplicationServices;

namespace OsLog.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/empresas")]
public class EmpresaController : ControllerBase
{
    private readonly IEmpresaService _empresaService;

    public EmpresaController(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmpresaCreateDto dto, CancellationToken ct)
    {
        // Validação de modelo -> envelope padronizado
        if (!ModelState.IsValid)
            return this.ValidationProblemOsLog(ModelState);

        var usuarioId = 1; // TODO: pegar do usuário logado
        var id = await _empresaService.Create(dto, usuarioId, ct);

        //var payload = new { Id = id };

        return CreatedAtAction(nameof(GetById),
                               new { id },
                               OsLogResponse<object>.Ok(dados: id,
                                                        mensagem: "Empresa criada com sucesso.")
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var lista = await _empresaService.GetAll(ct);

        if (lista.Count <= 0)
        {
            return NotFound(
                OsLogResponse<EmpresaDetailDto>.Critica(
                    codigo: CodigosOsLog.EMPRESA_NAO_ENCONTRADA,
                    mensagem: CriticasOsLog.RetornaCritica(CodigosOsLog.EMPRESA_NAO_ENCONTRADA)
                )
            );
        }

        return Ok(
            OsLogResponse<IEnumerable<EmpresaListDto>>.Ok(
                dados: lista,
                mensagem: "Empresas retornadas com sucesso.")
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var empresa = await _empresaService.GetById(id, ct);

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

        var ok = await _empresaService.Delete(id, usuarioId, ct);
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
}
