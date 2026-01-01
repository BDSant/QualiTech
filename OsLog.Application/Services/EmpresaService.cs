using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.ApplicationServices;
using OsLog.Domain.Entities;

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

    public async Task<int> CriarEmpresaAsync(EmpresaCreateDto dto, int usuarioId, CancellationToken ct)
    {
        var empresa = new Empresa
        {
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            Cnpj = dto.Cnpj,
            DataCriacao = DateTime.UtcNow,
            AlteradoPor = usuarioId,
            FlExcluido = false
        };

        await _UnitOfWork.Empresas.AddAsync(empresa, ct);

        var unidadeMatriz = new Unidade
        {
            Empresa = empresa,
            Nome = "Matriz",
            Cnpj = dto.Cnpj,
            DataCriacao = DateTime.UtcNow,
            AlteradoPor = usuarioId,
            FlExcluido = false
        };

        await _UnitOfWork.Unidades.AddAsync(unidadeMatriz, ct);

        await _UnitOfWork.CommitAsync(ct);

        return empresa.Id;
    }

    public async Task<IReadOnlyList<EmpresaListDto>> ListarAsync(CancellationToken ct)
    {
        var empresas = await _UnitOfWork.Empresas.ListAsync(e => !e.FlExcluido, ct);
        return _mapper.Map<List<EmpresaListDto>>(empresas);
    }

    public async Task<EmpresaDetailDto?> ObterPorIdAsync(int id, CancellationToken ct)
    {
        var empresa = await _UnitOfWork.Empresas.GetByIdAsync(id, ct);
        if (empresa is null || empresa.FlExcluido)
            return null;

        return _mapper.Map<EmpresaDetailDto>(empresa);
    }

    public async Task<bool> SoftDeleteAsync(int id, int usuarioId, CancellationToken ct)
    {
        var empresa = await _UnitOfWork.Empresas.GetByIdAsync(id, ct);
        if (empresa is null || empresa.FlExcluido)
            return false;

        empresa.FlExcluido = true;
        empresa.DataAlteracao = DateTime.UtcNow;
        empresa.AlteradoPor = usuarioId;

        _UnitOfWork.Empresas.Update(empresa);
        await _UnitOfWork.CommitAsync(ct);

        return true;
    }
}
