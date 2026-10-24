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
    [InlineData(GameConstants.TileRune, "rune")]
    [InlineData(GameConstants.TilePedestal, "pedestal")]
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
    [InlineData(GameConstants.TileFloor, 4, "floor-4")]
    [InlineData(GameConstants.TileWall, 4, "wall-4")]
    [InlineData(GameConstants.TileDoor, 4, "door-tile-4")]
    [InlineData(GameConstants.TileFloor, 5, "floor-5")]
    [InlineData(GameConstants.TileWall, 5, "wall-5")]
    [InlineData(GameConstants.TileDoor, 5, "door-tile-5")]
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
        Assert.Equal("door-4", FloorThemes.DoorTexture(4));
        Assert.Equal("door-5", FloorThemes.DoorTexture(5));
    }

    [Theory]
    [InlineData(1, "MASMORRA")]
    [InlineData(2, "CATACUMBAS")]
    [InlineData(3, "ESGOTOS")]
    [InlineData(4, "ARSENAL")]
    [InlineData(5, "SANTUARIO")]
    public void NameForDepth_MatchesBiome(int depth, string expected)
    {
        Assert.Equal(expected, FloorThemes.NameForDepth(depth));
    }

    [Theory]
    [InlineData(1, "FirstFloorTheme")]
    [InlineData(2, "CatacombsTheme")]
    [InlineData(3, "SewersTheme")]
    [InlineData(4, "ArmoryTheme")]
    [InlineData(5, "SanctumTheme")]
    public void MusicForDepth_MatchesBiome(int depth, string expected)
    {
        Assert.Equal(expected, FloorThemes.MusicForDepth(depth));
    }
}
