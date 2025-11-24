namespace OsLog.Domain.Entities;

public class OrdemServicoFoto
{
    public int Id { get; set; }                 // PK
    public int OrdemServicoId { get; set; }     // FK para OS
    public string? Descricao { get; set; }                      // ex: "Antes do reparo", "Depois"
    public byte[] Foto { get; set; } = Array.Empty<byte>();
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool FlExcluido { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    public OrdemServico OrdemServico { get; set; } = null!;
}
