namespace Tcc.Api;

public class ContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apikey;

    public ContextMiddleware(RequestDelegate next, string apikey)
    {
        _next = next;
        _apikey = apikey;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string key = context.Request.Headers["x-tcc-apikey"];

        if (key != _apikey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            string json = System.Text.Json.JsonSerializer.Serialize(new { error = $"Bad apikey '{key}'" });
            await context.Response.WriteAsync(json);
            return;
        }

        Context.Id.Value = Guid.NewGuid();
        await _next(context);
    }

}
