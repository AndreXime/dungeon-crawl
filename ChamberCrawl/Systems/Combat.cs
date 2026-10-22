using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Combat
{
    public const float PlayerProjectileSpeed = 300f;
    public const float EnemyProjectileRange = 400f;
    public const float CoinDropChance = 0.5f;
    private const float CoinRadius = 8f;
    private const float TripleShotSpread = 15f * MathF.PI / 180f;

    private static int projectileCounter;

    /// <summary>Não avança TimeMs. O Game1 é responsável por isso.</summary>
    public static void Update(RunState state, InputSnapshot input, float dt, Func<float>? rng = null)
    {
        if (state.Status != RunStatus.Playing) return;
        var player = state.Entities.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return;

        UpdatePlayerShooting(state, player, input, dt);
        UpdateProjectiles(state, player, dt, rng ?? Random.Shared.NextSingle);
        UpdateContactDamage(state, player);

        player.Hp = state.PlayerStats.Hp;
        if (state.PlayerStats.Hp <= 0) state.Status = RunStatus.Dead;
    }

    public static void SpawnEnemyProjectile(RunState state, EnemyEntity from, float towardX, float towardY, float damage)
    {
        var dx = towardX - from.X;
        var dy = towardY - from.Y;
        var length = MathF.Sqrt(dx * dx + dy * dy);
        if (length == 0) return;

        var speed = Enemies.Defs[from.Enemy].ProjectileSpeed;
        if (speed <= 0) return;

        state.Entities.Add(new ProjectileEntity
        {
            Id = NextProjectileId("enemy"),
            FromPlayer = false,
            X = from.X,
            Y = from.Y,
            Vx = dx / length * speed,
            Vy = dy / length * speed,
            Radius = GameConstants.ProjectileRadius,
            Damage = damage,
            Life = EnemyProjectileRange / speed,
        });
    }

    private static string NextProjectileId(string owner) => $"proj-{owner}-{++projectileCounter}";

    private static void UpdatePlayerShooting(RunState state, PlayerEntity player, InputSnapshot input, float dt)
    {
        state.ShootCooldown = MathF.Max(0, state.ShootCooldown - dt);
        if (state.ShootCooldown > 0) return;
        if (input.ShootX == 0 && input.ShootY == 0) return;

        var stats = state.PlayerStats;
        var baseAngle = MathF.Atan2(input.ShootY, input.ShootX);
        var offsets = stats.TripleShot ? new[] { -TripleShotSpread, 0f, TripleShotSpread } : new[] { 0f };

        foreach (var offset in offsets)
        {
            var angle = baseAngle + offset;
            state.Entities.Add(new ProjectileEntity
            {
                Id = NextProjectileId("player"),
                FromPlayer = true,
                X = player.X,
                Y = player.Y,
                Vx = MathF.Cos(angle) * PlayerProjectileSpeed,
                Vy = MathF.Sin(angle) * PlayerProjectileSpeed,
                Radius = GameConstants.ProjectileRadius,
                Damage = stats.Damage,
                Life = stats.Range / PlayerProjectileSpeed,
            });
        }
        state.ShootCooldown = 1f / stats.FireRate;
    }

    private static void DamagePlayer(RunState state, float amount)
    {
        if (state.TimeMs < state.PlayerStats.IframeUntil) return;
        state.PlayerStats.Hp = Math.Max(0, state.PlayerStats.Hp - (int)MathF.Ceiling(amount));
        state.PlayerStats.IframeUntil = state.TimeMs + GameConstants.IframeMs;
    }

    private static void DropCoin(RunState state, EnemyEntity enemy, Func<float> rng)
    {
        if (rng() >= CoinDropChance) return;
        state.Entities.Add(new PickupEntity
        {
            Id = $"coin-{enemy.Id}-{(int)MathF.Floor(state.TimeMs)}",
            Pickup = PickupKind.Coin,
            Amount = state.PlayerStats.CoinMultiplier,
            X = enemy.X,
            Y = enemy.Y,
            Radius = CoinRadius,
        });
    }

    private static void UpdateProjectiles(RunState state, PlayerEntity player, float dt, Func<float> rng)
    {
        var removed = new HashSet<Entity>();
        var projectiles = state.Entities.OfType<ProjectileEntity>().ToList();
        var enemies = state.Entities.OfType<EnemyEntity>().Where(e => e.Hp > 0).ToList();

        foreach (var projectile in projectiles)
        {
            projectile.X += projectile.Vx * dt;
            projectile.Y += projectile.Vy * dt;
            projectile.Life -= dt;

            if (projectile.Life <= 0)
            {
                removed.Add(projectile);
                continue;
            }

            if (Collision.TrySampleSolidTile(
                    projectile.X, projectile.Y, projectile.Radius, state.Room.Tiles,
                    out var hitTx, out var hitTy, out var hitTile))
            {
                if (projectile.FromPlayer && hitTile == GameConstants.TileFolder)
                    state.Room.Tiles[hitTy, hitTx] = GameConstants.TileFloor;
                removed.Add(projectile);
                continue;
            }

            if (projectile.FromPlayer)
            {
                var target = enemies.FirstOrDefault(enemy => enemy.Hp > 0 && Collision.CirclesOverlap(
                    projectile.X, projectile.Y, projectile.Radius, enemy.X, enemy.Y, enemy.Radius));
                if (target is null) continue;

                target.Hp -= projectile.Damage;
                removed.Add(projectile);
                if (target.Hp <= 0)
                {
                    removed.Add(target);
                    DropCoin(state, target, rng);
                }
            }
            else if (Collision.CirclesOverlap(
                projectile.X, projectile.Y, projectile.Radius, player.X, player.Y, player.Radius))
            {
                DamagePlayer(state, projectile.Damage);
                removed.Add(projectile);
            }
        }

        if (removed.Count > 0) state.Entities.RemoveAll(removed.Contains);
    }

    private static void UpdateContactDamage(RunState state, PlayerEntity player)
    {
        foreach (var enemy in state.Entities.OfType<EnemyEntity>())
        {
            if (enemy.Hp <= 0) continue;
            if (Collision.CirclesOverlap(player.X, player.Y, player.Radius, enemy.X, enemy.Y, enemy.Radius))
                DamagePlayer(state, Enemies.Defs[enemy.Enemy].Damage);
        }
    }
}
