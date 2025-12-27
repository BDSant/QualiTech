using System.ComponentModel.DataAnnotations;

namespace OsLog.Application.UseCases.Autenticacao.Login;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    [MaxLength(100)]
    public string Senha { get; set; } = string.Empty;
}
