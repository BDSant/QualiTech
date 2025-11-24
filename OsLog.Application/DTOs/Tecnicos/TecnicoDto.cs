namespace OsLog.Application.DTOs.Tecnicos;

public class TecnicoDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int UnidadeId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
    public bool Ativo { get; set; }

    public bool FlExcluido { get; set; }
    public DateTime DataCriacao { get; set; }
}
