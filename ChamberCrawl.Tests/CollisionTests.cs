using System.Collections.Generic;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class CollisionTests
{
    private const int W = GameConstants.TileWall;
    private const int F = GameConstants.TileFloor;
    private const int O = GameConstants.TileObstacle;
    private const int D = GameConstants.TileDoor;
    private const int Cell = GameConstants.Cell;

    [Fact]
    public void CirclesOverlap_DetectsOverlapAndSeparation()
    {
        Assert.True(Collision.CirclesOverlap(0, 0, 5, 6, 0, 5));
        Assert.False(Collision.CirclesOverlap(0, 0, 5, 20, 0, 5));
    }

    [Fact]
    public void CircleHitsSolid_BlocksAgainstWalls()
    {
        var tiles = new[,] { { W, W }, { W, F } };
        Assert.False(Collision.CircleHitsSolid(Cell * 1.5f, Cell * 1.5f, 8, tiles));
        Assert.True(Collision.CircleHitsSolid(8, 8, 8, tiles));
    }

    [Fact]
    public void CircleHitsSolid_BlocksAgainstObstaclesAndOutOfBounds()
    {
        var tiles = new[,] { { F, O }, { D, F } };
        Assert.True(Collision.CircleHitsSolid(Cell * 1.5f, Cell * 0.5f, 6, tiles));
        Assert.False(Collision.CircleHitsSolid(Cell * 0.5f, Cell * 1.5f, 6, tiles));
        Assert.True(Collision.CircleHitsSolid(-10, -10, 5, tiles));
    }

    [Theory]
    [InlineData(GameConstants.TileCrate)]
    [InlineData(GameConstants.TileBarrel)]
    [InlineData(GameConstants.TileSpike)]
    public void CircleHitsSolid_BlocksNewPartitionTiles(int tile)
    {
        var tiles = new[,] { { GameConstants.TileFloor, tile }, { GameConstants.TileDoor, GameConstants.TileFloor } };
        Assert.True(Collision.CircleHitsSolid(GameConstants.Cell * 1.5f, GameConstants.Cell * 0.5f, 6, tiles));
    }

    [Fact]
    public void TrySampleSolidTile_ReturnsFolderUnderProjectile()
    {
        var tiles = new[,] { { GameConstants.TileFloor, GameConstants.TileCrate } };
        var ok = Collision.TrySampleSolidTile(
            GameConstants.Cell * 1.5f, GameConstants.Cell * 0.5f, 4, tiles, out var tx, out var ty, out var tile);
        Assert.True(ok);
        Assert.Equal(1, tx);
        Assert.Equal(0, ty);
        Assert.Equal(GameConstants.TileCrate, tile);
    }

    [Fact]
    public void IsRoomCleared_ChecksForLivingEnemies()
    {
        var alive = new List<Entity>
        {
            new PlayerEntity { Id = "p1", Hp = 6 },
            new EnemyEntity { Id = "e1", Enemy = EnemyKind.Bat, Hp = 2 },
        };
        var cleared = new List<Entity>
        {
            new PlayerEntity { Id = "p1", Hp = 6 },
            new EnemyEntity { Id = "e1", Enemy = EnemyKind.Bat, Hp = 0 },
        };
        Assert.False(Doors.IsRoomCleared(alive));
        Assert.True(Doors.IsRoomCleared(cleared));
    }
}
