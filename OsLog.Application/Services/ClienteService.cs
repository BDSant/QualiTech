using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Clientes;
using OsLog.Domain.Entities;

namespace OsLog.Application.Services;

public class ClienteService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ClienteService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<int> CreateAsync(ClienteCreateDto dto, int usuarioId, CancellationToken ct)
    {
        var cliente = _mapper.Map<Cliente>(dto);
        cliente.DataCriacao = DateTime.UtcNow;
        cliente.FlExcluido = false;
        cliente.AlteradoPor = usuarioId;

        await _uow.Clientes.AddAsync(cliente, ct);
        await _uow.CommitAsync(ct);

        return cliente.Id;
    }

    public async Task<ClienteDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var cliente = await _uow.Clientes.GetByIdAsync(id, ct);
        return cliente == null ? null : _mapper.Map<ClienteDto>(cliente);
    }

    public async Task<List<ClienteDto>> ListAsync(CancellationToken ct)
    {
        var list = await _uow.Clientes.ListAsync(ct);
        return list.Where(c => !c.FlExcluido)
                   .Select(_mapper.Map<ClienteDto>)
                   .ToList();
    }

    public async Task UpdateAsync(int id, ClienteCreateDto dto, int usuarioId, CancellationToken ct)
    {
        var cliente = await _uow.Clientes.GetByIdAsync(id, ct)
                      ?? throw new InvalidOperationException("Cliente não encontrado.");

        cliente.Nome = dto.Nome;
        cliente.Documento = dto.Documento;
        cliente.Telefone = dto.Telefone;
        cliente.Email = dto.Email;
        cliente.DataAlteracao = DateTime.UtcNow;
        cliente.AlteradoPor = usuarioId;

        _uow.Clientes.Update(cliente);
        await _uow.CommitAsync(ct);
    }

    public async Task SoftDeleteAsync(int id, int usuarioId, CancellationToken ct)
    {
        var cliente = await _uow.Clientes.GetByIdAsync(id, ct)
                      ?? throw new InvalidOperationException("Cliente não encontrado.");

        if (cliente.FlExcluido) return;

        cliente.FlExcluido = true;
        cliente.DataAlteracao = DateTime.UtcNow;
        cliente.AlteradoPor = usuarioId;

        _uow.Clientes.Update(cliente);
        await _uow.CommitAsync(ct);
    }
}
