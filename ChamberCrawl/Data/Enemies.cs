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
        [EnemyKind.Chaser] = new(3, 70, 1, 10, 0, 0, "enemy-chaser"),
        [EnemyKind.Shooter] = new(2, 40, 1, 10, 1500, 140, "enemy-shooter", PreferredDist: 140),
        [EnemyKind.Wander] = new(2, 50, 1, 9, 0, 0, "enemy-wander"),
        [EnemyKind.Boss] = new(30, 45, 2, 22, 1200, 160, "enemy-boss", ChargeSpeed: 110),
    };
}
