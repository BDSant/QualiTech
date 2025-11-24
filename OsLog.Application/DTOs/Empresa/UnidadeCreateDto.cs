namespace OsLog.Application.DTOs.Empresa;

public class UnidadeCreateDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
}
