using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.ApplicationServices;
using OsLog.Domain.Entities;
using OsLog.Domain.Enums;

namespace OsLog.Application.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IUnitOfWork _UnitOfWork;
    private readonly IMapper _mapper;

    public EmpresaService(IUnitOfWork uow, IMapper mapper)
    {
        _UnitOfWork = uow;
        _mapper = mapper;
    }

    public async Task<Guid> Create(EmpresaCreateDto dto, string usuarioId, CancellationToken ct)
    {
        var dtaNow = DateTime.UtcNow;

        await _UnitOfWork.BeginTransactionAsync(ct);

        var empresa = new Empresa
        {
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            DataCriacaoUtc = dtaNow,
            Ativa = true
        };

        await _UnitOfWork.Empresas.AddAsync(empresa, ct);
        await _UnitOfWork.SaveChangesAsync();


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
            DataCriacaoUtc = dtaNow,
        };

        await _UnitOfWork.Unidades.AddAsync(unidadeMatriz, ct);


        var usuarioAcesso = new UsuarioAcesso
        {
            UsuarioId = usuarioId,
            EmpresaId = empresa.Id,
            UnidadeId = null,
            Escopo = EscopoAcesso.Empresa,
            Perfil = PerfilAcesso.Proprietario,
            Ativo = true,
            DataCriacaoUtc = dtaNow
        };

        await _UnitOfWork.UsuarioAcessos.AddAsync(usuarioAcesso, ct);
        await _UnitOfWork.CommitAsync(ct);

        return empresa.Id;
    }

    public async Task<IReadOnlyList<EmpresaListDto>> GetAll(CancellationToken ct)
    {
        var empresas = await _UnitOfWork.Empresas.GetById(e => !e.Ativa, ct);
        return _mapper.Map<IReadOnlyList<EmpresaListDto>>(empresas);
    }

    public async Task<EmpresaDetailDto?> GetById(Guid id, CancellationToken ct)
    {
        var empresa = await _UnitOfWork.Empresas.GetById(id, ct);
        return empresa is null ? null : _mapper.Map<EmpresaDetailDto>(empresa);
    }

    public async Task<bool> Delete(Guid id, string usuarioId, CancellationToken ct)
    {
        var empresa = await _UnitOfWork.Empresas.GetById(id, ct);
        if (empresa is null)
            return false;

        empresa.Ativa = false;

        _UnitOfWork.Empresas.Update(empresa);

        await _UnitOfWork.CommitAsync(ct);

        return true;
    }
}
