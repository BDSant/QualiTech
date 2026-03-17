using OsLog.Domain.Enums;

namespace OsLog.Application.DTOs.Unidade;

public class UnidadeDto
{
    public int Id { get; set; }
    public Guid EmpresaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public TipoUnidade Tipo { get; set; }
    public bool Ativa { get; set; }
}
