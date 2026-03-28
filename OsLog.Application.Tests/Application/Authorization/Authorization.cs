using Xunit;

namespace OsLog.Tests.Unit.Application.Authorization;

public class EscopoUnidadeTests
{
    [Fact(DisplayName = "[UNIT] Usuário com escopo unidade não deve enxergar dados fora da própria unidade")]
    [Trait("Category", "Unit")]
    [Trait("SubCategory", "Authorization")]
    public Task UsuarioDeEscopoUnidade_NaoDeveEnxergarDadosDeOutraUnidade()
    {
        // Pendente:
        // Implementar quando houver filtro por EmpresaId/UnidadeId
        // aplicado aos casos de uso/serviços de consulta.
        //
        // Cenário esperado:
        // 1. usuário autenticado com Escopo = Unidade
        // 2. consulta retorna apenas dados da própria unidade
        // 3. tentativa de acessar outra unidade falha

        return Task.CompletedTask;
    }
}