namespace Tcc.Api;

public class ContextMiddleware
{
    private readonly RequestDelegate _next;

    public ContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Context.Id.Value = Guid.NewGuid();
        await _next(context);
    }

}
