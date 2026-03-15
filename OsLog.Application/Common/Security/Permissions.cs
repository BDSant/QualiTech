namespace OsLog.Application.Common.Security;

public static class Permissions
{
    public static class Empresa
    {
        public const string Criar = "empresa.criar";
        public const string Consultar = "empresa.consultar";
        public const string Excluir = "empresa.excluir";
        public const string Editar = "empresa.editar";
    }

    public static class Cliente
    {
        public const string Criar = "cliente.criar";
        public const string Consultar = "cliente.consultar";
        public const string Excluir = "cliente.excluir";
        public const string Editar = "cliente.editar";
    }

    public static class Tecnico
    {
        public const string Criar = "tecnico.criar";
        public const string Consultar = "tecnico.consultar";
        public const string Excluir = "tecnico.excluir";
        public const string Editar = "tecnico.editar";
    }

    public static class OrdemServico
    {
        public const string Criar = "ordemservico.criar";
        public const string Consultar = "ordemservico.consultar";
        public const string Excluir = "ordemservico.excluir";
        public const string Editar = "ordemservico.editar";
    }
}