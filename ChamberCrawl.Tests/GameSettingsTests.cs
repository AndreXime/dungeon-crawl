using ChamberCrawl.Data;

namespace ChamberCrawl.Tests;

public class GameSettingsTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsFullscreen()
    {
        var path = Path.Combine(Path.GetTempPath(), $"chamber-settings-{Guid.NewGuid():N}.json");
        try
        {
            var settings = new GameSettings { Fullscreen = true };
            settings.Save(path);

            var loaded = GameSettings.Load(path);
            Assert.True(loaded.Fullscreen);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Load_MissingFile_ReturnsDefaults()
    {
        var path = Path.Combine(Path.GetTempPath(), $"chamber-missing-{Guid.NewGuid():N}.json");
        var loaded = GameSettings.Load(path);
        Assert.False(loaded.Fullscreen);
    }
}
