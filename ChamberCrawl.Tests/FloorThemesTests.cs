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
    [InlineData(GameConstants.TilePuddle, "puddle")]
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

    [Theory]
    [InlineData(GameConstants.TileFloor, 1, "floor")]
    [InlineData(GameConstants.TileWall, 1, "wall")]
    [InlineData(GameConstants.TileDoor, 1, "door-tile")]
    [InlineData(GameConstants.TileFloor, 2, "floor-2")]
    [InlineData(GameConstants.TileWall, 2, "wall-2")]
    [InlineData(GameConstants.TileDoor, 2, "door-tile-2")]
    [InlineData(GameConstants.TileFloor, 3, "floor-3")]
    [InlineData(GameConstants.TileWall, 3, "wall-3")]
    [InlineData(GameConstants.TileDoor, 3, "door-tile-3")]
    public void TextureForTile_UsesDepthSuffix(int tile, int depth, string expected)
    {
        Assert.Equal(expected, FloorThemes.TextureForTile(tile, depth));
    }

    [Fact]
    public void DoorTexture_UsesDepthSuffix()
    {
        Assert.Equal("door", FloorThemes.DoorTexture(1));
        Assert.Equal("door-2", FloorThemes.DoorTexture(2));
        Assert.Equal("door-3", FloorThemes.DoorTexture(3));
    }
}
