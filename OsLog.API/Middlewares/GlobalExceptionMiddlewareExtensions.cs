namespace OsLog.API.Middlewares;

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseOsLogExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
