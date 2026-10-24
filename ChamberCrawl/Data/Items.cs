using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.Data;

public enum ItemRarity
{
    Common,
    Rare,
    Legendary,
}

public sealed record ItemDef(
    string Id,
    string Name,
    string Texture,
    ItemRarity Rarity,
    float? Damage = null,
    float? Speed = null,
    int? MaxHp = null,
    int? Heal = null,
    float? FireRate = null,
    float? Range = null,
    bool? TripleShot = null,
    int? CoinMultiplier = null);

public static class Items
{
    public static readonly IReadOnlyDictionary<string, ItemDef> Defs = new Dictionary<string, ItemDef>
    {
        ["damage_up"] = new("damage_up", "Lâmina", "item-damage", ItemRarity.Common, Damage: 1),
        ["speed_up"] = new("speed_up", "Botas", "item-speed", ItemRarity.Common, Speed: 30),
        ["hp_up"] = new("hp_up", "Poção", "item-hp", ItemRarity.Common, MaxHp: 2, Heal: 2),
        ["range_up"] = new("range_up", "Arco", "item-range", ItemRarity.Common, Range: 80),
        ["fire_rate"] = new("fire_rate", "Pergaminho", "item-firerate", ItemRarity.Rare, FireRate: 1.5f),
        ["triple"] = new("triple", "Tridente", "item-triple", ItemRarity.Rare, TripleShot: true),
        ["sharpness"] = new("sharpness", "Espada", "item-sharp", ItemRarity.Rare, Damage: 2),
        ["haste"] = new("haste", "Ampulheta", "item-haste", ItemRarity.Rare, FireRate: 2f, Speed: 15),
        ["vigor"] = new("vigor", "Escudo", "item-vigor", ItemRarity.Rare, MaxHp: 2, Heal: 4),
        ["greedy"] = new("greedy", "Ímã", "item-greedy", ItemRarity.Legendary, CoinMultiplier: 1),
        ["fortune"] = new("fortune", "Trevo", "item-fortune", ItemRarity.Legendary, CoinMultiplier: 2),
    };

    public static string RarityLabel(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Common => "COMUM",
        ItemRarity.Rare => "RARO",
        ItemRarity.Legendary => "LENDARIO",
        _ => "COMUM",
    };

    public static int Weight(ItemRarity rarity) => Weight(rarity, depth: 1);

    /// <summary>Andares mais profundos pesam mais Rare/Legendary.</summary>
    public static int Weight(ItemRarity rarity, int depth)
    {
        var d = depth < 1 ? 1 : depth > 5 ? 5 : depth;
        return (rarity, d) switch
        {
            (ItemRarity.Common, 1) => 75,
            (ItemRarity.Rare, 1) => 22,
            (ItemRarity.Legendary, 1) => 3,
            (ItemRarity.Common, 2) => 65,
            (ItemRarity.Rare, 2) => 28,
            (ItemRarity.Legendary, 2) => 7,
            (ItemRarity.Common, 3) => 55,
            (ItemRarity.Rare, 3) => 35,
            (ItemRarity.Legendary, 3) => 10,
            (ItemRarity.Common, 4) => 45,
            (ItemRarity.Rare, 4) => 40,
            (ItemRarity.Legendary, 4) => 15,
            (ItemRarity.Common, _) => 35,
            (ItemRarity.Rare, _) => 45,
            (ItemRarity.Legendary, _) => 20,
            _ => 35,
        };
    }

    public static int ShopPrice(ItemRarity rarity, Random rng) => ShopPrice(rarity, rng, depth: 1);

    public static int ShopPrice(ItemRarity rarity, Random rng, int depth)
    {
        var basePrice = rarity switch
        {
            ItemRarity.Common => 4 + rng.Next(4),
            ItemRarity.Rare => 8 + rng.Next(5),
            ItemRarity.Legendary => 15 + rng.Next(6),
            _ => 4 + rng.Next(4),
        };
        var d = depth < 1 ? 1 : depth > 5 ? 5 : depth;
        return basePrice + (d - 1);
    }

    public static ItemDef PickWeighted(
        Random rng, IEnumerable<string>? excludeIds = null, int depth = 1)
    {
        var exclude = excludeIds is null
            ? null
            : new HashSet<string>(excludeIds, StringComparer.Ordinal);

        var available = Defs.Values
            .Where(d => exclude is null || !exclude.Contains(d.Id))
            .ToList();
        if (available.Count == 0)
            available = Defs.Values.ToList();

        var rarity = RollRarity(rng, available, depth);
        var pool = available.Where(d => d.Rarity == rarity).ToList();
        if (pool.Count == 0)
            pool = available;
        return pool[rng.Next(pool.Count)];
    }

    private static ItemRarity RollRarity(Random rng, IReadOnlyList<ItemDef> available, int depth)
    {
        var present = available.Select(d => d.Rarity).Distinct().ToList();
        var total = present.Sum(r => Weight(r, depth));
        var roll = rng.Next(total);
        var acc = 0;
        foreach (var rarity in present.OrderBy(r => Weight(r, depth)))
        {
            acc += Weight(rarity, depth);
            if (roll < acc) return rarity;
        }
        return present[0];
    }

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
        if (item.Range is { } range) next.Range += range;
        if (item.TripleShot is { } triple) next.TripleShot = triple;
        if (item.CoinMultiplier is { } coins) next.CoinMultiplier += coins;
        return next;
    }
}
