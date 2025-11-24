namespace OsLog.Application.DTOs.Clientes;

public class ClienteDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int UnidadeId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Documento { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }

    public bool FlExcluido { get; set; }
    public DateTime DataCriacao { get; set; }
}
