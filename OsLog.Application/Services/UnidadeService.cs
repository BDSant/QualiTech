using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Unidade;
using OsLog.Application.Ports.ApplicationServices;
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

    public async Task<int> Create(Guid empresaId, UnidadeCreateDto unidadeDto, int usuarioId, CancellationToken ct)
    {
        var empresa = await _UnitOfWork.Empresas.GetById(empresaId, ct);
        if (empresa is null || empresa.Ativa)
            throw new InvalidOperationException("Empresa não encontrada ou inativa.");

        var unidade = new Unidade
        {
            EmpresaId = empresaId,
            Nome = unidadeDto.Nome,
            Cnpj = unidadeDto.Cnpj,
            InscricaoEstadual = unidadeDto.InscricaoEstadual,
            InscricaoMunicipal = unidadeDto.InscricaoMunicipal,
            Endereco = unidadeDto.Endereco,
            Telefone = unidadeDto.Telefone,
            DataCriacaoUtc = DateTime.UtcNow,
            Ativa = true
        };

        await _UnitOfWork.Unidades.AddAsync(unidade, ct);
        await _UnitOfWork.CommitAsync(ct);

        return unidade.Id;
    }

    public async Task<IReadOnlyList<UnidadeDto>> GetAll(CancellationToken ct)
    {
        var unidades = await _UnitOfWork.Unidades.GetAll(ct);
        return _mapper.Map<List<UnidadeDto>>(unidades);
    }

    public async Task<IReadOnlyList<UnidadeDto>> GetById(Guid empresaId, CancellationToken ct)
    {
        var unidades = await _UnitOfWork.Unidades.GetById(u => u.EmpresaId == empresaId &&
                                                          !u.Ativa, ct);
        return _mapper.Map<List<UnidadeDto>>(unidades);
    }

    public async Task<bool> Delete(Guid empresaId, int unidadeId, int usuarioId, CancellationToken ct)
    {
        var unidades = await _UnitOfWork.Unidades.GetById(u => u.Id == unidadeId && u.EmpresaId == empresaId, ct);

        var unidade = unidades.FirstOrDefault();

        if (unidade is null || unidade.Ativa)
            return false;

        unidade.Ativa = false;

        _UnitOfWork.Unidades.Update(unidade);
        await _UnitOfWork.CommitAsync(ct);

        return true;
    }



}
