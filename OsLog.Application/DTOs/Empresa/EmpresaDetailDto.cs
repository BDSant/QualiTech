namespace OsLog.Application.DTOs.Empresa;

public class EmpresaDetailDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool Ativa { get; set; } // Corresponde a !FlExcluido
}
