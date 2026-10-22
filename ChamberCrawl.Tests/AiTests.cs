using System.Linq;
using ChamberCrawl.Data;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class AiTests
{
    private const float Dt = 1f / 60f;

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

    private static EnemyEntity AddEnemy(RunState run, EnemyKind kind, float x, float y, float radius = 9)
    {
        var enemy = new EnemyEntity
        {
            Id = $"e{run.Entities.Count}",
            Enemy = kind,
            X = x,
            Y = y,
            Radius = radius,
            Hp = Enemies.Defs[kind].Hp,
        };
        run.Entities.Add(enemy);
        return enemy;
    }

    [Fact]
    public void Update_Pspam_SplitsIntoTwoChildrenAfter4Seconds()
    {
        var run = OpenRoomRun();
        var parent = AddEnemy(run, EnemyKind.Pspam, 200, 160);
        parent.CanSplit = true;
        parent.Cooldown = 4000f;
        parent.Hp = 2;

        Ai.Update(run, 4.1f);

        Assert.DoesNotContain(run.Entities, e => e.Id == parent.Id);
        var kids = run.Entities.OfType<EnemyEntity>().Where(e => e.Enemy == EnemyKind.Pspam).ToList();
        Assert.Equal(2, kids.Count);
        Assert.All(kids, k => Assert.False(k.CanSplit));
        Assert.All(kids, k => Assert.Equal(1, k.Hp));
    }

    [Fact]
    public void Update_PspamChild_DoesNotSplit()
    {
        var run = OpenRoomRun();
        var child = AddEnemy(run, EnemyKind.Pspam, 200, 160);
        child.CanSplit = false;
        child.Cooldown = 4000f;
        Ai.Update(run, 5f);
        Assert.Contains(run.Entities, e => e.Id == child.Id);
        Assert.Single(run.Entities.OfType<EnemyEntity>());
    }

    [Fact]
    public void Update_Trojan_StaysIdleFarAway()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Trojan, 100, 160);
        enemy.Phase = 0;
        run.Entities.OfType<PlayerEntity>().Single().X = 300;
        Ai.Update(run, Dt);
        Assert.Equal(0, enemy.Phase);
        Assert.Equal(100, enemy.X);
    }

    [Fact]
    public void Update_Trojan_ChargesWhenPlayerClose()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Trojan, 200, 160);
        enemy.Phase = 0;
        run.Entities.OfType<PlayerEntity>().Single().X = 250;
        Ai.Update(run, Dt);
        Assert.Equal(1, enemy.Phase);
        Assert.True(enemy.X > 200);
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
