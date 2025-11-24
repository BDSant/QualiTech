namespace OsLog.Application.DTOs.Tecnicos;

public class TecnicoCreateDto
{
    public int EmpresaId { get; set; }
    public int UnidadeId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? Apelido { get; set; }
}

