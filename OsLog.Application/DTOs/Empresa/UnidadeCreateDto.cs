using System.ComponentModel.DataAnnotations;

namespace OsLog.Application.DTOs.Unidade;

public class UnidadeCreateDto
{
    [Required(ErrorMessage = "Nome é obrigatória.")]
    [StringLength(150, ErrorMessage = "Razão social pode ter no máximo {1} caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "CNPJ é obrigatório.")]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "CNPJ deve conter 14 dígitos numéricos (apenas números).")]
    public string Cnpj { get; set; } = string.Empty;

    public string? InscricaoEstadual { get; set; }
    public string? InscricaoMunicipal { get; set; }
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
}
