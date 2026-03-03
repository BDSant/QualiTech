using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
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

    public async Task<int> Create(int empresaId, UnidadeCreateDto unidadeDto, int usuarioId, CancellationToken ct)
    {
        var empresa = await _UnitOfWork.Empresas.GetById(empresaId, ct);
        if (empresa is null || empresa.FlExcluido)
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
            DataCriacao = DateTime.UtcNow,
            AlteradoPor = usuarioId,
            FlExcluido = false
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

    public async Task<IReadOnlyList<UnidadeDto>> GetById(int empresaId, CancellationToken ct)
    {
        var unidades = await _UnitOfWork.Unidades.GetById(u => u.EmpresaId == empresaId &&
                                                          !u.FlExcluido, ct);
        return _mapper.Map<List<UnidadeDto>>(unidades);
    }

    public async Task<bool> Delete(int unidadeId, int usuarioId, CancellationToken ct)
    {
        var unidade = await _UnitOfWork.Unidades.GetById(unidadeId, ct);
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
