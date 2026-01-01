using System.ComponentModel.DataAnnotations;

namespace OsLog.API.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    [MaxLength(100)]
    public string Senha { get; set; } = null!;
}


public class ClaimDto
{
    public string Tipo { get; set; } = string.Empty;    // ex: "nome", "tipo_usuario"
    public string Valor { get; set; } = string.Empty;   // ex: "Administrador", "interno"
}

public class CreateUserRequestDto
{
    /// <summary>E-mail do usuário (será usado também como UserName)</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Senha inicial do usuário</summary>
    public string Senha { get; set; } = string.Empty;

    /// <summary>Lista de roles a atribuir (ex: ["Admin","GerenteFinanceiro"])</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>Claims adicionais (ex: [{ tipo: "nome", valor: "João" }])</summary>
    public List<ClaimDto> Claims { get; set; } = new();
}
