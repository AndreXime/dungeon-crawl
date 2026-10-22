#nullable enable
using System;
using System.Linq;
using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Ai
{
    public const float WanderMinMs = 500f;
    public const float WanderMaxMs = 1500f;
    public const float BossChargeS = 2f;
    public const float BossBurstS = 1f;
    private const float ShooterApproachFactor = 0.3f;
    private const float ShooterDeadzone = 12f;
    private const int BossBurstDirections = 8;

    private readonly record struct Aim(float DirX, float DirY, float Dist);

    /// <summary>Dano de contato fica em Combat.Update.</summary>
    public static void Update(RunState state, float dt, Func<float>? rng = null)
    {
        if (state.Status != RunStatus.Playing) return;
        var player = state.Entities.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return;
        var random = rng ?? Random.Shared.NextSingle;

        foreach (var enemy in state.Entities.OfType<EnemyEntity>().Where(e => e.Hp > 0).ToList())
        {
            var dx = player.X - enemy.X;
            var dy = player.Y - enemy.Y;
            var dist = MathF.Sqrt(dx * dx + dy * dy);
            var aim = dist == 0 ? new Aim(0, 0, 0) : new Aim(dx / dist, dy / dist, dist);

            switch (enemy.Enemy)
            {
                case EnemyKind.Chaser:
                    Steer(state, enemy, aim.DirX, aim.DirY, Enemies.Defs[EnemyKind.Chaser].Speed, dt);
                    break;
                case EnemyKind.Wander:
                    UpdateWander(state, enemy, dt, random);
                    break;
                case EnemyKind.Shooter:
                    UpdateShooter(state, enemy, player, aim, dt);
                    break;
                case EnemyKind.Boss:
                    UpdateBoss(state, enemy, aim, dt);
                    break;
            }
        }
    }

    private static void MoveWithTiles(RunState state, EnemyEntity enemy, float dx, float dy)
    {
        var tiles = state.Room.Tiles;
        var nextX = enemy.X + dx;
        var nextY = enemy.Y + dy;

        if (!Collision.CircleHitsSolid(nextX, nextY, enemy.Radius, tiles))
        {
            enemy.X = nextX;
            enemy.Y = nextY;
            return;
        }
        if (!Collision.CircleHitsSolid(nextX, enemy.Y, enemy.Radius, tiles)) enemy.X = nextX;
        else enemy.Vx = 0;
        if (!Collision.CircleHitsSolid(enemy.X, nextY, enemy.Radius, tiles)) enemy.Y = nextY;
        else enemy.Vy = 0;
    }

    private static void Steer(RunState state, EnemyEntity enemy, float dirX, float dirY, float speed, float dt)
    {
        enemy.Vx = dirX * speed;
        enemy.Vy = dirY * speed;
        MoveWithTiles(state, enemy, enemy.Vx * dt, enemy.Vy * dt);
    }

    private static void UpdateWander(RunState state, EnemyEntity enemy, float dt, Func<float> rng)
    {
        var speed = Enemies.Defs[EnemyKind.Wander].Speed;
        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown <= 0)
        {
            var angle = rng() * MathF.PI * 2f;
            enemy.Vx = MathF.Cos(angle) * speed;
            enemy.Vy = MathF.Sin(angle) * speed;
            enemy.Cooldown = WanderMinMs + rng() * (WanderMaxMs - WanderMinMs);
        }
        var x = enemy.X;
        var y = enemy.Y;
        MoveWithTiles(state, enemy, enemy.Vx * dt, enemy.Vy * dt);
        // Bateu na parede: sorteia nova direção no próximo tick.
        if (enemy.X == x && enemy.Y == y) enemy.Cooldown = 0;
    }

    private static void UpdateShooter(RunState state, EnemyEntity enemy, PlayerEntity player, Aim aim, float dt)
    {
        var def = Enemies.Defs[EnemyKind.Shooter];
        var preferred = def.PreferredDist ?? 0f;

        if (aim.Dist < preferred - ShooterDeadzone)
            Steer(state, enemy, -aim.DirX, -aim.DirY, def.Speed, dt);
        else if (aim.Dist > preferred + ShooterDeadzone)
            Steer(state, enemy, aim.DirX, aim.DirY, def.Speed * ShooterApproachFactor, dt);
        else
        {
            enemy.Vx = 0;
            enemy.Vy = 0;
        }

        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown <= 0)
        {
            Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage);
            enemy.Cooldown = def.FireCooldownMs;
        }
    }

    private static void FireBossBurst(RunState state, EnemyEntity enemy)
    {
        var damage = Enemies.Defs[EnemyKind.Boss].Damage;
        for (var i = 0; i < BossBurstDirections; i++)
        {
            var angle = (float)i / BossBurstDirections * MathF.PI * 2f;
            Combat.SpawnEnemyProjectile(
                state, enemy, enemy.X + MathF.Cos(angle), enemy.Y + MathF.Sin(angle), damage);
        }
    }

    /// <summary>Phase é o tempo (s) dentro do ciclo: charge, depois rajada radial.</summary>
    private static void UpdateBoss(RunState state, EnemyEntity enemy, Aim aim, float dt)
    {
        var def = Enemies.Defs[EnemyKind.Boss];
        var before = enemy.Phase;
        var phase = before + dt;
        const float cycle = BossChargeS + BossBurstS;
        if (phase >= cycle) phase -= cycle;
        enemy.Phase = phase;

        if (before < BossChargeS && phase >= BossChargeS) FireBossBurst(state, enemy);

        if (phase < BossChargeS)
            Steer(state, enemy, aim.DirX, aim.DirY, def.ChargeSpeed ?? def.Speed, dt);
        else
        {
            enemy.Vx = 0;
            enemy.Vy = 0;
        }
    }
}
