using System;
using System.Collections.Generic;
using ChamberCrawl.Models;

namespace ChamberCrawl.Data;

public sealed record ItemDef(
    string Id,
    string Name,
    string Texture,
    float? Damage = null,
    float? Speed = null,
    int? MaxHp = null,
    int? Heal = null,
    float? FireRate = null,
    bool? TripleShot = null,
    int? CoinMultiplier = null);

public static class Items
{
    public static readonly IReadOnlyDictionary<string, ItemDef> Defs = new Dictionary<string, ItemDef>
    {
        ["damage_up"] = new("damage_up", "Dente Afiado", "item-damage", Damage: 1),
        ["speed_up"] = new("speed_up", "Botas", "item-speed", Speed: 30),
        ["hp_up"] = new("hp_up", "Coração Extra", "item-hp", MaxHp: 2, Heal: 2),
        ["fire_rate"] = new("fire_rate", "Cafeína", "item-firerate", FireRate: 1.5f),
        ["triple"] = new("triple", "Tridente", "item-triple", TripleShot: true),
        ["greedy"] = new("greedy", "Ímã", "item-greedy", CoinMultiplier: 1),
    };

    public static PlayerStats Apply(PlayerStats stats, string itemId)
    {
        var next = new PlayerStats
        {
            MaxHp = stats.MaxHp,
            Hp = stats.Hp,
            Speed = stats.Speed,
            Damage = stats.Damage,
            FireRate = stats.FireRate,
            Range = stats.Range,
            TripleShot = stats.TripleShot,
            CoinMultiplier = stats.CoinMultiplier,
            IframeUntil = stats.IframeUntil,
        };
        if (!Defs.TryGetValue(itemId, out var item)) return next;

        if (item.Damage is { } damage) next.Damage += damage;
        if (item.Speed is { } speed) next.Speed += speed;
        if (item.MaxHp is { } maxHp) next.MaxHp += maxHp;
        if (item.Heal is { } heal) next.Hp = Math.Min(next.MaxHp, next.Hp + heal);
        if (item.FireRate is { } fireRate) next.FireRate += fireRate;
        if (item.TripleShot is { } triple) next.TripleShot = triple;
        if (item.CoinMultiplier is { } coins) next.CoinMultiplier += coins;
        return next;
    }
}
