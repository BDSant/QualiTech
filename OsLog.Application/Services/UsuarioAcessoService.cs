using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Ports.ApplicationServices;
using OsLog.Domain.Entities;

namespace OsLog.Application.Services;

public class UsuarioAcessoService : IUsuarioAcessoService
{
    private readonly IUnitOfWork _UnitOfWork;
    private readonly IMapper _mapper;

    public UsuarioAcessoService(IUnitOfWork uow, IMapper mapper)
    {
        _UnitOfWork = uow;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<EmpresaAcessoDto>> ObterAcessoPorUsuarioAsync(
        string userId,
        CancellationToken ct = default)
    {
        var acessos = await _UnitOfWork.UsuarioAcessos.ObterListaPorUserIdAsync(userId, ct);

        if (acessos is null || !acessos.Any())
            return Array.Empty<EmpresaAcessoDto>();

        var resultado = acessos
            .Where(x => x.Empresa is not null)
            .GroupBy(x => new
            {
                x.EmpresaId,
                x.Empresa!.RazaoSocial,
                x.Empresa.NomeFantasia
            })
            .Select(grp =>
            {
                var acessoTotal = grp.Any(x => x.UnidadeId == null);

                var acessosPorUnidade = grp
                    .Where(x => x.UnidadeId.HasValue && x.Unidade is not null)
                    .GroupBy(x => x.UnidadeId)
                    .Select(x => x.First())
                    .ToList();

                var unidadesAcesso = _mapper.Map<List<UnidadeAcessoDto>>(acessosPorUnidade);

                return new EmpresaAcessoDto
                {
                    EmpresaId = grp.Key.EmpresaId,
                    RazaoSocial = grp.Key.RazaoSocial,
                    NomeFantasia = grp.Key.NomeFantasia,
                    AcessoTotalEmpresa = acessoTotal,
                    Unidades = acessoTotal ? new List<UnidadeAcessoDto>() : unidadesAcesso
                };
            })
            .ToList();

        return resultado;
    }
}
