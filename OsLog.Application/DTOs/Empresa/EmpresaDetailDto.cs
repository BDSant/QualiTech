using System.ComponentModel.DataAnnotations;

namespace OsLog.Application.DTOs.Empresa;

public class EmpresaDetailDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Razão social é obrigatória.")]
    [StringLength(150, ErrorMessage = "Razão social pode ter no máximo {1} caracteres.")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome fantasia é obrigatório.")]
    [StringLength(120, ErrorMessage = "Nome fantasia pode ter no máximo {1} caracteres.")]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required(ErrorMessage = "CNPJ é obrigatório.")]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "CNPJ deve conter 14 dígitos numéricos (apenas números).")]
    public string? Cnpj { get; set; }

    public DateTime DataCriacao { get; set; }

    public bool Ativa { get; set; } // Corresponde a !FlExcluido
}
