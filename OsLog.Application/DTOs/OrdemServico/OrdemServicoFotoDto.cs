namespace OsLog.Application.DTOs.OrdemServico;

public class OrdemServicoFotoDto
{
    public int Id { get; set; }
    public byte[]? Foto { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataCadastro { get; set; }
}
