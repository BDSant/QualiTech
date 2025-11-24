namespace OsLog.Domain.Entities;

public class OrdemServicoAcessorio
{
    public int Id { get; set; }                 // PK
    public int OrdemServicoId { get; set; }     // FK para OS

    public string Descricao { get; set; } = string.Empty;   // Ex: "Carregador", "Capa", etc.
    public bool Devolvido { get; set; } = false;            // se já foi devolvido ao cliente

    public bool FlExcluido { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    // Navegação
    public OrdemServico OrdemServico { get; set; } = null!;
}
