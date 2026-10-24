using System;
using System.Linq;
using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Ai
{
    public const float SlimeSplitMs = 4000f;
    public const float OgrePhase1MaxMs = 8000f;
    public const float WanderMinMs = 500f;
    public const float WanderMaxMs = 1500f;
    public const float SpecterBlinkCooldownMs = 1600f;
    public const float SpecterPhaseMs = 380f;
    public const float SpecterBlinkDistance = 48f;
    private const float SlimeSplitOffsetPx = 8f;
    private const float SlimeNoSplitCooldownMs = 999_999f;

    /// <summary>Dano de contato fica em Combat.Update. Movimenta inimigos por kind (stubs até Tasks 3–6).</summary>
    public static void Update(RunState state, float dt, Func<float>? rng = null)
    {
        if (state.Status != RunStatus.Playing) return;
        var player = state.Entities.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return;
        var random = rng ?? Random.Shared.NextSingle;

        foreach (var enemy in state.Entities.OfType<EnemyEntity>().Where(e => e.Hp > 0).ToList())
        {
            switch (enemy.Enemy)
            {
                case EnemyKind.Bat:
                    UpdateBat(state, enemy, player, dt, rng);
                    break;
                case EnemyKind.Skeleton:
                    UpdateSkeleton(state, enemy, player, dt, rng);
                    break;
                case EnemyKind.Slime:
                    UpdateSlime(state, enemy, dt, random);
                    break;
                case EnemyKind.Specter:
                    UpdateSpecter(state, enemy, player, dt);
                    break;
                case EnemyKind.Ogre:
                    UpdateOgre(state, enemy, player, dt, rng);
                    break;
            }
        }
    }

    private static void UpdateBat(RunState state, EnemyEntity enemy, PlayerEntity player, float dt, Func<float>? rng)
    {
        var def = Enemies.Defs[EnemyKind.Bat];
        var dx = player.X - enemy.X;
        var dy = player.Y - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist == 0) return;
        Steer(state, enemy, dx / dist, dy / dist, def.Speed, dt);
    }

    private static void Steer(RunState state, EnemyEntity enemy, float dirX, float dirY, float speed, float dt)
    {
        enemy.Vx = dirX * speed;
        enemy.Vy = dirY * speed;
        MoveWithTiles(state, enemy, enemy.Vx * dt, enemy.Vy * dt);
    }

    private static void UpdateSkeleton(RunState state, EnemyEntity enemy, PlayerEntity player, float dt, Func<float>? rng)
    {
        var def = Enemies.Defs[EnemyKind.Skeleton];
        var dx = player.X - enemy.X;
        var dy = player.Y - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        var preferred = def.PreferredDist ?? 90f;

        if (dist > 0)
        {
            float dirX, dirY;
            if (dist > preferred + 8f) { dirX = dx / dist; dirY = dy / dist; }
            else if (dist < preferred - 8f) { dirX = -dx / dist; dirY = -dy / dist; }
            else { dirX = 0; dirY = 0; }
            if (dirX != 0 || dirY != 0)
                Steer(state, enemy, dirX, dirY, def.Speed, dt);
        }

        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown > 0) return;
        Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage);
        enemy.Cooldown = def.FireCooldownMs;
    }

    private static void UpdateSlime(RunState state, EnemyEntity enemy, float dt, Func<float> rng)
    {
        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown <= 0)
        {
            if (enemy.CanSplit && enemy.Hp > 0)
            {
                SplitSlime(state, enemy);
                return;
            }
            if (!enemy.CanSplit)
                enemy.Cooldown = SlimeNoSplitCooldownMs;
        }

        UpdateSlimeWander(state, enemy, dt, rng);
    }

    private static void SplitSlime(RunState state, EnemyEntity parent)
    {
        var def = Enemies.Defs[EnemyKind.Slime];
        var childRadius = def.Radius * 0.7f;
        var px = parent.X;
        var py = parent.Y;
        state.Entities.Remove(parent);

        SpawnChild(-SlimeSplitOffsetPx);
        SpawnChild(SlimeSplitOffsetPx);

        void SpawnChild(float offsetX)
        {
            state.Entities.Add(new EnemyEntity
            {
                Id = $"{parent.Id}-child-{offsetX}",
                Enemy = EnemyKind.Slime,
                X = px + offsetX,
                Y = py,
                Radius = childRadius,
                Hp = 1,
                CanSplit = false,
                Cooldown = SlimeSplitMs,
            });
        }
    }

    private static void UpdateSlimeWander(RunState state, EnemyEntity enemy, float dt, Func<float> rng)
    {
        var speed = Enemies.Defs[EnemyKind.Slime].Speed;
        enemy.Phase -= dt * 1000f;
        if (enemy.Phase <= 0)
        {
            var angle = rng() * MathF.PI * 2f;
            enemy.Vx = MathF.Cos(angle) * speed;
            enemy.Vy = MathF.Sin(angle) * speed;
            enemy.Phase = WanderMinMs + rng() * (WanderMaxMs - WanderMinMs);
        }

        var x = enemy.X;
        var y = enemy.Y;
        MoveWithTiles(state, enemy, enemy.Vx * dt, enemy.Vy * dt);
        if (enemy.X == x && enemy.Y == y) enemy.Phase = 0;
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

    private static void UpdateSpecter(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
    {
        var def = Enemies.Defs[EnemyKind.Specter];
        var phasing = state.TimeMs < enemy.IframeUntil;

        enemy.Cooldown -= dt * 1000f;
        if (!phasing && enemy.Cooldown <= 0)
        {
            var dx = player.X - enemy.X;
            var dy = player.Y - enemy.Y;
            var dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist > 1f)
            {
                var step = MathF.Min(SpecterBlinkDistance, dist * 0.65f);
                enemy.X += dx / dist * step;
                enemy.Y += dy / dist * step;
                ClampInsideRoom(enemy);
            }
            enemy.IframeUntil = state.TimeMs + SpecterPhaseMs;
            enemy.Cooldown = SpecterBlinkCooldownMs;
            return;
        }

        if (phasing) return;

        var sx = player.X - enemy.X;
        var sy = player.Y - enemy.Y;
        var sdist = MathF.Sqrt(sx * sx + sy * sy);
        if (sdist == 0) return;
        enemy.Vx = sx / sdist * def.Speed;
        enemy.Vy = sy / sdist * def.Speed;
        enemy.X += enemy.Vx * dt;
        enemy.Y += enemy.Vy * dt;
        ClampInsideRoom(enemy);
    }

    private static void ClampInsideRoom(EnemyEntity enemy)
    {
        var min = GameConstants.Cell + enemy.Radius;
        var maxX = GameConstants.RoomW * GameConstants.Cell - GameConstants.Cell - enemy.Radius;
        var maxY = GameConstants.RoomH * GameConstants.Cell - GameConstants.Cell - enemy.Radius;
        enemy.X = Math.Clamp(enemy.X, min, maxX);
        enemy.Y = Math.Clamp(enemy.Y, min, maxY);
    }

    private static void UpdateOgre(RunState state, EnemyEntity enemy, PlayerEntity player, float dt, Func<float>? rng)
    {
        var def = Enemies.Defs[EnemyKind.Ogre];
        if ((int)enemy.Phase == 0)
        {
            enemy.Cooldown -= dt * 1000f;
            if (enemy.Hp <= def.Hp * 0.5f || enemy.Cooldown <= 0)
            {
                enemy.Phase = 1;
                enemy.Cooldown = def.FireCooldownMs;
            }
            else
            {
                var dx = player.X - enemy.X;
                var dy = player.Y - enemy.Y;
                var dist = MathF.Sqrt(dx * dx + dy * dy);
                if (dist > 0)
                    Steer(state, enemy, dx / dist, dy / dist, def.ChargeSpeed ?? def.Speed, dt);
                return;
            }
        }

        var dx1 = player.X - enemy.X;
        var dy1 = player.Y - enemy.Y;
        var dist1 = MathF.Sqrt(dx1 * dx1 + dy1 * dy1);
        if (dist1 > 0)
            Steer(state, enemy, dx1 / dist1, dy1 / dist1, def.Speed, dt);

        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown > 0) return;
        Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage);
        enemy.Cooldown = def.FireCooldownMs;
    }
}
