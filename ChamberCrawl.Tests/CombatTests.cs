using System;
using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class CombatTests
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
        run.PlayerStats = new PlayerStats
        {
            MaxHp = 6, Hp = 6, Speed = 120, Damage = 1, FireRate = 2f, Range = 200, CoinMultiplier = 1,
        };
        run.Entities.Add(new PlayerEntity
        {
            Id = "player", X = 100, Y = 160, Radius = GameConstants.PlayerRadius, Hp = 6,
        });
        return run;
    }

    private static EnemyEntity AddEnemy(RunState run, float x, float y, float hp = 2, EnemyKind kind = EnemyKind.Bat)
    {
        var enemy = new EnemyEntity { Id = $"e{run.Entities.Count}", Enemy = kind, X = x, Y = y, Radius = 10, Hp = hp };
        run.Entities.Add(enemy);
        return enemy;
    }

    private static InputSnapshot Shoot(float x, float y) => new(0, 0, x, y, false);
    private static readonly InputSnapshot Idle = new(0, 0, 0, 0, false);

    [Fact]
    public void PlayerShot_SpawnsFromTopOfPlayer()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        Combat.Update(run, Shoot(1, 0), Dt);
        var shots = run.Entities.OfType<ProjectileEntity>().Where(p => p.FromPlayer).ToList();
        Assert.NotEmpty(shots);
        Assert.All(shots, s => Assert.True(s.Y < player.Y));
    }

    [Fact]
    public void Update_ShootInput_SpawnsSinglePlayerProjectileAndSetsCooldown()
    {
        var run = OpenRoomRun();

        Combat.Update(run, Shoot(1, 0), Dt);

        var shot = Assert.Single(run.Entities.OfType<ProjectileEntity>());
        Assert.True(shot.FromPlayer);
        Assert.True(shot.Vx > 0);
        Assert.Equal(0, shot.Vy, 3);
        Assert.Equal(0.5f, run.ShootCooldown, 3);
    }

    [Fact]
    public void Update_TripleShot_SpawnsThreeProjectilesSpreadFifteenDegrees()
    {
        var run = OpenRoomRun();
        run.PlayerStats.TripleShot = true;

        Combat.Update(run, Shoot(1, 0), Dt);

        var angles = run.Entities.OfType<ProjectileEntity>()
            .Select(p => MathF.Atan2(p.Vy, p.Vx) * 180f / MathF.PI)
            .OrderBy(a => a)
            .ToArray();
        Assert.Equal(3, angles.Length);
        Assert.Equal(-15, angles[0], 1);
        Assert.Equal(0, angles[1], 1);
        Assert.Equal(15, angles[2], 1);
    }

    [Fact]
    public void Update_WhileOnCooldown_DoesNotShootAgain()
    {
        var run = OpenRoomRun();

        Combat.Update(run, Shoot(1, 0), Dt);
        Combat.Update(run, Shoot(1, 0), Dt);

        Assert.Single(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_NoShootInput_DoesNotSpawnProjectile()
    {
        var run = OpenRoomRun();

        Combat.Update(run, Idle, Dt);

        Assert.Empty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_DoesNotAdvanceTimeMs()
    {
        var run = OpenRoomRun();
        run.TimeMs = 1234;

        Combat.Update(run, Shoot(1, 0), Dt);

        Assert.Equal(1234, run.TimeMs);
    }

    [Fact]
    public void Update_PlayerProjectileHitsEnemy_DamagesAndRemovesProjectile()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, 120, 160, hp: 3);
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p", FromPlayer = true, X = 115, Y = 160, Vx = 300, Radius = 4, Damage = 1, Life = 1,
        });

        Combat.Update(run, Idle, Dt);

        Assert.Equal(2, enemy.Hp);
        Assert.Empty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_KillingEnemy_RemovesItAndCanDropCoin()
    {
        var run = OpenRoomRun();
        run.PlayerStats.CoinMultiplier = 2;
        var enemy = AddEnemy(run, 120, 160, hp: 1);
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p", FromPlayer = true, X = 115, Y = 160, Vx = 300, Radius = 4, Damage = 1, Life = 1,
        });

        Combat.Update(run, Idle, Dt, () => 0f);

        Assert.DoesNotContain(enemy, run.Entities);
        var coin = Assert.Single(run.Entities.OfType<PickupEntity>());
        Assert.Equal(PickupKind.Coin, coin.Pickup);
        Assert.Equal(2, coin.Amount);
    }

    [Fact]
    public void Update_KillingEnemyWithHighRoll_DropsNoCoin()
    {
        var run = OpenRoomRun();
        AddEnemy(run, 120, 160, hp: 1);
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p", FromPlayer = true, X = 115, Y = 160, Vx = 300, Radius = 4, Damage = 1, Life = 1,
        });

        Combat.Update(run, Idle, Dt, () => 0.9f);

        Assert.Empty(run.Entities.OfType<PickupEntity>());
    }

    [Fact]
    public void Update_ProjectileHitsWall_IsRemoved()
    {
        var run = OpenRoomRun();
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p", FromPlayer = true, X = 40, Y = 160, Vx = -300, Radius = 4, Damage = 1, Life = 5,
        });

        Combat.Update(run, Idle, Dt);

        Assert.Empty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_ProjectileLifeExpires_IsRemoved()
    {
        var run = OpenRoomRun();
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p", FromPlayer = true, X = 200, Y = 160, Vx = 10, Radius = 4, Damage = 1, Life = Dt / 2,
        });

        Combat.Update(run, Idle, Dt);

        Assert.Empty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_EnemyProjectileHitsPlayer_DamagesAndGrantsIframes()
    {
        var run = OpenRoomRun();
        run.TimeMs = 1000;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p", FromPlayer = false, X = 100, Y = 160, Radius = 4, Damage = 1, Life = 1,
        });

        Combat.Update(run, Idle, Dt);

        Assert.Equal(5, run.PlayerStats.Hp);
        Assert.Equal(5, run.Entities.OfType<PlayerEntity>().Single().Hp);
        Assert.Equal(1000 + GameConstants.IframeMs, run.PlayerStats.IframeUntil);
        Assert.Empty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_DuringIframes_IgnoresDamage()
    {
        var run = OpenRoomRun();
        run.TimeMs = 1000;
        run.PlayerStats.IframeUntil = 1500;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p", FromPlayer = false, X = 100, Y = 160, Radius = 4, Damage = 1, Life = 1,
        });

        Combat.Update(run, Idle, Dt);

        Assert.Equal(6, run.PlayerStats.Hp);
    }

    [Fact]
    public void Update_EnemyContact_DamagesPlayerOnceThenIframesProtect()
    {
        var run = OpenRoomRun();
        AddEnemy(run, 105, 160);

        Combat.Update(run, Idle, Dt);
        Combat.Update(run, Idle, Dt);

        Assert.Equal(5, run.PlayerStats.Hp);
    }

    [Fact]
    public void Update_HpReachesZero_SetsStatusDead()
    {
        var run = OpenRoomRun();
        run.PlayerStats.Hp = 1;
        AddEnemy(run, 105, 160);

        Combat.Update(run, Idle, Dt);

        Assert.Equal(0, run.PlayerStats.Hp);
        Assert.Equal(RunStatus.Dead, run.Status);
    }

    [Fact]
    public void Update_WhenNotPlaying_DoesNothing()
    {
        var run = OpenRoomRun();
        run.Status = RunStatus.Dead;

        Combat.Update(run, Shoot(1, 0), Dt);

        Assert.Empty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void SpawnEnemyProjectile_AimsAtTargetWithEnemySpeed()
    {
        var run = OpenRoomRun();
        var ogre = AddEnemy(run, 200, 160, kind: EnemyKind.Ogre);

        Combat.SpawnEnemyProjectile(run, ogre, 100, 160, 1);

        var shot = Assert.Single(run.Entities.OfType<ProjectileEntity>());
        Assert.False(shot.FromPlayer);
        Assert.Equal(-160, shot.Vx, 2);
        Assert.Equal(0, shot.Vy, 2);
        Assert.Equal(400f / 160f, shot.Life, 3);
    }

    [Fact]
    public void SpawnEnemyProjectile_ZeroDistanceOrNoProjectileSpeed_SpawnsNothing()
    {
        var run = OpenRoomRun();
        var bat = AddEnemy(run, 200, 160, kind: EnemyKind.Bat);

        Combat.SpawnEnemyProjectile(run, bat, 200, 160, 1);
        Combat.SpawnEnemyProjectile(run, bat, 100, 160, 1);

        Assert.Empty(run.Entities.OfType<ProjectileEntity>());
    }
}
