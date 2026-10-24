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
        [EnemyKind.Bat] = new(3, 70, 1, 10, 0, 0, "enemy-bat"),
        [EnemyKind.Skeleton] = new(3, 40, 1, 10, 1200, 160, "enemy-skeleton", PreferredDist: 90),
        [EnemyKind.Slime] = new(2, 50, 1, 9, 0, 0, "enemy-slime"),
        [EnemyKind.Specter] = new(2, 55, 1, 10, 0, 0, "enemy-specter"),
        [EnemyKind.Rat] = new(2, 95, 1, 9, 0, 0, "enemy-rat"),
        [EnemyKind.Guard] = new(8, 28, 2, 12, 0, 0, "enemy-guard"),
        [EnemyKind.Acolyte] = new(3, 45, 1, 10, 1400, 150, "enemy-acolyte", PreferredDist: 70),
        [EnemyKind.Ogre] = new(40, 45, 2, 24, 1200, 160, "enemy-ogre", ChargeSpeed: 90),
        [EnemyKind.Necromancer] = new(36, 35, 1, 16, 1100, 140, "enemy-necromancer", PreferredDist: 110),
        [EnemyKind.SlimeKing] = new(32, 40, 2, 20, 1400, 130, "enemy-slime-king", PreferredDist: 80),
        [EnemyKind.IronGolem] = new(45, 30, 2, 22, 1000, 150, "enemy-iron-golem", PreferredDist: 100, ChargeSpeed: 110),
        [EnemyKind.RuneLord] = new(48, 32, 2, 18, 900, 155, "enemy-rune-lord", PreferredDist: 105),
    };
}
