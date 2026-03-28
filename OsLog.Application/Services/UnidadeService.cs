using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Unidade;
using OsLog.Application.Ports.ApplicationServices;
using OsLog.Domain.Entities;
using OsLog.Domain.Enums;

namespace OsLog.Application.Services;

public class UnidadeService : IUnidadeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UnidadeService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<int> Create(Guid empresaId, UnidadeCreateDto dto, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var empresa = await _unitOfWork.Empresas.GetByPredicateAsync(
                e => e.Id == empresaId && e.Ativa,
                ct);

            if (empresa is null)
            {
                await _unitOfWork.RollbackAsync(ct);
                return 0;
            }

            var unidade = _mapper.Map<Unidade>(dto);

            unidade.EmpresaId = empresaId;
            unidade.Tipo = TipoUnidade.Filial;
            unidade.Ativa = true;
            unidade.DataCriacaoUtc = DateTime.UtcNow;

            await _unitOfWork.Unidades.AddAsync(unidade, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _unitOfWork.CommitAsync(ct);

            return unidade.Id;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<UnidadeDto>> GetAllByEmpresa(Guid empresaId, CancellationToken ct)
    {
        var unidades = await _unitOfWork.Unidades.FindAsync(
            u => u.EmpresaId == empresaId && u.Ativa,
            ct);

        return _mapper.Map<IReadOnlyList<UnidadeDto>>(unidades);
    }

    public async Task<UnidadeDto?> GetById(int id, Guid empresaId, CancellationToken ct)
    {
        var unidade = await _unitOfWork.Unidades.GetByPredicateAsync(
            u => u.Id == id &&
                 u.EmpresaId == empresaId &&
                 u.Ativa,
            ct);

        return unidade is null
            ? null
            : _mapper.Map<UnidadeDto>(unidade);
    }

    public async Task<bool> Delete(int id, Guid empresaId, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var unidade = await _unitOfWork.Unidades.GetByPredicateAsync(
                u => u.Id == id &&
                     u.EmpresaId == empresaId &&
                     u.Ativa,
                ct);

            if (unidade is null)
            {
                await _unitOfWork.RollbackAsync(ct);
                return false;
            }

            unidade.Ativa = false;
            _unitOfWork.Unidades.Update(unidade);

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