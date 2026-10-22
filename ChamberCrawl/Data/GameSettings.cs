using System.Text.Json;

namespace ChamberCrawl.Data;

public sealed class GameSettings
{
    public const int MinWindowScale = 1;
    public const int MaxWindowScale = 4;
    public const int DefaultWindowScale = 2;

    public bool Fullscreen { get; set; }
    public int WindowScale { get; set; } = DefaultWindowScale;

    public int ClampedWindowScale => ClampWindowScale(WindowScale);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static string DefaultPath =>
        Path.Combine(AppContext.BaseDirectory, "settings.json");

    public static int ClampWindowScale(int scale) =>
        Math.Clamp(scale, MinWindowScale, MaxWindowScale);

    public static GameSettings Load(string? path = null)
    {
        path ??= DefaultPath;
        try
        {
            if (!File.Exists(path))
                return new GameSettings();
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<GameSettings>(json, JsonOptions) ?? new GameSettings();
            loaded.WindowScale = ClampWindowScale(loaded.WindowScale);
            return loaded;
        }
        catch
        {
            return new GameSettings();
        }
    }

    public void Save(string? path = null)
    {
        path ??= DefaultPath;
        WindowScale = ClampedWindowScale;
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(path, json);
    }
}
