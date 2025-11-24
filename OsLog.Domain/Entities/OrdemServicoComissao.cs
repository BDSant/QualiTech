namespace OsLog.Domain.Entities;

public class OrdemServicoComissao
{
    public int Id { get; set; }                 // PK

    public int OrdemServicoId { get; set; }     // FK para OS
    public int TecnicoId { get; set; }          // FK para Técnico

    // Tipo de base: "VALOR_OS", "VALOR_SINAL_RETIDO", etc.
    public string TipoBase { get; set; } = "VALOR_OS";

    public decimal Percentual { get; set; }     // ex: 10 = 10%
    public decimal ValorBase { get; set; }      // ex: valor da OS ou valor do sinal
    public decimal ValorComissao { get; set; }  // Percentual calculado sobre a base
    public bool Liquidado { get; set; } = false;    // se já foi pago ao técnico
    public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
    public DateTime? DataLiquidacao { get; set; }
    public bool FlExcluido { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    public OrdemServico OrdemServico { get; set; } = null!;
    public Tecnico Tecnico { get; set; } = null!;
}
