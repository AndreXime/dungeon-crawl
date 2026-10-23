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
    public void Update_NotPlaying_DoesNothing()
    {
        var run = OpenRoomRun();
        run.Status = RunStatus.Dead;
        var enemy = new EnemyEntity { Id = "e1", Enemy = EnemyKind.Bat, X = 100, Y = 160, Radius = 10, Hp = 5 };
        run.Entities.Add(enemy);

        Ai.Update(run, 1f);

        Assert.Equal(100, enemy.X);
    }

    [Fact]
    public void Update_Slime_SplitsIntoTwoChildrenAfter4Seconds()
    {
        var run = OpenRoomRun();
        var parent = AddEnemy(run, EnemyKind.Slime, 200, 160);
        parent.CanSplit = true;
        parent.Cooldown = 4000f;
        parent.Hp = 2;

        Ai.Update(run, 4.1f);

        Assert.DoesNotContain(run.Entities, e => e.Id == parent.Id);
        var kids = run.Entities.OfType<EnemyEntity>().Where(e => e.Enemy == EnemyKind.Slime).ToList();
        Assert.Equal(2, kids.Count);
        Assert.All(kids, k => Assert.False(k.CanSplit));
        Assert.All(kids, k => Assert.Equal(1, k.Hp));
    }

    [Fact]
    public void Update_SlimeChild_DoesNotSplit()
    {
        var run = OpenRoomRun();
        var child = AddEnemy(run, EnemyKind.Slime, 200, 160);
        child.CanSplit = false;
        child.Cooldown = 4000f;
        Ai.Update(run, 5f);
        Assert.Contains(run.Entities, e => e.Id == child.Id);
        Assert.Single(run.Entities.OfType<EnemyEntity>());
    }

    [Fact]
    public void Update_Bat_ChasesPlayer()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Bat, 100, 160);
        run.Entities.OfType<PlayerEntity>().Single().X = 300;
        var before = enemy.X;
        Ai.Update(run, Dt);
        Assert.True(enemy.X > before);
    }

    [Theory]
    [InlineData(EnemyKind.Skeleton)]
    [InlineData(EnemyKind.Ogre)]
    public void Update_Playing_StubDoesNotThrow(EnemyKind kind)
    {
        var run = OpenRoomRun();
        run.Entities.Add(new EnemyEntity { Id = "e1", Enemy = kind, X = 100, Y = 160, Radius = 10, Hp = 5 });

        Ai.Update(run, Dt);
    }

    [Fact]
    public void Combat_IgnoresDamageWhileEnemyIframes()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Ogre, 200, 160, 24);
        boss.Hp = 40;
        boss.IframeUntil = 10_000;
        run.TimeMs = 100;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "hit", FromPlayer = true, X = 200, Y = 160, Vx = 0, Vy = 0,
            Radius = 4, Damage = 5, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), Dt);
        Assert.Equal(40, boss.Hp);
    }
}
