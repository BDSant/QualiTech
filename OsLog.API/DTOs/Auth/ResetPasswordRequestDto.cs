using System.ComponentModel.DataAnnotations;

namespace OsLog.API.DTOs.Auth;

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
    [MaxLength(100)]
    public string NovaSenha { get; set; } = string.Empty;
}
