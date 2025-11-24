using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Interfaces.Services;
using OsLog.Domain.Entities;

namespace OsLog.Application.Services;

public class UnidadeService : IUnidadeService
{
    private readonly IUnitOfWork _UnitOfWork;
    private readonly IMapper _mapper;

    public UnidadeService(IUnitOfWork uow, IMapper mapper)
    {
        _UnitOfWork = uow;
        _mapper = mapper;
    }

    public async Task<int> CriarUnidadeAsync(int empresaId, UnidadeCreateDto dto, int usuarioId, CancellationToken ct)
    {
        var empresa = await _UnitOfWork.Empresas.GetByIdAsync(empresaId, ct);
        if (empresa is null || empresa.FlExcluido)
            throw new InvalidOperationException("Empresa não encontrada ou inativa.");

        var unidade = new Unidade
        {
            EmpresaId = empresaId,
            Nome = dto.Nome,
            Cnpj = dto.Cnpj,
            InscricaoEstadual = dto.InscricaoEstadual,
            InscricaoMunicipal = dto.InscricaoMunicipal,
            Endereco = dto.Endereco,
            Telefone = dto.Telefone,
            DataCriacao = DateTime.UtcNow,
            AlteradoPor = usuarioId,
            FlExcluido = false
        };

        await _UnitOfWork.Unidades.AddAsync(unidade, ct);
        await _UnitOfWork.CommitAsync(ct);

        return unidade.Id;
    }

    public async Task<IReadOnlyList<UnidadeDto>> ListarTodasAsync(CancellationToken ct)
    {
        var unidades = await _UnitOfWork.Unidades.ListAsync(ct);
        return _mapper.Map<List<UnidadeDto>>(unidades);
    }

    public async Task<IReadOnlyList<UnidadeDto>> ListarPorEmpresaAsync(int empresaId, CancellationToken ct)
    {
        var unidades = await _UnitOfWork.Unidades.ListAsync(u => u.EmpresaId == empresaId && !u.FlExcluido, ct);
        return _mapper.Map<List<UnidadeDto>>(unidades);
    }

    public async Task<bool> SoftDeleteAsync(int id, int usuarioId, CancellationToken ct)
    {
        var unidade = await _UnitOfWork.Unidades.GetByIdAsync(id, ct);
        if (unidade is null || unidade.FlExcluido)
            return false;

        unidade.FlExcluido = true;
        unidade.DataAlteracao = DateTime.UtcNow;
        unidade.AlteradoPor = usuarioId;

        _UnitOfWork.Unidades.Update(unidade);
        await _UnitOfWork.CommitAsync(ct);

        return true;
    }


}
