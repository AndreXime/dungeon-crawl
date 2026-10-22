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

    [Fact]
    public void IsRoomCleared_ChecksForLivingEnemies()
    {
        var alive = new List<Entity>
        {
            new PlayerEntity { Id = "p1", Hp = 6 },
            new EnemyEntity { Id = "e1", Enemy = EnemyKind.Chaser, Hp = 2 },
        };
        var cleared = new List<Entity>
        {
            new PlayerEntity { Id = "p1", Hp = 6 },
            new EnemyEntity { Id = "e1", Enemy = EnemyKind.Chaser, Hp = 0 },
        };
        Assert.False(Doors.IsRoomCleared(alive));
        Assert.True(Doors.IsRoomCleared(cleared));
    }
}
