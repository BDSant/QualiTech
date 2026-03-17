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

    public async Task<Guid> Create(EmpresaCreateDto dto, int usuarioId, CancellationToken ct)
    {
        var empresa = new Empresa
        {
            RazaoSocial = dto.RazaoSocial,
            NomeFantasia = dto.NomeFantasia,
            DataCriacaoUtc = DateTime.UtcNow,
            Ativa = false
        };

        await _UnitOfWork.Empresas.AddAsync(empresa, ct);

        var unidadeMatriz = new Unidade
        {
            Empresa = empresa,
            Nome = "Matriz",
            DataCriacaoUtc = DateTime.UtcNow,
            Ativa = true
        };

        await _UnitOfWork.Unidades.AddAsync(unidadeMatriz, ct);

        await _UnitOfWork.CommitAsync(ct);

        return empresa.Id;
    }

    public async Task<IReadOnlyList<EmpresaListDto>> GetAll(CancellationToken ct)
    {
        var empresas = await _UnitOfWork.Empresas.GetById(e => !e.Ativa, ct);
        return _mapper.Map<List<EmpresaListDto>>(empresas);
    }

    public async Task<EmpresaDetailDto?> GetById(Guid id, CancellationToken ct)
    {
        var empresa = await _UnitOfWork.Empresas.GetById(id, ct);
        if (empresa is null || empresa.Ativa)
            return null;

        return _mapper.Map<EmpresaDetailDto>(empresa);
    }

    public async Task<bool> Delete(Guid id, int usuarioId, CancellationToken ct)
    {
        var empresa = await _UnitOfWork.Empresas.GetById(id, ct);
        if (empresa is null || empresa.Ativa)
            return false;

        empresa.Ativa = true;
        empresa.DataCriacaoUtc = DateTime.UtcNow;

        _UnitOfWork.Empresas.Update(empresa);
        await _UnitOfWork.CommitAsync(ct);

        return true;
    }
}
