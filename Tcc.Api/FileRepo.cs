namespace Tcc.Api;

public interface IFileRepo
{
    Task<string> LoadSessionIdAsync();
    Task SaveSessionIdAsync(string sessionId);
}

public class FileRepo : IFileRepo
{
    private readonly string _path;

    public FileRepo(string path)
    {
        _path = path;
    }

    public async Task<string> LoadSessionIdAsync()
    {
        try
        {
            using var fw = new FileStream(_path, FileMode.Open, FileAccess.Read);
            using var sw = new StreamReader(fw);
            string sessionId = await sw.ReadLineAsync() ?? "";
            return sessionId;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return "";
        }
    }

    public async Task SaveSessionIdAsync(string sessionId)
    {
        try
        {
            using var rs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write);
            rs.SetLength(0);
            using var sw = new StreamWriter(rs);
            await sw.WriteLineAsync(sessionId);
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }
}
