using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class MovementTests
{
    private static RunState OpenRunWithTile(int tile)
    {
        var run = new RunState();
        for (var y = 0; y < GameConstants.RoomH; y++)
            for (var x = 0; x < GameConstants.RoomW; x++)
                run.Room.Tiles[y, x] = x == 0 || y == 0 || x == GameConstants.RoomW - 1 || y == GameConstants.RoomH - 1
                    ? GameConstants.TileWall
                    : GameConstants.TileFloor;

        run.Room.Tiles[5, 7] = tile;
        run.PlayerStats = new PlayerStats { MaxHp = 6, Hp = 6, Speed = 120 };
        run.Entities.Add(new PlayerEntity
        {
            Id = "player",
            X = 7.5f * GameConstants.Cell,
            Y = 5.5f * GameConstants.Cell,
            Radius = GameConstants.PlayerRadius,
            Hp = 6,
        });
        return run;
    }

    [Fact]
    public void Update_SlowsPlayerOnPuddle()
    {
        var slow = OpenRunWithTile(GameConstants.TilePuddle);
        var player = slow.Entities.OfType<PlayerEntity>().Single();
        var startX = player.X;
        Movement.Update(slow, new InputSnapshot(1, 0, 0, 0, false), 0.1f);
        var slowDelta = player.X - startX;

        var dry = OpenRunWithTile(GameConstants.TileFloor);
        var dryPlayer = dry.Entities.OfType<PlayerEntity>().Single();
        var dryStart = dryPlayer.X;
        Movement.Update(dry, new InputSnapshot(1, 0, 0, 0, false), 0.1f);
        var dryDelta = dryPlayer.X - dryStart;

        Assert.True(slowDelta > 0);
        Assert.True(dryDelta > slowDelta);
        Assert.Equal(dryDelta * Movement.SlowZoneSpeedFactor, slowDelta, 2);
    }

    [Fact]
    public void Update_BlocksPlayerAgainstLockedDoor()
    {
        var run = OpenRunWithTile(GameConstants.TileDoor);
        var player = run.Entities.OfType<PlayerEntity>().Single();
        run.Entities.Add(new DoorEntity
        {
            Id = "door",
            Locked = true,
            X = player.X + GameConstants.Cell,
            Y = player.Y,
            Radius = GameConstants.Cell / 2f,
        });
        var startX = player.X;
        Movement.Update(run, new InputSnapshot(1, 0, 0, 0, false), 0.2f);
        Assert.True(player.X < startX + 8f);
    }

    [Fact]
    public void SpeedScale_IgnoresPuddleForSlimeKinds()
    {
        var tiles = new int[GameConstants.RoomH, GameConstants.RoomW];
        tiles[1, 1] = GameConstants.TilePuddle;
        var x = 1.5f * GameConstants.Cell;
        var y = 1.5f * GameConstants.Cell;
        Assert.Equal(Movement.SlowZoneSpeedFactor, Movement.SpeedScale(x, y, tiles));
        Assert.Equal(1f, Movement.SpeedScale(x, y, tiles, EnemyKind.Slime));
        Assert.Equal(1f, Movement.SpeedScale(x, y, tiles, EnemyKind.SlimeKing));
        Assert.Equal(Movement.SlowZoneSpeedFactor, Movement.SpeedScale(x, y, tiles, EnemyKind.Rat));
    }
}
