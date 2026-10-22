using System;
using System.Linq;
using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Ai
{
    public const float PspamSplitMs = 4000f;
    public const float WanderMinMs = 500f;
    public const float WanderMaxMs = 1500f;
    private const float PspamSplitOffsetPx = 8f;
    private const float PspamNoSplitCooldownMs = 999_999f;

    /// <summary>Dano de contato fica em Combat.Update. AI por kind nas tasks 4–6.</summary>
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
                case EnemyKind.Pspam:
                    UpdatePspam(state, enemy, dt, random);
                    break;
                case EnemyKind.Trojan:
                case EnemyKind.AdwareKing:
                    break;
            }
        }
    }

    private static void UpdatePspam(RunState state, EnemyEntity enemy, float dt, Func<float> rng)
    {
        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown <= 0)
        {
            if (enemy.CanSplit && enemy.Hp > 0)
            {
                SplitPspam(state, enemy);
                return;
            }
            if (!enemy.CanSplit)
                enemy.Cooldown = PspamNoSplitCooldownMs;
        }

        UpdatePspamWander(state, enemy, dt, rng);
    }

    private static void SplitPspam(RunState state, EnemyEntity parent)
    {
        var def = Enemies.Defs[EnemyKind.Pspam];
        var childRadius = def.Radius * 0.7f;
        var px = parent.X;
        var py = parent.Y;
        state.Entities.Remove(parent);

        SpawnChild(-PspamSplitOffsetPx);
        SpawnChild(PspamSplitOffsetPx);

        void SpawnChild(float offsetX)
        {
            state.Entities.Add(new EnemyEntity
            {
                Id = $"{parent.Id}-child-{offsetX}",
                Enemy = EnemyKind.Pspam,
                X = px + offsetX,
                Y = py,
                Radius = childRadius,
                Hp = 1,
                CanSplit = false,
                Cooldown = PspamSplitMs,
            });
        }
    }

    private static void UpdatePspamWander(RunState state, EnemyEntity enemy, float dt, Func<float> rng)
    {
        var speed = Enemies.Defs[EnemyKind.Pspam].Speed;
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
}
