using ChamberCrawl.Data;

namespace ChamberCrawl.Tests;

public class GameSettingsTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsFullscreenAndScale()
    {
        var path = Path.Combine(Path.GetTempPath(), $"chamber-settings-{Guid.NewGuid():N}.json");
        try
        {
            var settings = new GameSettings { Fullscreen = true, WindowScale = 3 };
            settings.Save(path);

            var loaded = GameSettings.Load(path);
            Assert.True(loaded.Fullscreen);
            Assert.Equal(3, loaded.WindowScale);
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
        Assert.Equal(GameSettings.DefaultWindowScale, loaded.WindowScale);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(5, 4)]
    [InlineData(2, 2)]
    public void ClampWindowScale_LimitsRange(int input, int expected)
    {
        Assert.Equal(expected, GameSettings.ClampWindowScale(input));
    }
}
