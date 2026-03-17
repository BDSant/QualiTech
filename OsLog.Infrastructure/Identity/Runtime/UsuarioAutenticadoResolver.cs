using OsLog.Application.Ports.Identity.Runtime;
using OsLog.Domain.Interfaces.Repositories;

namespace OsLog.Infrastructure.Identity.Runtime;

public sealed class UsuarioAutenticadoResolver : IUsuarioAutenticadoResolver
{
    private readonly IUsuarioAcessoRepository _usuarioAcessoRepository;

    public UsuarioAutenticadoResolver(IUsuarioAcessoRepository usuarioAcessoRepository)
    {
        _usuarioAcessoRepository = usuarioAcessoRepository;
    }

    public async Task<int?> ObterUsuarioIdAsync(string userId, CancellationToken ct = default)
    {
        var usuarioAcesso = await _usuarioAcessoRepository.ObterPorUserIdAsync(userId, ct);
        return usuarioAcesso?.Id;
    }
}