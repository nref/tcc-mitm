namespace Tcc.Api;

public static class FormCollectionExtensions
{
    public static bool TryGet(this IFormCollection? form, string key, out int value)
    {
        value = -1;
        return form != null && form.ContainsKey(key) && int.TryParse(form[key], out value);
    }
}
