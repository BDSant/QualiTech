using System;
using System.Collections.Generic;
using System.Text;

namespace OsLog.Domain.Entities;

public class Empresa
{
    public int Id { get; set; }

    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string? Cnpj { get; set; }

    // Soft delete
    public bool FlExcluido { get; set; }

    // Auditoria
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAlteracao { get; set; }
    public int? AlteradoPor { get; set; }

    // Navegações
    public ICollection<Unidade> Unidades { get; set; } = new List<Unidade>();
}