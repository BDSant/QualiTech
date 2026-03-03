using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Tecnicos;
using OsLog.Application.Ports.ApplicationServices;
using OsLog.Domain.Entities;

namespace OsLog.Application.Services;

public class TecnicoService : ITecnicoService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public TecnicoService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<int> CreateAsync(TecnicoCreateDto dto, int usuarioId, CancellationToken ct)
    {
        var tecnico = _mapper.Map<Tecnico>(dto);
        tecnico.DataCriacao = DateTime.UtcNow;
        tecnico.Ativo = true;
        tecnico.FlExcluido = false;
        tecnico.AlteradoPor = usuarioId;

        await _uow.Tecnicos.AddAsync(tecnico, ct);
        await _uow.CommitAsync(ct);

        return tecnico.Id;
    }

    public async Task<TecnicoDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var t = await _uow.Tecnicos.GetById(id, ct);
        return t == null ? null : _mapper.Map<TecnicoDto>(t);
    }

    public async Task<List<TecnicoDto>> ListAsync(CancellationToken ct)
    {
        var list = await _uow.Tecnicos.GetAll(ct);
        return list.Where(t => !t.FlExcluido)
                   .Select(_mapper.Map<TecnicoDto>)
                   .ToList();
    }

    public async Task InativarAsync(int id, int usuarioId, CancellationToken ct)
    {
        var t = await _uow.Tecnicos.GetById(id, ct)
                ?? throw new InvalidOperationException("Técnico não encontrado.");

        if (!t.Ativo && t.FlExcluido) return;

        t.Ativo = false;
        t.FlExcluido = true;
        t.DataAlteracao = DateTime.UtcNow;
        t.AlteradoPor = usuarioId;

        _uow.Tecnicos.Update(t);
        await _uow.CommitAsync(ct);
    }
}
