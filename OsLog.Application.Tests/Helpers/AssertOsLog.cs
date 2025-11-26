using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OsLog.Application.Common.Responses;

//public static class AssertOsLog
//{
//    public static OsLogResponse<T> Ok<T>(IActionResult result)
//    {
//        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
//        Assert.True(objectResult.StatusCode is null or StatusCodes.Status200OK);

//        var response = Assert.IsType<OsLogResponse<T>>(objectResult.Value);
//        Assert.True(response.Sucesso);
//        Assert.True(response.Codigo is null or 0);

//        return response;
//    }

//    public static OsLogResponse Critica(IActionResult result, int expectedCodigo)
//    {
//        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
//        var response = Assert.IsType<OsLogResponse>(objectResult.Value);

//        Assert.False(response.Sucesso);
//        Assert.Equal(expectedCodigo, response.Codigo);

//        return response;
//    }
//}
