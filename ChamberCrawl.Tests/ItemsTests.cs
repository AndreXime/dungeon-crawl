using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Tests;

public class ItemsTests
{
    private static PlayerStats Base() => new()
    {
        MaxHp = 6, Hp = 4, Speed = 120, Damage = 1, FireRate = 4,
        Range = 320, TripleShot = false, CoinMultiplier = 1, IframeUntil = 0,
    };

    [Fact]
    public void Defs_ContainsElevenItemsWithRarities()
    {
        Assert.Equal(11, Items.Defs.Count);
        Assert.Equal(ItemRarity.Common, Items.Defs["damage_up"].Rarity);
        Assert.Equal(ItemRarity.Common, Items.Defs["speed_up"].Rarity);
        Assert.Equal(ItemRarity.Common, Items.Defs["hp_up"].Rarity);
        Assert.Equal(ItemRarity.Common, Items.Defs["range_up"].Rarity);
        Assert.Equal(ItemRarity.Rare, Items.Defs["fire_rate"].Rarity);
        Assert.Equal(ItemRarity.Rare, Items.Defs["triple"].Rarity);
        Assert.Equal(ItemRarity.Rare, Items.Defs["sharpness"].Rarity);
        Assert.Equal(ItemRarity.Rare, Items.Defs["haste"].Rarity);
        Assert.Equal(ItemRarity.Rare, Items.Defs["vigor"].Rarity);
        Assert.Equal(ItemRarity.Legendary, Items.Defs["greedy"].Rarity);
        Assert.Equal(ItemRarity.Legendary, Items.Defs["fortune"].Rarity);
    }

    [Fact]
    public void Apply_AppliesEachItemWithoutMutatingOriginal()
    {
        var original = Base();

        Assert.Equal(2, Items.Apply(original, "damage_up").Damage);
        var speed = Items.Apply(original, "speed_up");
        Assert.Equal(150, speed.Speed);
        Assert.Equal(120, original.Speed);

        var hp = Items.Apply(original, "hp_up");
        Assert.Equal(8, hp.MaxHp);
        Assert.Equal(6, hp.Hp);

        Assert.Equal(5.5f, Items.Apply(original, "fire_rate").FireRate);
        Assert.True(Items.Apply(original, "triple").TripleShot);
        Assert.Equal(2, Items.Apply(original, "greedy").CoinMultiplier);
        Assert.Equal(400, Items.Apply(original, "range_up").Range);
        Assert.Equal(3, Items.Apply(original, "sharpness").Damage);
        var haste = Items.Apply(original, "haste");
        Assert.Equal(6f, haste.FireRate);
        Assert.Equal(135, haste.Speed);
        var vigor = Items.Apply(original, "vigor");
        Assert.Equal(8, vigor.MaxHp);
        Assert.Equal(8, vigor.Hp);
        Assert.Equal(3, Items.Apply(original, "fortune").CoinMultiplier);
    }

    [Fact]
    public void Apply_UnknownItemReturnsEqualClone()
    {
        var original = Base();
        var next = Items.Apply(original, "nope");
        Assert.NotSame(original, next);
        Assert.Equal(original.Damage, next.Damage);
    }

    [Fact]
    public void Apply_HealIsCappedAtMaxHp()
    {
        var stats = Base();
        stats.Hp = 6;
        Assert.Equal(8, Items.Apply(stats, "hp_up").Hp);
    }

    [Fact]
    public void ShopPrice_FallsInRarityBand()
    {
        var rng = new Random(1);
        for (var i = 0; i < 40; i++)
        {
            Assert.InRange(Items.ShopPrice(ItemRarity.Common, rng), 4, 7);
            Assert.InRange(Items.ShopPrice(ItemRarity.Rare, rng), 8, 12);
            Assert.InRange(Items.ShopPrice(ItemRarity.Legendary, rng), 15, 20);
        }
    }

    [Fact]
    public void PickWeighted_RespectsExcludeAndReturnsKnownId()
    {
        var rng = new Random(42);
        var picked = Items.PickWeighted(rng, new[] { "damage_up" });
        Assert.True(Items.Defs.ContainsKey(picked.Id));
        Assert.NotEqual("damage_up", picked.Id);
    }
}
