namespace OsLog.Application.DTOs.Clientes;

public class ClienteCreateDto
{
    public int EmpresaId { get; set; }
    public int UnidadeId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
}
