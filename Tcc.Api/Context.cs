namespace Tcc.Api;

public static class Context
{
    public static AsyncLocal<Guid> Id { get; } = new();
}
