using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class AiTests
{
    private static RunState OpenRoomRun()
    {
        var run = new RunState();
        for (var y = 0; y < GameConstants.RoomH; y++)
            for (var x = 0; x < GameConstants.RoomW; x++)
                run.Room.Tiles[y, x] = x == 0 || y == 0 || x == GameConstants.RoomW - 1 || y == GameConstants.RoomH - 1
                    ? GameConstants.TileWall
                    : GameConstants.TileFloor;
        run.PlayerStats = new PlayerStats { MaxHp = 6, Hp = 6 };
        run.Entities.Add(new PlayerEntity
        {
            Id = "player", X = 300, Y = 160, Radius = GameConstants.PlayerRadius, Hp = 6,
        });
        return run;
    }

    [Fact]
    public void Update_NotPlaying_DoesNothing()
    {
        var run = OpenRoomRun();
        run.Status = RunStatus.Dead;
        var enemy = new EnemyEntity { Id = "e1", Enemy = EnemyKind.Trojan, X = 100, Y = 160, Radius = 10, Hp = 5 };
        run.Entities.Add(enemy);

        Ai.Update(run, 1f);

        Assert.Equal(100, enemy.X);
    }

    [Theory]
    [InlineData(EnemyKind.Pspam)]
    [InlineData(EnemyKind.Trojan)]
    [InlineData(EnemyKind.AdwareKing)]
    public void Update_Playing_StubDoesNotThrow(EnemyKind kind)
    {
        var run = OpenRoomRun();
        run.Entities.Add(new EnemyEntity { Id = "e1", Enemy = kind, X = 100, Y = 160, Radius = 10, Hp = 5 });

        Ai.Update(run, 1f / 60f);
    }
}
