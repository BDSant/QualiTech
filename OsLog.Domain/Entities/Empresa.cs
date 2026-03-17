namespace OsLog.Domain.Entities;

public class Empresa
{
    public Guid Id { get; set; }

    public string RazaoSocial { get; set; } = string.Empty;

    public string NomeFantasia { get; set; } = string.Empty;

    public bool Ativa { get; set; } = true;

    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Unidade> Unidades { get; set; } = new List<Unidade>();
    public ICollection<UsuarioAcesso> UsuariosAcesso { get; set; } = new List<UsuarioAcesso>();
}