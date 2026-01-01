using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OsLog.Application.Common.Responses;

namespace OsLog.API.Extensions;

public static class ValidationProblemOsLogExtensions
{
    /// <summary>
    /// Converte um ModelState inválido em um OsLogResponse de erro (400).
    /// </summary>
    public static IActionResult ValidationProblemOsLog(
        this ControllerBase controller,
        ModelStateDictionary modelState,
        int? codigoNegocio = null)
    {
        // Código de negócio padrão para erro de validação
        var codigo = codigoNegocio ?? CodigosOsLog.ERRO_VALIDACAO;

        var response = OsLogResponse<object?>.Critica(
            codigo: codigo,
            mensagem: CriticasOsLog.RetornaCritica(codigo),
            erros: modelState
        );

        // 400 BadRequest com o OsLogResponse no body
        return controller.BadRequest(response);
    }
}
