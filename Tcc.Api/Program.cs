namespace Tcc.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Context.Id.Value = Guid.NewGuid();

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddLettuceEncrypt();

        var app = builder.Build();

        app.UseContextMiddleware();
        app.UseHttpsRedirection();
        app.UseHttpLogging();

        // Setup log
        var factory = app.Services.GetService<ILoggerFactory>();
        ILogger log = factory!.CreateLogger("Tcc.Api");
        Log.Sink = log;

        var config = app.Services.GetService<IConfiguration>();
        string user = config!["tcc:user"];
        string password = config!["tcc:password"];

        IFileRepo repo = new FileRepo("sessionid.txt");
        ITccClient client = new TccClient(repo, user, password);

        app.MapGet("setpoint", async context =>
        {
            int? setpoint = await client.GetCoolSetpointAsync();

            context.Response.StatusCode = setpoint != null
                ? StatusCodes.Status200OK
                : StatusCodes.Status500InternalServerError;
            
            context.Response.ContentType = "application/json";

            string json = System.Text.Json.JsonSerializer.Serialize(new { setpoint });

            await context.Response.WriteAsync(json);
        });

        app.MapPost("/setpoint", async context =>
        {
            IFormCollection? form = await context.Request.ReadFormAsync();

            if (!form.TryGet("setpoint", out int setpoint))
            {
                Log.Error($"/cool: Could not find setpoint in request");
                return;
            }

            Log.Info($"/cool: Got setpoint {setpoint}");
            bool ok = await client.SetAsync(setpoint);

            context.Response.StatusCode = ok
                ? StatusCodes.Status200OK
                : StatusCodes.Status500InternalServerError;

            context.Response.ContentType = "application/json";
        });

        await app.RunAsync();
    }
}
