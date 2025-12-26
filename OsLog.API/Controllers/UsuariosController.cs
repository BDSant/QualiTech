using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OsLog.Api.DTOs.Auth;
using OsLog.Infrastructure.Identity;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsuariosController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Cria um usuário com as roles e claims informadas.
    /// Exemplo de body:
    /// {
    ///   "email": "admin2@oslog.local",
    ///   "senha": "Admin@123",
    ///   "roles": [ "Admin", "GerenteFinanceiro" ],
    ///   "claims": [
    ///     { "tipo": "nome", "valor": "Admin 2" },
    ///     { "tipo": "tipo_usuario", "valor": "interno" }
    ///   ]
    /// }
    /// </summary>
    [HttpPost("criar")]
    [AllowAnonymous]
    //[Authorize(Roles = "Master")] // Só Master pode criar usuários; remova se não quiser essa regra
    public async Task<IActionResult> CriarUsuario([FromBody] CreateUserRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1) Verifica se já existe usuário com esse e-mail
        var existente = await _userManager.FindByEmailAsync(dto.Email);
        if (existente != null)
        {
            return BadRequest(new
            {
                mensagem = "Já existe um usuário com este e-mail.",
                email = dto.Email
            });
        }

        // 2) Cria o usuário
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, dto.Senha);
        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                mensagem = "Erro ao criar usuário.",
                erros = createResult.Errors.Select(e => e.Description)
            });
        }

        // 3) Garante que as roles existem e atribui ao usuário
        if (dto.Roles is not null && dto.Roles.Count > 0)
        {
            foreach (var roleName in dto.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!roleResult.Succeeded)
                    {
                        return BadRequest(new
                        {
                            mensagem = $"Erro ao criar role '{roleName}'.",
                            erros = roleResult.Errors.Select(e => e.Description)
                        });
                    }
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(user, roleName);
                if (!addToRoleResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        mensagem = $"Erro ao atribuir role '{roleName}' ao usuário.",
                        erros = addToRoleResult.Errors.Select(e => e.Description)
                    });
                }
            }
        }

        // 4) Adiciona claims
        if (dto.Claims is not null && dto.Claims.Count > 0)
        {
            var claims = dto.Claims
                .Where(c => !string.IsNullOrWhiteSpace(c.Tipo))
                .Select(c => new Claim(c.Tipo, c.Valor ?? string.Empty))
                .ToList();

            if (claims.Count > 0)
            {
                var addClaimsResult = await _userManager.AddClaimsAsync(user, claims);
                if (!addClaimsResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        mensagem = "Erro ao atribuir claims ao usuário.",
                        erros = addClaimsResult.Errors.Select(e => e.Description)
                    });
                }
            }
        }

        // 5) Retorna um resumo do usuário criado
        var userRoles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        return Ok(new
        {
            mensagem = "Usuário criado com sucesso.",
            usuario = new
            {
                user.Id,
                user.Email,
                Roles = userRoles,
                Claims = userClaims.Select(c => new { c.Type, c.Value })
            }
        });
    }
}
