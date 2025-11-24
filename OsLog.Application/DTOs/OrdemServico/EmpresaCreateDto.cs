namespace OsLog.Application.DTOs.OrdemServico;

public class EmpresaCreateDto
{
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string? Cnpj { get; set; }
}
