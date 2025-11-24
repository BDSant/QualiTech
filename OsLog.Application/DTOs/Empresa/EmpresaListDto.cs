namespace OsLog.Application.DTOs.Empresa;

public class EmpresaListDto
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
    public bool Ativa { get; set; } // Corresponde a !FlExcluido
}
