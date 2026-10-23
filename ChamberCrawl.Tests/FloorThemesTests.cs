using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Tests;

public class FloorThemesTests
{
    [Theory]
    [InlineData(GameConstants.TileCrate, "crate")]
    [InlineData(GameConstants.TileBarrel, "barrel")]
    [InlineData(GameConstants.TileSpike, "spike")]
    [InlineData(GameConstants.TileObstacle, "obstacle")]
    public void TryOverlayTexture_ReturnsObstacleSprites(int tile, string expected)
    {
        Assert.True(FloorThemes.TryOverlayTexture(tile, out var name));
        Assert.Equal(expected, name);
    }

    [Theory]
    [InlineData(GameConstants.TileFloor)]
    [InlineData(GameConstants.TileWall)]
    [InlineData(GameConstants.TileDoor)]
    public void TryOverlayTexture_FalseForBaseTiles(int tile)
    {
        Assert.False(FloorThemes.TryOverlayTexture(tile, out _));
    }
}
