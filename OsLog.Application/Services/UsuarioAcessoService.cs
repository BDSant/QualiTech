using AutoMapper;
using OsLog.Application.Common;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Interfaces.Services;
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
        // Carrega os vínculos + empresa/unidade
        var acessos = await _UnitOfWork.UsuarioAcessos.ObterAcessosPorUsuarioAsync(userId, ct);

        // Agrupa por empresa
        var resultado = acessos
            .GroupBy(x => x.Empresa)
            .Select(grp =>
            {
                var empresa = grp.Key;

                bool acessoTotal = grp.Any(x => x.UnidadeId == null);

                // Se acesso total, você pode opcionalmente listar TODAS unidades da empresa
                // mesmo que não haja vínculo individual por unidade.
                var acessosPorUnidade = grp
                    .Where(x => x.UnidadeId != null && x.Unidade != null)
                    .ToList();

                var unidadesAcesso = _mapper
                     .Map<List<UnidadeAcessoDto>>(acessosPorUnidade);

                return new EmpresaAcessoDto
                {
                    EmpresaId = empresa.Id,
                    RazaoSocial = empresa.RazaoSocial,
                    NomeFantasia = empresa.NomeFantasia,
                    AcessoTotalEmpresa = acessoTotal,
                    Unidades = unidadesAcesso
                };
            })
            .ToList();

        return resultado;
    }
}
