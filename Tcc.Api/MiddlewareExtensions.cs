namespace Tcc.Api;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseContextMiddleware(this IApplicationBuilder builder, string apikey)
    {
        return builder.UseMiddleware<ContextMiddleware>(apikey);
    }
}
