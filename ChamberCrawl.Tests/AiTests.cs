using System.Linq;
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

    private static EnemyEntity AddEnemy(RunState run, EnemyKind kind, float x, float y, float radius = 10)
    {
        var enemy = new EnemyEntity { Id = $"e{run.Entities.Count}", Enemy = kind, X = x, Y = y, Radius = radius, Hp = 5 };
        run.Entities.Add(enemy);
        return enemy;
    }

    [Fact]
    public void Update_Chaser_MovesTowardPlayer()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Chaser, 100, 160);

        Ai.Update(run, 1f);

        Assert.Equal(170, enemy.X, 2);
        Assert.Equal(160, enemy.Y, 2);
    }

    [Fact]
    public void Update_Chaser_DoesNotDamagePlayerOnContact()
    {
        var run = OpenRoomRun();
        AddEnemy(run, EnemyKind.Chaser, 300, 160);

        Ai.Update(run, Dt);

        Assert.Equal(6, run.PlayerStats.Hp);
    }

    [Fact]
    public void Update_Chaser_StopsAtWall()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Chaser, 100, 160);
        run.Entities.OfType<PlayerEntity>().Single().X = 5;

        for (var i = 0; i < 120; i++) Ai.Update(run, Dt);

        Assert.True(enemy.X >= GameConstants.Cell + enemy.Radius - 1f);
    }

    [Fact]
    public void Update_NotPlaying_DoesNothing()
    {
        var run = OpenRoomRun();
        run.Status = RunStatus.Dead;
        var enemy = AddEnemy(run, EnemyKind.Chaser, 100, 160);

        Ai.Update(run, 1f);

        Assert.Equal(100, enemy.X);
    }

    [Fact]
    public void Update_Wander_PicksDirectionFromRngAndMoves()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Wander, 200, 160, 9);

        Ai.Update(run, 0.1f, () => 0f);

        Assert.Equal(200 + 50 * 0.1f, enemy.X, 2);
        Assert.Equal(500f, enemy.Cooldown, 1);
    }

    [Fact]
    public void Update_Wander_RerollsDirectionAfterHittingWall()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Wander, GameConstants.Cell + 9, 160, 9);
        enemy.Vx = -50;
        enemy.Cooldown = 1000;

        Ai.Update(run, Dt, () => 0f);

        Assert.Equal(0f, enemy.Cooldown);
    }

    [Fact]
    public void Update_Shooter_FiresAtPlayerWhenCooldownExpires()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Shooter, 160, 160);

        Ai.Update(run, Dt);

        var shot = Assert.Single(run.Entities.OfType<ProjectileEntity>());
        Assert.False(shot.FromPlayer);
        Assert.True(shot.Vx > 0);
        Assert.Equal(1500f, enemy.Cooldown);
    }

    [Fact]
    public void Update_Shooter_DoesNotFireWhileOnCooldown()
    {
        var run = OpenRoomRun();
        AddEnemy(run, EnemyKind.Shooter, 160, 160).Cooldown = 1000;

        Ai.Update(run, Dt);

        Assert.Empty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_Shooter_RetreatsWhenTooCloseAndHoldsAtPreferredDistance()
    {
        var run = OpenRoomRun();
        var close = AddEnemy(run, EnemyKind.Shooter, 250, 160);
        close.Cooldown = 1000;
        var holding = AddEnemy(run, EnemyKind.Shooter, 160, 100);
        holding.Cooldown = 1000;
        run.Entities.OfType<PlayerEntity>().Single().Y = 100;

        Ai.Update(run, Dt);

        Assert.True(close.X < 250);
        Assert.Equal(160, holding.X);
        Assert.Equal(0, holding.Vx);
    }

    [Fact]
    public void Update_Boss_ChargesTowardPlayerThenFiresEightDirectionBurst()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Boss, 100, 160, 22);

        Ai.Update(run, 1.5f);
        Assert.Equal(100 + 110 * 1.5f, boss.X, 1);
        Assert.Empty(run.Entities.OfType<ProjectileEntity>());

        Ai.Update(run, 0.6f);
        Assert.Equal(8, run.Entities.OfType<ProjectileEntity>().Count());
        Assert.All(run.Entities.OfType<ProjectileEntity>(), p => Assert.False(p.FromPlayer));
    }

    [Fact]
    public void Update_Boss_StandsStillDuringBurstPhaseAndWrapsCycle()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Boss, 100, 160, 22);
        boss.Phase = 2.1f;
        var x = boss.X;

        Ai.Update(run, 0.5f);
        Assert.Equal(x, boss.X);

        Ai.Update(run, 0.5f);
        Assert.InRange(boss.Phase, 0.09f, 0.11f);
    }
}
