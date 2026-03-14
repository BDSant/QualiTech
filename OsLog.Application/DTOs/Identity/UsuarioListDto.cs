namespace OsLog.Application.DTOs.Identity;

public class UsuarioListDto
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmado { get; set; }
    public bool Ativo { get; set; }
}