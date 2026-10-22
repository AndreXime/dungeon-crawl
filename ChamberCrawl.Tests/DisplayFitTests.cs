using ChamberCrawl;
using Microsoft.Xna.Framework;

namespace ChamberCrawl.Tests;

public class DisplayFitTests
{
    [Fact]
    public void FitSceneDestination_Fullscreen_FillsOneAxisWithoutCropping()
    {
        // 1920x1080 vs 480x376 → altura limita (~2.87x), largura com barras
        var dest = Game1.FitSceneDestination(1920, 1080, 480, 376, integerScale: false);
        Assert.Equal(1080, dest.Height);
        Assert.True(dest.Width < 1920);
        Assert.True(dest.Width > 480 * 2);
        Assert.Equal(0, dest.Y);
        Assert.Equal((1920 - dest.Width) / 2, dest.X);
    }

    [Fact]
    public void FitSceneDestination_Integer_UsesWholeMultiplesOnly()
    {
        var dest = Game1.FitSceneDestination(1920, 1080, 480, 376, integerScale: true);
        Assert.Equal(960, dest.Width);
        Assert.Equal(752, dest.Height);
    }
}
