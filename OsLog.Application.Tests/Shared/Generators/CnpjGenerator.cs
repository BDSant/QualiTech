namespace OsLog.Tests.Shared.Generators;

public static class CnpjGenerator
{
    /// <summary>
    /// Gera um CNPJ válido (apenas números, 14 dígitos) para uso em testes.
    /// Não use para dados reais.
    /// </summary>
    public static string GenerateValidCnpj(Random? random = null)
    {
        random ??= new Random();

        // 12 dígitos base: 8 do "raiz" + 4 da "filial"
        // Aqui geramos aleatório; você pode fixar parte se quiser previsibilidade.
        var baseDigits = new int[12];

        for (var i = 0; i < 8; i++)
            baseDigits[i] = random.Next(0, 10);

        // Filial: por padrão "0001" (matriz) para ficar mais realista
        baseDigits[8] = 0;
        baseDigits[9] = 0;
        baseDigits[10] = 0;
        baseDigits[11] = 1;

        var dv1 = CalculateDigit(baseDigits, new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });
        var dv2 = CalculateDigit(baseDigits.Concat(new[] { dv1 }).ToArray(),
                                 new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });

        return string.Concat(baseDigits.Select(d => d.ToString())) + dv1 + dv2;
    }

    /// <summary>
    /// Gera um CNPJ válido determinístico a partir de um seed (ideal para testes reproduzíveis).
    /// </summary>
    public static string GenerateValidCnpj(int seed)
    {
        return GenerateValidCnpj(new Random(seed));
    }

    private static int CalculateDigit(int[] digits, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
            sum += digits[i] * weights[i];

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
