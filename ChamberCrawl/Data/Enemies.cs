using System;
using System.Collections.Generic;
using ChamberCrawl.Models;

namespace ChamberCrawl.Data;

public sealed record EnemyDef(
    float Hp,
    float Speed,
    int Damage,
    float Radius,
    float FireCooldownMs,
    float ProjectileSpeed,
    string Texture,
    float? PreferredDist = null,
    float? ChargeSpeed = null);

public static class Enemies
{
    public static readonly IReadOnlyDictionary<EnemyKind, EnemyDef> Defs = new Dictionary<EnemyKind, EnemyDef>
    {
        [EnemyKind.Bat] = new(5, 90, 1, 10, 0, 0, "enemy-bat"),
        [EnemyKind.Skeleton] = new(5, 55, 1, 10, 1100, 170, "enemy-skeleton", PreferredDist: 90),
        [EnemyKind.Slime] = new(2, 65, 1, 9, 0, 0, "enemy-slime"),
        [EnemyKind.Specter] = new(3, 70, 1, 10, 0, 0, "enemy-specter"),
        [EnemyKind.Rat] = new(3, 115, 1, 9, 0, 0, "enemy-rat"),
        [EnemyKind.Guard] = new(12, 38, 2, 12, 0, 0, "enemy-guard"),
        [EnemyKind.Acolyte] = new(5, 58, 1, 10, 1300, 160, "enemy-acolyte", PreferredDist: 70),
        [EnemyKind.Ogre] = new(60, 52, 2, 24, 1100, 170, "enemy-ogre", ChargeSpeed: 105),
        [EnemyKind.Necromancer] = new(55, 42, 1, 16, 1000, 150, "enemy-necromancer", PreferredDist: 110),
        [EnemyKind.SlimeKing] = new(50, 48, 2, 20, 1300, 140, "enemy-slime-king", PreferredDist: 80),
        [EnemyKind.IronGolem] = new(70, 36, 2, 22, 950, 160, "enemy-iron-golem", PreferredDist: 100, ChargeSpeed: 130),
        [EnemyKind.RuneLord] = new(75, 40, 2, 18, 850, 165, "enemy-rune-lord", PreferredDist: 105),
    };

    /// <summary>HP e dano sobem por depth; speed/cadência ficam no def base.</summary>
    public static EnemyDef AtDepth(EnemyKind kind, int depth)
    {
        var d = Defs[kind];
        var floor = depth < 1 ? 1 : depth > 5 ? 5 : depth;
        var hpMul = 1f + (floor - 1) * 0.15f;
        var damageBonus = Math.Max(0, floor - 2);
        return d with
        {
            Hp = MathF.Round(d.Hp * hpMul),
            Damage = d.Damage + damageBonus,
        };
    }
}
