using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Ai
{
    public const float PspamSplitMs = 4000f;
    public const float TrojanWakeRadius = 80f;
    public const float WanderMinMs = 500f;
    public const float WanderMaxMs = 1500f;
    public const float AdwareIdleMs = 800f;
    public const float AdwareButtonsMs = 1200f;
    public const float AdwareSpamMs = 200f;
    public const float AdwareResizeMs = 3000f;
    public const float AdwareShockRadius = 48f;
    public const int AdwareBurstCount = 12;
    private const float PspamSplitOffsetPx = 8f;
    private const float PspamNoSplitCooldownMs = 999_999f;
    private const int AdwareSpamSpawnCount = 3;

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
                    UpdateTrojan(state, enemy, player, dt);
                    break;
                case EnemyKind.AdwareKing:
                    UpdateAdwareKing(state, enemy, player, dt, random);
                    break;
            }
        }
    }

    private static void UpdateAdwareKing(
        RunState state, EnemyEntity enemy, PlayerEntity player, float dt, Func<float> rng)
    {
        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown <= 0)
        {
            var leaving = (int)enemy.Phase;
            if (leaving == 5)
                TryAdwareShockwave(state, enemy, player);

            var next = (leaving + 1) % 6;
            enemy.Phase = next;
            enemy.Cooldown = AdwarePhaseDuration(next);
            EnterAdwarePhase(state, enemy, next, rng);
        }

        if ((int)enemy.Phase == 5)
            MoveWithTiles(state, enemy, enemy.Vx * dt, enemy.Vy * dt);
    }

    private static float AdwarePhaseDuration(int phase) => phase switch
    {
        1 => AdwareButtonsMs,
        3 => AdwareSpamMs,
        5 => AdwareResizeMs,
        _ => AdwareIdleMs,
    };

    private static void EnterAdwarePhase(RunState state, EnemyEntity enemy, int phase, Func<float> rng)
    {
        enemy.Vx = 0;
        enemy.Vy = 0;

        switch (phase)
        {
            case 1:
                FireAdwareButtonBurst(state, enemy);
                break;
            case 3:
                SpawnAdwarePspams(state, enemy, rng);
                break;
            case 5:
                BeginAdwareResize(state, enemy, rng);
                break;
        }
    }

    private static void FireAdwareButtonBurst(RunState state, EnemyEntity enemy)
    {
        var damage = Enemies.Defs[EnemyKind.AdwareKing].Damage;
        for (var i = 0; i < AdwareBurstCount; i++)
        {
            var angle = i * MathF.PI * 2f / AdwareBurstCount;
            Combat.SpawnEnemyProjectile(
                state, enemy,
                enemy.X + MathF.Cos(angle),
                enemy.Y + MathF.Sin(angle),
                damage);
        }
    }

    private static void SpawnAdwarePspams(RunState state, EnemyEntity enemy, Func<float> rng)
    {
        var def = Enemies.Defs[EnemyKind.Pspam];
        var tiles = FreeFloorTiles(state);
        var spawnCount = Math.Min(AdwareSpamSpawnCount, tiles.Count);
        for (var i = 0; i < spawnCount; i++)
        {
            var pick = (int)MathF.Floor(rng() * tiles.Count);
            if (pick >= tiles.Count) pick = tiles.Count - 1;
            var (tx, ty) = tiles[pick];
            tiles.RemoveAt(pick);
            state.Entities.Add(new EnemyEntity
            {
                Id = $"{enemy.Id}-spam-{i}-{(int)state.TimeMs}",
                Enemy = EnemyKind.Pspam,
                X = (tx + 0.5f) * GameConstants.Cell,
                Y = (ty + 0.5f) * GameConstants.Cell,
                Radius = def.Radius,
                Hp = 1,
                CanSplit = false,
                Cooldown = PspamNoSplitCooldownMs,
            });
        }
    }

    private static List<(int Tx, int Ty)> FreeFloorTiles(RunState state)
    {
        var occupied = new HashSet<(int, int)>();
        foreach (var entity in state.Entities)
        {
            var tx = (int)MathF.Floor(entity.X / GameConstants.Cell);
            var ty = (int)MathF.Floor(entity.Y / GameConstants.Cell);
            occupied.Add((tx, ty));
        }

        var free = new List<(int Tx, int Ty)>();
        var tiles = state.Room.Tiles;
        for (var ty = 0; ty < GameConstants.RoomH; ty++)
        {
            for (var tx = 0; tx < GameConstants.RoomW; tx++)
            {
                if (tiles[ty, tx] != GameConstants.TileFloor) continue;
                if (occupied.Contains((tx, ty))) continue;
                free.Add((tx, ty));
            }
        }
        return free;
    }

    private static void BeginAdwareResize(RunState state, EnemyEntity enemy, Func<float> rng)
    {
        enemy.IframeUntil = state.TimeMs + AdwareResizeMs;
        var corners = new[]
        {
            (1, 1),
            (GameConstants.RoomW - 2, 1),
            (1, GameConstants.RoomH - 2),
            (GameConstants.RoomW - 2, GameConstants.RoomH - 2),
        };
        var pick = (int)MathF.Floor(rng() * corners.Length);
        if (pick >= corners.Length) pick = corners.Length - 1;
        var (tx, ty) = corners[pick];
        var targetX = (tx + 0.5f) * GameConstants.Cell;
        var targetY = (ty + 0.5f) * GameConstants.Cell;
        var dx = targetX - enemy.X;
        var dy = targetY - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        var speed = Enemies.Defs[EnemyKind.AdwareKing].ChargeSpeed
            ?? Enemies.Defs[EnemyKind.AdwareKing].Speed;
        if (dist > 0)
        {
            enemy.Vx = dx / dist * speed;
            enemy.Vy = dy / dist * speed;
        }
    }

    private static void TryAdwareShockwave(RunState state, EnemyEntity enemy, PlayerEntity player)
    {
        if (!Collision.CirclesOverlap(
                enemy.X, enemy.Y, AdwareShockRadius, player.X, player.Y, player.Radius))
            return;
        Combat.DamagePlayer(state, Enemies.Defs[EnemyKind.AdwareKing].Damage);
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

    private static void UpdateTrojan(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
    {
        var def = Enemies.Defs[EnemyKind.Trojan];
        var dx = player.X - enemy.X;
        var dy = player.Y - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);

        if (enemy.Phase == 0)
        {
            if (dist > 0 && dist <= TrojanWakeRadius)
            {
                enemy.Phase = 1;
                var dirX = dx / dist;
                var dirY = dy / dist;
                var charge = def.ChargeSpeed ?? def.Speed;
                enemy.Vx = dirX * charge;
                enemy.Vy = dirY * charge;
                MoveWithTiles(state, enemy, enemy.Vx * dt, enemy.Vy * dt);
            }

            return;
        }

        if (dist == 0) return;
        Steer(state, enemy, dx / dist, dy / dist, def.Speed, dt);
    }

    private static void Steer(RunState state, EnemyEntity enemy, float dirX, float dirY, float speed, float dt)
    {
        enemy.Vx = dirX * speed;
        enemy.Vy = dirY * speed;
        MoveWithTiles(state, enemy, enemy.Vx * dt, enemy.Vy * dt);
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
