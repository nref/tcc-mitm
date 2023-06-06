namespace Tcc.Api;

public static class Program
{
  public static async Task Main(string[] args)
  {
    Context.Id.Value = Guid.NewGuid();

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddLettuceEncrypt();

    var app = builder.Build();

    // Setup log
    var factory = app.Services.GetService<ILoggerFactory>();
    ILogger log = factory!.CreateLogger("Tcc.Api");
    Log.Sink = log;

    var config = app.Services.GetService<IConfiguration>();
    string user = config!["tcc:user"];
    string password = config!["tcc:password"];
    string apikey = config!["tcc:apikey"] ?? "supersecret";

    app.UseWhen(context => context.Request.Path != "/", builder =>
    {
      builder.UseContextMiddleware(apikey);
    });
    app.UseHttpsRedirection();
    app.UseHttpLogging();

    IFileRepo repo = new FileRepo("sessionid.txt");
    ITccClient client = new TccClient(repo, user, password);

    app.MapGet("/", async context =>
    {
      context.Response.Redirect("https://github.com/slater1/tcc-mitm");
      await Task.CompletedTask;
    });

    app.MapGet("/setpoint", async context =>
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
        Log.Error($"Could not find setpoint in request");
        return;
      }

      Log.Info($"/setpoint: Got setpoint {setpoint}");
      bool ok = await client.SetCoolSetpointAsync(setpoint);

      context.Response.StatusCode = ok
              ? StatusCodes.Status200OK
              : StatusCodes.Status500InternalServerError;

      context.Response.ContentType = "application/json";
    });

    app.MapPost("/fan/on", async context =>
    {
      bool ok = await client.SetFanAsync(on: true);

      context.Response.StatusCode = ok
              ? StatusCodes.Status200OK
              : StatusCodes.Status500InternalServerError;
    });

    app.MapPost("/fan/off", async context =>
    {
      bool ok = await client.SetFanAsync(on: false);

      context.Response.StatusCode = ok
              ? StatusCodes.Status200OK
              : StatusCodes.Status500InternalServerError;
    });

    app.MapPost("/fan/schedule/{minutes}", async (int minutes, HttpContext context) =>
    {
      bool ok = await client.ScheduleFanAsync(minutes);

      context.Response.StatusCode = ok
              ? StatusCodes.Status200OK
              : StatusCodes.Status500InternalServerError;
    });

    await app.RunAsync();
  }
}
