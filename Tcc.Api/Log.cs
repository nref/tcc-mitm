using Microsoft.Extensions.Logging.Abstractions;

namespace Tcc.Api;

public static class Log
{
    public static ILogger Sink { get; internal set; } = NullLogger.Instance;

    public static void Debug(object o) => Sink.LogDebug($"[{Context.Id.Value}] {o}");
    public static void Info(object o) => Sink.LogInformation($"[{Context.Id.Value}] {o}");
    public static void Warn(object o) => Sink.LogWarning($"[{Context.Id.Value}] {o}");
    public static void Error(object o) => Sink.LogError($"[{Context.Id.Value}] {o}");
}
