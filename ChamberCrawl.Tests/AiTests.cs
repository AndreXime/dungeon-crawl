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
    public void Update_Slime_ChasesWhenPlayerIsClose()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        player.X = 200;
        player.Y = 160;
        var enemy = AddEnemy(run, EnemyKind.Slime, 140, 160);
        enemy.Hp = Enemies.Defs[EnemyKind.Slime].Hp;
        enemy.CanSplit = false;
        enemy.Cooldown = 99999f;
        var before = enemy.X;
        Ai.Update(run, Dt);
        Assert.True(enemy.X > before);
    }

    [Fact]
    public void Update_Slime_ChasesAfterTakingDamageEvenFromFar()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        player.X = 300;
        player.Y = 160;
        var enemy = AddEnemy(run, EnemyKind.Slime, 100, 160);
        enemy.Hp = Enemies.Defs[EnemyKind.Slime].Hp - 1;
        enemy.CanSplit = false;
        enemy.Cooldown = 99999f;
        var before = enemy.X;
        Ai.Update(run, Dt);
        Assert.True(enemy.X > before);
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

    [Fact]
    public void Update_Skeleton_FiresProjectileAfterCooldown()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Skeleton, 200, 160);
        enemy.Cooldown = 0;
        Ai.Update(run, Dt);
        Assert.Single(run.Entities.OfType<ProjectileEntity>());
        Assert.False(run.Entities.OfType<ProjectileEntity>().Single().FromPlayer);
        Assert.Equal(Enemies.Defs[EnemyKind.Skeleton].FireCooldownMs, enemy.Cooldown);
    }

    [Fact]
    public void Update_Skeleton_MovesAwayWhenTooClose()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        player.X = 200;
        player.Y = 160;
        var enemy = AddEnemy(run, EnemyKind.Skeleton, 210, 160);
        enemy.Cooldown = 99999f;
        var before = enemy.X;
        Ai.Update(run, Dt);
        Assert.True(enemy.X > before);
    }

    [Fact]
    public void Update_Ogre_ChasesInPhase0()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Ogre, 100, 160, 24);
        boss.Phase = 0;
        boss.Cooldown = Ai.OgrePhase1MaxMs;
        boss.Hp = Enemies.Defs[EnemyKind.Ogre].Hp;
        run.Entities.OfType<PlayerEntity>().Single().X = 300;
        var before = boss.X;
        Ai.Update(run, Dt);
        Assert.Equal(0, boss.Phase);
        Assert.True(boss.X > before);
    }

    [Fact]
    public void Update_Ogre_EntersPhase1AtHalfHp()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Ogre, 200, 160, 24);
        boss.Phase = 0;
        boss.Cooldown = Ai.OgrePhase1MaxMs;
        boss.Hp = Enemies.Defs[EnemyKind.Ogre].Hp * 0.5f;
        Ai.Update(run, Dt);
        Assert.Equal(1, boss.Phase);
    }

    [Fact]
    public void Update_Ogre_ShootsInPhase1()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Ogre, 200, 160, 24);
        boss.Phase = 1;
        boss.Cooldown = 0;
        Ai.Update(run, Dt);
        Assert.NotEmpty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_Necromancer_ShootsOrbWhenCooldownEnds()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Necromancer, 200, 160, 16);
        boss.Cooldown = 0;
        boss.Phase = 99999f;
        boss.Hp = Enemies.Defs[EnemyKind.Necromancer].Hp;
        Ai.Update(run, Dt);
        Assert.Single(run.Entities.OfType<ProjectileEntity>());
        Assert.False(run.Entities.OfType<ProjectileEntity>().Single().FromPlayer);
    }

    [Fact]
    public void Update_Necromancer_SummonsSkeletonWhenPhaseEnds()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Necromancer, 200, 160, 16);
        boss.Cooldown = 99999f;
        boss.Phase = 0;
        boss.Hp = Enemies.Defs[EnemyKind.Necromancer].Hp;
        Ai.Update(run, Dt);
        var summons = run.Entities.OfType<EnemyEntity>()
            .Where(e => e.Enemy == EnemyKind.Skeleton && e.Hp == Ai.NecromancerMinionHp)
            .ToList();
        Assert.Single(summons);
        Assert.Equal(Ai.NecromancerSummonMs, boss.Phase);
    }

    [Fact]
    public void Update_Necromancer_RespectsMaxMinions()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Necromancer, 200, 160, 16);
        boss.Cooldown = 99999f;
        boss.Phase = 0;
        for (var i = 0; i < Ai.NecromancerMaxMinions; i++)
            AddEnemy(run, EnemyKind.Skeleton, 220 + i * 10, 160);

        Ai.Update(run, Dt);
        Assert.Equal(Ai.NecromancerMaxMinions, run.Entities.OfType<EnemyEntity>()
            .Count(e => e.Enemy == EnemyKind.Skeleton));
    }

    [Fact]
    public void Update_Acolyte_ShootsWhileOrbiting()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Acolyte, 100, 160);
        enemy.Cooldown = 0;
        enemy.Phase = 0;
        run.Entities.OfType<PlayerEntity>().Single().X = 200;
        Ai.Update(run, Dt);
        Assert.Single(run.Entities.OfType<ProjectileEntity>());
        Assert.True(enemy.Phase > 0);
    }

    [Fact]
    public void Update_Acolyte_DoesNotShootThroughObstacle()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        player.X = 240;
        player.Y = 176;
        var enemy = AddEnemy(run, EnemyKind.Acolyte, 100, 176);
        enemy.Cooldown = 0;
        // Barril exatamente entre o acólito e o jogador.
        run.Room.Tiles[5, 5] = GameConstants.TileBarrel;

        Ai.Update(run, Dt);

        Assert.Empty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_Acolyte_StrafesUntilClearShotAroundObstacle()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        player.X = 240;
        player.Y = 176;
        var enemy = AddEnemy(run, EnemyKind.Acolyte, 100, 176);
        enemy.Cooldown = 0;
        run.Room.Tiles[5, 5] = GameConstants.TileBarrel;

        var startY = enemy.Y;
        for (var i = 0; i < 90; i++)
        {
            enemy.Cooldown = 0;
            Ai.Update(run, Dt);
            if (run.Entities.OfType<ProjectileEntity>().Any())
                break;
        }

        Assert.True(MathF.Abs(enemy.Y - startY) > 6f, $"esperado desvio lateral, dy={enemy.Y - startY}");
        Assert.True(
            Collision.HasLineOfSight(
                enemy.X, enemy.Y, player.X, player.Y,
                GameConstants.ProjectileRadius, run.Room.Tiles),
            "esperado linha de visão limpa após contornar");
        Assert.NotEmpty(run.Entities.OfType<ProjectileEntity>());
    }

    [Fact]
    public void Update_RuneLord_SummonsAcolyteWhenPhaseEnds()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.RuneLord, 200, 160, 18);
        boss.Cooldown = 99999f;
        boss.Phase = 0;
        boss.Hp = Enemies.Defs[EnemyKind.RuneLord].Hp;
        Ai.Update(run, Dt);
        Assert.Contains(run.Entities.OfType<EnemyEntity>(), e => e.Enemy == EnemyKind.Acolyte);
        Assert.Equal(Ai.RuneLordSummonMs, boss.Phase);
    }

    [Fact]
    public void Combat_RuneDamagesPlayerStandingOnIt()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        var tx = (int)(player.X / GameConstants.Cell);
        var ty = (int)(player.Y / GameConstants.Cell);
        run.Room.Tiles[ty, tx] = GameConstants.TileRune;
        run.PlayerStats.Hp = 6;
        run.PlayerStats.IframeUntil = 0;
        run.TimeMs = 100;
        Combat.Update(run, new InputSnapshot(), Dt);
        Assert.Equal(5, run.PlayerStats.Hp);
    }

    [Fact]
    public void Update_Guard_ChasesPlayerSlowly()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Guard, 100, 160, 12);
        run.Entities.OfType<PlayerEntity>().Single().X = 300;
        var before = enemy.X;
        Ai.Update(run, Dt);
        Assert.True(enemy.X > before);
        Assert.True(enemy.X - before < Enemies.Defs[EnemyKind.Rat].Speed * Dt);
    }

    [Fact]
    public void Update_IronGolem_ShootsInPhase0()
    {
        var run = OpenRoomRun();
        run.TimeMs = 100;
        var boss = AddEnemy(run, EnemyKind.IronGolem, 200, 160, 22);
        boss.Phase = 0;
        boss.Cooldown = 0;
        boss.IframeUntil = 0;
        boss.Hp = Enemies.Defs[EnemyKind.IronGolem].Hp;
        Ai.Update(run, Dt);
        Assert.Single(run.Entities.OfType<ProjectileEntity>());
        Assert.Equal(0, (int)boss.Phase);
        Assert.True(boss.IframeUntil > run.TimeMs);
    }

    [Fact]
    public void Update_IronGolem_EntersChargeAfterPhase0Timer()
    {
        var run = OpenRoomRun();
        run.TimeMs = 5000;
        var boss = AddEnemy(run, EnemyKind.IronGolem, 200, 160, 22);
        boss.Phase = 0;
        boss.Cooldown = 99999f;
        boss.IframeUntil = 4000;
        boss.Hp = Enemies.Defs[EnemyKind.IronGolem].Hp;
        Ai.Update(run, Dt);
        Assert.Equal(1, (int)boss.Phase);
        Assert.Equal(Ai.IronGolemChargeMs, boss.Cooldown);
    }

    [Fact]
    public void Update_IronGolem_EntersVulnerableAtHalfHp()
    {
        var run = OpenRoomRun();
        run.TimeMs = 100;
        var boss = AddEnemy(run, EnemyKind.IronGolem, 200, 160, 22);
        boss.Phase = 0;
        boss.Cooldown = 99999f;
        boss.IframeUntil = 10_000;
        boss.Hp = Enemies.Defs[EnemyKind.IronGolem].Hp * 0.5f;
        Ai.Update(run, Dt);
        Assert.Equal(2, (int)boss.Phase);
        Assert.Equal(Ai.IronGolemVulnerableMs, boss.Cooldown);
    }

    [Fact]
    public void Combat_IronGolem_TakesDoubleDamageWhenVulnerable()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.IronGolem, 200, 160, 22);
        boss.Phase = 2;
        boss.Hp = 20;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "hit", FromPlayer = true, X = 200, Y = 160, Vx = 0, Vy = 0,
            Radius = 4, Damage = 5, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), Dt);
        Assert.Equal(10, boss.Hp);
    }

    [Fact]
    public void Update_Rat_ChasesPlayer()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Rat, 100, 160);
        run.Entities.OfType<PlayerEntity>().Single().X = 300;
        var before = enemy.X;
        Ai.Update(run, Dt);
        Assert.True(enemy.X > before);
    }

    [Fact]
    public void Update_SlimeKing_SpitsWhenCooldownEnds()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.SlimeKing, 200, 160, 20);
        boss.Cooldown = 0;
        boss.CanSplit = true;
        Ai.Update(run, Dt);
        Assert.Single(run.Entities.OfType<ProjectileEntity>());
        Assert.False(run.Entities.OfType<ProjectileEntity>().Single().FromPlayer);
    }

    [Fact]
    public void Combat_SlimeKing_SplitsIntoTwoMediumSlimesOnDeath()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.SlimeKing, 200, 160, 20);
        boss.Hp = 1;
        boss.CanSplit = true;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "hit", FromPlayer = true, X = 200, Y = 160, Vx = 0, Vy = 0,
            Radius = 4, Damage = 5, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), Dt);
        Assert.DoesNotContain(run.Entities.OfType<EnemyEntity>(), e => e.Enemy == EnemyKind.SlimeKing);
        var heirs = run.Entities.OfType<EnemyEntity>().Where(e => e.Enemy == EnemyKind.Slime).ToList();
        Assert.Equal(2, heirs.Count);
        Assert.All(heirs, h =>
        {
            Assert.False(h.CanSplit);
            Assert.Equal(Ai.SlimeKingChildHp, h.Hp);
        });
    }

    [Fact]
    public void Update_Specter_FloatsTowardPlayer()
    {
        var run = OpenRoomRun();
        var enemy = AddEnemy(run, EnemyKind.Specter, 100, 160);
        enemy.Cooldown = 99999f;
        run.Entities.OfType<PlayerEntity>().Single().X = 300;
        var before = enemy.X;
        Ai.Update(run, Dt);
        Assert.True(enemy.X > before);
    }

    [Fact]
    public void Update_Specter_BacksOffWhenTooClose()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        player.X = 200;
        player.Y = 160;
        var enemy = AddEnemy(run, EnemyKind.Specter, 210, 160);
        enemy.Cooldown = 99999f;
        var before = enemy.X;
        Ai.Update(run, Dt);
        Assert.True(enemy.X > before);
    }

    [Fact]
    public void Update_Specter_BlinksTowardPlayerWhenCooldownEnds()
    {
        var run = OpenRoomRun();
        run.TimeMs = 1000f;
        var enemy = AddEnemy(run, EnemyKind.Specter, 100, 160);
        enemy.Cooldown = 0;
        enemy.IframeUntil = 0;
        run.Entities.OfType<PlayerEntity>().Single().X = 300;
        var before = enemy.X;
        Ai.Update(run, Dt);
        Assert.True(enemy.X > before + 10f);
        var dist = MathF.Abs(300f - enemy.X);
        Assert.True(dist >= Ai.SpecterPreferredDist - 1f);
        Assert.Equal(Ai.SpecterBlinkCooldownMs, enemy.Cooldown);
        Assert.Equal(run.TimeMs + Ai.SpecterPhaseMs, enemy.IframeUntil);
    }

    [Fact]
    public void Combat_IgnoresDamageWhileEnemyIframes()
    {
        var run = OpenRoomRun();
        var boss = AddEnemy(run, EnemyKind.Ogre, 200, 160, 24);
        boss.Hp = Enemies.Defs[EnemyKind.Ogre].Hp;
        boss.IframeUntil = 10_000;
        run.TimeMs = 100;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "hit", FromPlayer = true, X = 200, Y = 160, Vx = 0, Vy = 0,
            Radius = 4, Damage = 5, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), Dt);
        Assert.Equal(Enemies.Defs[EnemyKind.Ogre].Hp, boss.Hp);
    }

    [Fact]
    public void Update_Bat_StrafesAroundBlockingObstacle()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        player.X = 300;
        player.Y = 160;
        var enemy = AddEnemy(run, EnemyKind.Bat, 100, 160);
        // Coluna de obstáculos bloqueando a linha reta até o jogador.
        for (var y = 3; y <= 7; y++)
            run.Room.Tiles[y, 5] = GameConstants.TileCrate;

        var startX = enemy.X;
        var startY = enemy.Y;
        for (var i = 0; i < 120; i++)
            Ai.Update(run, Dt);

        Assert.True(enemy.X > startX);
        Assert.True(MathF.Abs(enemy.Y - startY) > 8f, $"esperado desvio lateral, dy={enemy.Y - startY}");
        Assert.True(enemy.X > 5.5f * GameConstants.Cell, $"esperado passar a coluna, x={enemy.X}");
    }

    [Fact]
    public void Update_Bat_StrafesAroundWideWall()
    {
        var run = OpenRoomRun();
        var player = run.Entities.OfType<PlayerEntity>().Single();
        player.X = 380;
        player.Y = 176;
        var enemy = AddEnemy(run, EnemyKind.Bat, 80, 176);
        // Parede larga (2 colunas) cobrindo quase toda a altura útil.
        for (var y = 2; y <= 8; y++)
        {
            run.Room.Tiles[y, 5] = GameConstants.TileCrate;
            run.Room.Tiles[y, 6] = GameConstants.TileCrate;
        }

        for (var i = 0; i < 200; i++)
            Ai.Update(run, Dt);

        Assert.True(enemy.X > 7f * GameConstants.Cell, $"esperado contornar parede larga, x={enemy.X}");
    }
}
