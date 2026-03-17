using System.ComponentModel.DataAnnotations;

namespace OsLog.Application.DTOs.Empresa;

public class EmpresaDetailDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Razão social é obrigatória.")]
    [StringLength(150, ErrorMessage = "Razão social pode ter no máximo {1} caracteres.")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome fantasia é obrigatório.")]
    [StringLength(120, ErrorMessage = "Nome fantasia pode ter no máximo {1} caracteres.")]
    public string NomeFantasia { get; set; } = string.Empty;

    public DateTime DataCriacaoUtc { get; set; }

    public bool Ativa { get; set; }
}
