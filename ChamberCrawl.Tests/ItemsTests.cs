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
    public void Defs_ContainsSixItems()
    {
        Assert.Equal(6, Items.Defs.Count);
        foreach (var id in new[] { "damage_up", "speed_up", "hp_up", "fire_rate", "triple", "greedy" })
            Assert.True(Items.Defs.ContainsKey(id));
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
}
