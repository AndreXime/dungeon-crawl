using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Ai
{
    public const float SlimeSplitMs = 4000f;
    public const float WanderMinMs = 500f;
    public const float WanderMaxMs = 1500f;

    /// <summary>Dano de contato fica em Combat.Update. Movimenta inimigos por kind (stubs até Tasks 3–6).</summary>
    public static void Update(RunState state, float dt, Func<float>? rng = null)
    {
        if (state.Status != RunStatus.Playing) return;
        var player = state.Entities.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return;

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
                    UpdateSlime(state, enemy, player, dt, rng);
                    break;
                case EnemyKind.Ogre:
                    UpdateOgre(state, enemy, player, dt, rng);
                    break;
            }
        }
    }

    private static void UpdateBat(RunState state, EnemyEntity enemy, PlayerEntity player, float dt, Func<float>? rng)
    {
    }

    private static void UpdateSkeleton(RunState state, EnemyEntity enemy, PlayerEntity player, float dt, Func<float>? rng)
    {
    }

    private static void UpdateSlime(RunState state, EnemyEntity enemy, PlayerEntity player, float dt, Func<float>? rng)
    {
    }

    private static void UpdateOgre(RunState state, EnemyEntity enemy, PlayerEntity player, float dt, Func<float>? rng)
    {
    }
}
