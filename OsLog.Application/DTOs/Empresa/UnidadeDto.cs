namespace OsLog.Application.DTOs.Empresa;

public class UnidadeDto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public bool Ativa { get; set; } // Corresponde a !FlExcluido
}
