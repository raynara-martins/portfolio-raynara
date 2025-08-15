using System.Text.Json;

namespace Tests.Common;

public static class JsonData
{
    public static T Read<T>(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, relativePath);
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public static string ReadRaw(string relativePath)
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, relativePath);
        return File.ReadAllText(path);
    }
}
