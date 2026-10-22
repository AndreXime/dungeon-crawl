using System;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Ai
{
    /// <summary>Dano de contato fica em Combat.Update. AI por kind nas tasks 4–6.</summary>
    public static void Update(RunState state, float dt, Func<float>? rng = null)
    {
        if (state.Status != RunStatus.Playing) return;
        var player = state.Entities.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return;

        foreach (var enemy in state.Entities.OfType<EnemyEntity>().Where(e => e.Hp > 0).ToList())
        {
            switch (enemy.Enemy)
            {
                case EnemyKind.Pspam:
                case EnemyKind.Trojan:
                case EnemyKind.AdwareKing:
                    break;
            }
        }
    }
}
