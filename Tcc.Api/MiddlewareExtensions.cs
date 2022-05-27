namespace Tcc.Api;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseContextMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ContextMiddleware>();
    }
}
