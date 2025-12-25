using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OsLog.Api.Configuration;
using OsLog.Api.DTOs.Auth;
using OsLog.Application.DTOs.Empresa;
using OsLog.Application.Interfaces.Services;

namespace OsLog.API.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly IUsuarioAcessoService _usuarioAcessoService;

    public JwtTokenService(
        IOptions<JwtOptions> jwtOptions,
        IUsuarioAcessoService usuarioAcessoService)
    {
        _jwtOptions = jwtOptions.Value;
        _usuarioAcessoService = usuarioAcessoService;
    }

    public async Task<TokenResponseDto> GenerateTokensAsync(
        string userId,
        string? email,
        string? userName,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // 1) Claims básicas do usuário
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, userName));
            claims.Add(new Claim(ClaimTypes.Name, userName));
        }

        // 2) Roles do Identity (Admin, GerenteFinanceiro, Atendente, Tecnico, Master...)
        var roleList = roles?.ToList() ?? new List<string>();
        foreach (var role in roleList)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 3) Claims adicionais (se o controller quiser passar)
        if (additionalClaims is not null)
        {
            claims.AddRange(additionalClaims);
        }

        // 4) Master vs acesso por empresa/unidade
        var isMaster = roleList.Contains("Master", StringComparer.OrdinalIgnoreCase);

        if (isMaster)
        {
            // Usuário Master vê tudo: claim especial
            claims.Add(new Claim("master", "true"));
        }
        else
        {
            // Usuários "normais": carrega acessos por empresa/unidade
            var acessos = await _usuarioAcessoService.ObterAcessoPorUsuarioAsync(userId, ct);
            claims.AddRange(BuildAcessoClaims(acessos));
        }

        // 5) Assinatura com chave simétrica (appsettings: Jwt:SigningKey)
        var keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expires = now.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: signingCredentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(tokenDescriptor);

        // 6) Refresh token simples (pode evoluir depois, por exemplo, salvando em tabela)
        var refreshToken = Guid.NewGuid().ToString("N");

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = (int)TimeSpan.FromMinutes(_jwtOptions.AccessTokenMinutes).TotalSeconds,
            ExpiresAt = expires
        };
    }

    /// <summary>
    /// Converte a visão de acesso (EmpresaAcessoDto) em claims:
    /// - "empresa"      => EmpresaId (sempre que o usuário tem algum vínculo com a empresa)
    /// - "empresa_full" => EmpresaId com acesso total (todas as unidades da empresa)
    /// - "unidade"      => "EmpresaId:UnidadeId" para cada unidade específica
    /// </summary>
    private static IEnumerable<Claim> BuildAcessoClaims(
        IReadOnlyCollection<EmpresaAcessoDto> acessos)
    {
        var claims = new List<Claim>();

        foreach (var empresa in acessos)
        {
            // Claim genérica de empresa (o usuário tem algum vínculo com essa empresa)
            claims.Add(new Claim("empresa", empresa.EmpresaId.ToString()));

            // Empresa com acesso total (todas as unidades)
            if (empresa.AcessoTotalEmpresa)
            {
                claims.Add(new Claim("empresa_full", empresa.EmpresaId.ToString()));
            }

            // Vínculos específicos de unidade
            foreach (var unidade in empresa.Unidades)
            {
                claims.Add(new Claim("unidade", $"{empresa.EmpresaId}:{unidade.UnidadeId}"));
            }
        }

        return claims;
    }
}
