using Microsoft.AspNetCore.Mvc;
using OsLog.Application.Common;
using OsLog.Application.DTOs.OrdemServico;
using OsLog.Application.Services;

namespace OsLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdemServicoController : ControllerBase
{
    private readonly OrdemServicoService _service;
    private readonly IUnitOfWork _UnitOfWork;

    public OrdemServicoController(OrdemServicoService service, IUnitOfWork uow)
    {
        _service = service;
        _UnitOfWork = uow;
    }

    public record AtualizarStatusRequest(string NovoStatus);

    public record AdicionarItemRequest(string TipoItem, string DescricaoItem, decimal Quantidade, decimal ValorUnitario);

    public record AdicionarAcessorioRequest(string Descricao);

    public record AtualizarDevolucaoAcessorioRequest(bool Devolvido);

    public record AdicionarFotoRequest(string CaminhoArquivo, string? Descricao);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrdemServicoListDto>>> GetAll(CancellationToken ct)
    {
        var lista = await _service.ListarAsync(ct);
        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrdemServicoDetailDto>> GetById(int id, CancellationToken ct)
    {
        var os = await _service.ObterPorIdAsync(id, ct);
        if (os is null)
            return NotFound();

        return Ok(os);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] OrdemServicoCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var usuarioId = 1; // TODO: pegar usuário logado

        var id = await _service.AbrirOsAsync(dto, usuarioId, ct);
        await _UnitOfWork.CommitAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] AtualizarStatusRequest dto, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: pegar usuário logado

        await _service.AtualizarStatusAsync(id, dto.NovoStatus, usuarioId, ct);
        await _UnitOfWork.CommitAsync(ct);

        return NoContent();
    }

    [HttpPost("{id:int}/sinal/confirmar")]
    public async Task<IActionResult> ConfirmarSinal(int id, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: pegar usuário logado

        await _service.ConfirmarPagamentoSinalAsync(id, usuarioId, ct);
        await _UnitOfWork.CommitAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: pegar usuário logado

        await _service.SoftDeleteAsync(id, usuarioId, ct);
        await _UnitOfWork.CommitAsync(ct);

        return NoContent();
    }

    [HttpGet("{id:int}/itens")]
    public async Task<ActionResult<IEnumerable<OrcamentoItemDto>>> ListarItens(int id, CancellationToken ct)
    {
        var itens = await _service.ListarItensAsync(id, ct);
        return Ok(itens);
    }

    [HttpPost("{id:int}/itens")]
    public async Task<IActionResult> AdicionarItem(int id, [FromBody] AdicionarItemRequest dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var usuarioId = 1; // TODO: usuário logado

        await _service.AdicionarItemAsync(
            id,
            dto.TipoItem,
            dto.DescricaoItem,
            dto.Quantidade,
            dto.ValorUnitario,
            usuarioId,
            ct);

        await _UnitOfWork.CommitAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}/itens/{itemId:int}")]
    public async Task<IActionResult> RemoverItem(int id, int itemId, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: usuário logado

        await _service.RemoverItemAsync(id, itemId, usuarioId, ct);
        await _UnitOfWork.CommitAsync(ct);

        return NoContent();
    }

    [HttpGet("{id:int}/acessorios")]
    public async Task<ActionResult<IEnumerable<OrdemServicoAcessorioDto>>> ListarAcessorios(int id, CancellationToken ct)
    {
        var acessorios = await _service.ListarAcessoriosAsync(id, ct);
        return Ok(acessorios);
    }

    [HttpPost("{id:int}/acessorios")]
    public async Task<IActionResult> AdicionarAcessorio(int id, [FromBody] AdicionarAcessorioRequest dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var usuarioId = 1; // TODO: usuário logado

        await _service.AdicionarAcessorioAsync(id, dto.Descricao, usuarioId, ct);
        await _UnitOfWork.CommitAsync(ct);

        return NoContent();
    }

    [HttpPatch("{id:int}/acessorios/{acessorioId:int}/devolucao")]
    public async Task<IActionResult> AtualizarDevolucaoAcessorio(int id, int acessorioId, [FromBody] AtualizarDevolucaoAcessorioRequest dto, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: usuário logado

        await _service.AtualizarDevolucaoAcessorioAsync(
            id,
            acessorioId,
            dto.Devolvido,
            usuarioId,
            ct);

        await _UnitOfWork.CommitAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}/acessorios/{acessorioId:int}")]
    public async Task<IActionResult> RemoverAcessorio(int id, int acessorioId, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: usuário logado

        await _service.RemoverAcessorioAsync(id, acessorioId, usuarioId, ct);
        await _UnitOfWork.CommitAsync(ct);

        return NoContent();
    }

    [HttpGet("{id:int}/fotos")]
    public async Task<ActionResult<IEnumerable<OrdemServicoFotoDto>>> ListarFotos(int id, CancellationToken ct)
    {
        var fotos = await _service.ListarFotosAsync(id, ct);
        return Ok(fotos);
    }

    [HttpPost("{id:int}/fotos")]
    public async Task<IActionResult> AdicionarFoto(int id, [FromBody] AdicionarFotoRequest dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var usuarioId = 1; // TODO: usuário logado

        await _service.AdicionarFotoAsync(id, dto.CaminhoArquivo, dto.Descricao, usuarioId, ct);
        await _UnitOfWork.CommitAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}/fotos/{fotoId:int}")]
    public async Task<IActionResult> RemoverFoto(int id, int fotoId, CancellationToken ct)
    {
        var usuarioId = 1; // TODO: usuário logado

        await _service.RemoverFotoAsync(id, fotoId, usuarioId, ct);
        await _UnitOfWork.CommitAsync(ct);

        return NoContent();
    }
}


