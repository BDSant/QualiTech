namespace OsLog.Application.DTOs.Empresa;

public class EmpresaListDto
{
    public Guid Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public bool Ativa { get; set; }
}
