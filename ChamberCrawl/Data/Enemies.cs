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
        [EnemyKind.Pspam] = new(2, 50, 1, 9, 0, 0, "enemy-pspam"),
        [EnemyKind.Trojan] = new(3, 70, 1, 10, 0, 0, "enemy-trojan", ChargeSpeed: 140),
        [EnemyKind.AdwareKing] = new(40, 45, 2, 24, 1200, 160, "enemy-adware-king", ChargeSpeed: 90),
    };
}
