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
    public const float SpecterPreferredDist = 42f;
    public const float NecromancerSummonMs = 3200f;
    public const float NecromancerSummonEnragedMs = 2000f;
    public const int NecromancerMaxMinions = 3;
    public const float NecromancerMinionHp = 1f;
    public const float SlimeKingChildHp = 3f;
    public const float SlimeKingChildRadiusScale = 1.15f;
    public const float IronGolemPhase0Ms = 6000f;
    public const float IronGolemChargeMs = 2200f;
    public const float IronGolemVulnerableMs = 2800f;
    public const float AcolyteOrbitSpeed = 1.6f;
    public const float RuneLordSummonMs = 3600f;
    public const float RuneLordSummonEnragedMs = 2200f;
    public const int RuneLordMaxMinions = 3;
    public const float RuneLordMinionHp = 2f;
    private const float SlimeKingSplitOffsetPx = 14f;
    private const float SlimeSplitOffsetPx = 8f;
    private const float SlimeNoSplitCooldownMs = 999_999f;
    private static int summonSerial;

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
                case EnemyKind.Rat:
                    UpdateRat(state, enemy, player, dt);
                    break;
                case EnemyKind.Guard:
                    UpdateGuard(state, enemy, player, dt);
                    break;
                case EnemyKind.Acolyte:
                    UpdateAcolyte(state, enemy, player, dt);
                    break;
                case EnemyKind.Ogre:
                    UpdateOgre(state, enemy, player, dt, rng);
                    break;
                case EnemyKind.Necromancer:
                    UpdateNecromancer(state, enemy, player, dt);
                    break;
                case EnemyKind.SlimeKing:
                    UpdateSlimeKing(state, enemy, player, dt);
                    break;
                case EnemyKind.IronGolem:
                    UpdateIronGolem(state, enemy, player, dt);
                    break;
                case EnemyKind.RuneLord:
                    UpdateRuneLord(state, enemy, player, dt);
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
        var scaled = speed * Movement.SpeedScale(enemy.X, enemy.Y, state.Room.Tiles, enemy.Enemy);
        enemy.Vx = dirX * scaled;
        enemy.Vy = dirY * scaled;
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
        var speed = Enemies.Defs[EnemyKind.Slime].Speed
            * Movement.SpeedScale(enemy.X, enemy.Y, state.Room.Tiles, enemy.Enemy);
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

    private static void UpdateRat(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
    {
        var def = Enemies.Defs[EnemyKind.Rat];
        var dx = player.X - enemy.X;
        var dy = player.Y - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist == 0) return;
        Steer(state, enemy, dx / dist, dy / dist, def.Speed, dt);
    }

    private static void UpdateGuard(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
    {
        var def = Enemies.Defs[EnemyKind.Guard];
        var dx = player.X - enemy.X;
        var dy = player.Y - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist == 0) return;
        Steer(state, enemy, dx / dist, dy / dist, def.Speed, dt);
    }

    private static void UpdateAcolyte(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
    {
        var def = Enemies.Defs[EnemyKind.Acolyte];
        var orbit = def.PreferredDist ?? 70f;
        enemy.Phase += dt * AcolyteOrbitSpeed;
        var targetX = player.X + MathF.Cos(enemy.Phase) * orbit;
        var targetY = player.Y + MathF.Sin(enemy.Phase) * orbit;
        var dx = targetX - enemy.X;
        var dy = targetY - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist > 1f)
            Steer(state, enemy, dx / dist, dy / dist, def.Speed, dt);

        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown > 0) return;
        Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage);
        enemy.Cooldown = def.FireCooldownMs;
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
            // Para perto o bastante para ameacar, sem pousar em cima do jogador (evita iframe eterno).
            if (dist > SpecterPreferredDist)
            {
                var step = MathF.Min(SpecterBlinkDistance, dist - SpecterPreferredDist);
                enemy.X += dx / dist * step;
                enemy.Y += dy / dist * step;
                ClampInsideRoom(enemy);
            }
            if (Collision.CirclesOverlap(
                    enemy.X, enemy.Y, enemy.Radius + 6f, player.X, player.Y, player.Radius))
                Combat.DamagePlayer(state, def.Damage);
            enemy.IframeUntil = state.TimeMs + SpecterPhaseMs;
            enemy.Cooldown = SpecterBlinkCooldownMs;
            return;
        }

        if (phasing) return;

        var sx = player.X - enemy.X;
        var sy = player.Y - enemy.Y;
        var sdist = MathF.Sqrt(sx * sx + sy * sy);
        if (sdist == 0) return;
        var speed = def.Speed * Movement.SpeedScale(enemy.X, enemy.Y, state.Room.Tiles, enemy.Enemy);
        float dirX, dirY;
        if (sdist > SpecterPreferredDist + 8f) { dirX = sx / sdist; dirY = sy / sdist; }
        else if (sdist < SpecterPreferredDist - 8f) { dirX = -sx / sdist; dirY = -sy / sdist; }
        else return;
        enemy.Vx = dirX * speed;
        enemy.Vy = dirY * speed;
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

    private static void UpdateNecromancer(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
    {
        var def = Enemies.Defs[EnemyKind.Necromancer];
        var enraged = enemy.Hp <= def.Hp * 0.5f;
        var preferred = def.PreferredDist ?? 110f;

        var dx = player.X - enemy.X;
        var dy = player.Y - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist > 0)
        {
            float dirX, dirY;
            if (dist > preferred + 10f) { dirX = dx / dist; dirY = dy / dist; }
            else if (dist < preferred - 10f) { dirX = -dx / dist; dirY = -dy / dist; }
            else { dirX = 0; dirY = 0; }
            if (dirX != 0 || dirY != 0)
                Steer(state, enemy, dirX, dirY, def.Speed, dt);
        }

        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown <= 0)
        {
            Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, Math.Max(1, def.Damage + (enraged ? 1 : 0)));
            enemy.Cooldown = enraged ? def.FireCooldownMs * 0.7f : def.FireCooldownMs;
        }

        enemy.Phase -= dt * 1000f;
        if (enemy.Phase > 0) return;

        enemy.Phase = enraged ? NecromancerSummonEnragedMs : NecromancerSummonMs;
        TrySummonMinion(state, enemy);
        if (enraged) TrySummonMinion(state, enemy);
    }

    private static void TrySummonMinion(RunState state, EnemyEntity boss)
    {
        var minions = state.Entities.OfType<EnemyEntity>()
            .Count(e => e.Hp > 0 && e.Enemy is EnemyKind.Skeleton or EnemyKind.Specter or EnemyKind.Bat);
        if (minions >= NecromancerMaxMinions) return;

        var skel = Enemies.Defs[EnemyKind.Skeleton];
        var angle = (++summonSerial % 8) * (MathF.PI / 4f);
        var ox = MathF.Cos(angle) * GameConstants.Cell * 1.5f;
        var oy = MathF.Sin(angle) * GameConstants.Cell * 1.5f;
        var minion = new EnemyEntity
        {
            Id = $"{boss.Id}-summon-{summonSerial}",
            Enemy = EnemyKind.Skeleton,
            X = boss.X + ox,
            Y = boss.Y + oy,
            Radius = skel.Radius * 0.85f,
            Hp = NecromancerMinionHp,
            Cooldown = skel.FireCooldownMs,
            CanSplit = false,
        };
        ClampInsideRoom(minion);
        state.Entities.Add(minion);
    }

    private static void UpdateSlimeKing(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
    {
        var def = Enemies.Defs[EnemyKind.SlimeKing];
        var preferred = def.PreferredDist ?? 80f;
        var dx = player.X - enemy.X;
        var dy = player.Y - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist > 0)
        {
            float dirX, dirY;
            if (dist > preferred + 10f) { dirX = dx / dist; dirY = dy / dist; }
            else if (dist < preferred - 10f) { dirX = -dx / dist; dirY = -dy / dist; }
            else { dirX = 0; dirY = 0; }
            if (dirX != 0 || dirY != 0)
                Steer(state, enemy, dirX, dirY, def.Speed, dt);
        }

        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown > 0) return;
        Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage);
        enemy.Cooldown = def.FireCooldownMs;
    }

    private static void UpdateIronGolem(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
    {
        var def = Enemies.Defs[EnemyKind.IronGolem];
        var mode = (int)enemy.Phase;

        if (mode == 0)
        {
            if (enemy.IframeUntil <= 0f)
                enemy.IframeUntil = state.TimeMs + IronGolemPhase0Ms;

            KeepPreferredDistance(state, enemy, player, def.PreferredDist ?? 100f, def.Speed, dt);

            enemy.Cooldown -= dt * 1000f;
            if (enemy.Cooldown <= 0)
            {
                Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage);
                enemy.Cooldown = def.FireCooldownMs;
            }

            if (state.TimeMs >= enemy.IframeUntil || enemy.Hp <= def.Hp * 0.5f)
            {
                enemy.Phase = enemy.Hp <= def.Hp * 0.5f ? 2 : 1;
                enemy.Cooldown = enemy.Phase >= 2 ? IronGolemVulnerableMs : IronGolemChargeMs;
                enemy.IframeUntil = 0;
            }
            return;
        }

        if (mode == 1)
        {
            var dx = player.X - enemy.X;
            var dy = player.Y - enemy.Y;
            var dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist > 0)
                Steer(state, enemy, dx / dist, dy / dist, def.ChargeSpeed ?? def.Speed, dt);

            enemy.Cooldown -= dt * 1000f;
            if (enemy.Cooldown > 0) return;

            if (enemy.Hp <= def.Hp * 0.5f)
            {
                enemy.Phase = 2;
                enemy.Cooldown = IronGolemVulnerableMs;
            }
            else
            {
                enemy.Phase = 0;
                enemy.Cooldown = def.FireCooldownMs;
                enemy.IframeUntil = state.TimeMs + IronGolemPhase0Ms;
            }
            return;
        }

        enemy.Vx = 0;
        enemy.Vy = 0;
        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown > 0) return;
        enemy.Phase = 1;
        enemy.Cooldown = IronGolemChargeMs * 0.65f;
    }

    private static void KeepPreferredDistance(
        RunState state, EnemyEntity enemy, PlayerEntity player, float preferred, float speed, float dt)
    {
        var dx = player.X - enemy.X;
        var dy = player.Y - enemy.Y;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist == 0) return;
        float dirX, dirY;
        if (dist > preferred + 10f) { dirX = dx / dist; dirY = dy / dist; }
        else if (dist < preferred - 10f) { dirX = -dx / dist; dirY = -dy / dist; }
        else { dirX = 0; dirY = 0; }
        if (dirX != 0 || dirY != 0)
            Steer(state, enemy, dirX, dirY, speed, dt);
    }

    private static void UpdateRuneLord(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
    {
        var def = Enemies.Defs[EnemyKind.RuneLord];
        var enraged = enemy.Hp <= def.Hp * 0.5f;
        KeepPreferredDistance(state, enemy, player, def.PreferredDist ?? 105f, def.Speed, dt);

        enemy.Cooldown -= dt * 1000f;
        if (enemy.Cooldown <= 0)
        {
            Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage + (enraged ? 1 : 0));
            if (enraged)
                Combat.SpawnEnemyProjectile(state, enemy, player.X + 24f, player.Y - 18f, def.Damage);
            enemy.Cooldown = enraged ? def.FireCooldownMs * 0.65f : def.FireCooldownMs;
        }

        enemy.Phase -= dt * 1000f;
        if (enemy.Phase > 0) return;
        enemy.Phase = enraged ? RuneLordSummonEnragedMs : RuneLordSummonMs;
        TrySummonAcolyte(state, enemy);
        if (enraged) TrySummonAcolyte(state, enemy);
    }

    private static void TrySummonAcolyte(RunState state, EnemyEntity boss)
    {
        var minions = state.Entities.OfType<EnemyEntity>()
            .Count(e => e.Hp > 0 && e.Enemy == EnemyKind.Acolyte);
        if (minions >= RuneLordMaxMinions) return;

        var def = Enemies.Defs[EnemyKind.Acolyte];
        var angle = (++summonSerial % 8) * (MathF.PI / 4f);
        var minion = new EnemyEntity
        {
            Id = $"{boss.Id}-acolyte-{summonSerial}",
            Enemy = EnemyKind.Acolyte,
            X = boss.X + MathF.Cos(angle) * GameConstants.Cell * 1.6f,
            Y = boss.Y + MathF.Sin(angle) * GameConstants.Cell * 1.6f,
            Radius = def.Radius,
            Hp = RuneLordMinionHp,
            Cooldown = def.FireCooldownMs,
            Phase = angle,
            CanSplit = false,
        };
        ClampInsideRoom(minion);
        state.Entities.Add(minion);
    }

    /// <summary>Chamado pelo Combat quando o Rei Lodo morre com CanSplit.</summary>
    public static void SplitSlimeKing(RunState state, EnemyEntity king)
    {
        var slime = Enemies.Defs[EnemyKind.Slime];
        var px = king.X;
        var py = king.Y;
        var radius = slime.Radius * SlimeKingChildRadiusScale;
        state.Entities.Remove(king);

        SpawnChild(-SlimeKingSplitOffsetPx);
        SpawnChild(SlimeKingSplitOffsetPx);

        void SpawnChild(float offsetX)
        {
            var child = new EnemyEntity
            {
                Id = $"{king.Id}-heir-{offsetX}",
                Enemy = EnemyKind.Slime,
                X = px + offsetX,
                Y = py,
                Radius = radius,
                Hp = SlimeKingChildHp,
                CanSplit = false,
                Cooldown = SlimeNoSplitCooldownMs,
            };
            ClampInsideRoom(child);
            state.Entities.Add(child);
        }
    }
}
