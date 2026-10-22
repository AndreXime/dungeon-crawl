using System.Text.Json;

namespace ChamberCrawl.Data;

public sealed class GameSettings
{
    public bool Fullscreen { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static string DefaultPath =>
        Path.Combine(AppContext.BaseDirectory, "settings.json");

    public static GameSettings Load(string? path = null)
    {
        path ??= DefaultPath;
        try
        {
            if (!File.Exists(path))
                return new GameSettings();
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<GameSettings>(json, JsonOptions) ?? new GameSettings();
        }
        catch
        {
            return new GameSettings();
        }
    }

    public void Save(string? path = null)
    {
        path ??= DefaultPath;
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(path, json);
    }
}
