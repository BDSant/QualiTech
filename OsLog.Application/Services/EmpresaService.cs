using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.ApplicationServices;
using OsLog.Domain.Entities;
using OsLog.Domain.Enums;

namespace OsLog.Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EmpresaService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> Create(EmpresaCreateDto dto, string usuarioId, CancellationToken ct)
    {
        var agora = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var empresa = new Empresa
            {
                RazaoSocial = dto.RazaoSocial,
                NomeFantasia = dto.NomeFantasia,
                Ativa = true,
                DataCriacaoUtc = agora
            };

            await _unitOfWork.Empresas.AddAsync(empresa, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var unidadeMatriz = new Unidade
            {
                EmpresaId = empresa.Id,
                Nome = "Matriz",
                Cnpj = dto.CnpjMatriz,
                InscricaoEstadual = dto.InscricaoEstadualMatriz,
                InscricaoMunicipal = dto.InscricaoMunicipalMatriz,
                Endereco = dto.EnderecoMatriz,
                Telefone = dto.TelefoneMatriz,
                Tipo = TipoUnidade.Matriz,
                Ativa = true,
                DataCriacaoUtc = agora
            };

            await _unitOfWork.Unidades.AddAsync(unidadeMatriz, ct);

            var usuarioAcesso = new UsuarioAcesso
            {
                UsuarioId = usuarioId,
                EmpresaId = empresa.Id,
                UnidadeId = null,
                Escopo = EscopoAcesso.Empresa,
                Perfil = PerfilAcesso.Proprietario,
                Ativo = true,
                DataCriacaoUtc = agora
            };

            await _unitOfWork.UsuarioAcessos.AddAsync(usuarioAcesso, ct);

            await _unitOfWork.CommitAsync(ct);

            return empresa.Id;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<EmpresaListDto>> GetAll(string usuarioId, CancellationToken ct)
    {
        var acessos = await _unitOfWork.UsuarioAcessos.ObterListaPorUserIdAsync(usuarioId, ct);

        if (acessos.Count == 0)
            return Array.Empty<EmpresaListDto>();

        if (acessos.Any(a => a.Escopo == EscopoAcesso.Plataforma))
        {
            var todas = await _unitOfWork.Empresas.FindAsync(e => e.Ativa, ct);
            return _mapper.Map<IReadOnlyList<EmpresaListDto>>(todas);
        }

        var empresasIds = acessos
            .Where(a => a.EmpresaId.HasValue)
            .Select(a => a.EmpresaId!.Value)
            .Distinct()
            .ToList();

        var empresas = await _unitOfWork.Empresas.FindAsync(
            e => e.Ativa && empresasIds.Contains(e.Id),
            ct);

        return _mapper.Map<IReadOnlyList<EmpresaListDto>>(empresas);
    }

    public async Task<EmpresaDetailDto?> GetById(Guid id, string usuarioId, CancellationToken ct)
    {
        var acessos = await _unitOfWork.UsuarioAcessos.ObterListaPorUserIdAsync(usuarioId, ct);

        if (acessos.Count == 0)
            return null;

        var acessoPlataforma = acessos.Any(a => a.Escopo == EscopoAcesso.Plataforma);

        var acessoEmpresa = acessos.Any(a =>
            a.Escopo == EscopoAcesso.Empresa &&
            a.EmpresaId == id);

        var acessoUnidade = acessos.Any(a =>
            a.Escopo == EscopoAcesso.Unidade &&
            a.EmpresaId == id &&
            a.UnidadeId.HasValue);

        if (!acessoPlataforma && !acessoEmpresa && !acessoUnidade)
            return null;

        var empresa = await _unitOfWork.Empresas.GetByPredicateAsync(
            e => e.Id == id && e.Ativa,
            ct);

        return empresa is null
            ? null
            : _mapper.Map<EmpresaDetailDto>(empresa);
    }

    public async Task<bool> Delete(Guid id, string usuarioId, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var acessos = await _unitOfWork.UsuarioAcessos.ObterListaPorUserIdAsync(usuarioId, ct);

            var podeExcluir =
                acessos.Any(a => a.Escopo == EscopoAcesso.Plataforma) ||
                acessos.Any(a => a.Escopo == EscopoAcesso.Empresa && a.EmpresaId == id);

            if (!podeExcluir)
            {
                await _unitOfWork.RollbackAsync(ct);
                return false;
            }

            var empresa = await _unitOfWork.Empresas.GetByPredicateAsync(
                e => e.Id == id && e.Ativa,
                ct);

            if (empresa is null)
            {
                await _unitOfWork.RollbackAsync(ct);
                return false;
            }

            empresa.Ativa = false;
            _unitOfWork.Empresas.Update(empresa);

            await _unitOfWork.CommitAsync(ct);

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }
}