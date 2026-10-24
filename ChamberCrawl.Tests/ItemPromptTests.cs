using System.Collections.Generic;
using ChamberCrawl.Data;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class ItemPromptTests
{
    [Fact]
    public void TryResolve_FalseWhenNoItems()
    {
        var ok = ItemPrompt.TryResolve(
            new Entity[] { new PlayerEntity { Id = "p", X = 0, Y = 0 } },
            out _, out _);
        Assert.False(ok);
    }

    [Fact]
    public void TryResolve_PicksNearestItem()
    {
        var entities = new List<Entity>
        {
            new PlayerEntity { Id = "p", X = 0, Y = 0 },
            new PickupEntity { Id = "far", Pickup = PickupKind.Item, ItemId = "speed_up", X = 100, Y = 0 },
            new PickupEntity { Id = "near", Pickup = PickupKind.Item, ItemId = "damage_up", X = 10, Y = 0 },
        };
        Assert.True(ItemPrompt.TryResolve(entities, out var name, out var detail));
        Assert.Equal("LAMINA", name);
        Assert.Equal("COMUM - DANO MAIS 1", detail);
    }

    [Fact]
    public void TryResolve_IncludesShopPrice()
    {
        var entities = new List<Entity>
        {
            new PlayerEntity { Id = "p", X = 0, Y = 0 },
            new PickupEntity
            {
                Id = "shop",
                Pickup = PickupKind.Item,
                ItemId = "damage_up",
                Price = 5,
                X = 5,
                Y = 0,
            },
        };
        Assert.True(ItemPrompt.TryResolve(entities, out _, out var detail));
        Assert.Equal("COMUM - DANO MAIS 1 - 5", detail);
    }

    [Theory]
    [InlineData("damage_up", "DANO MAIS 1")]
    [InlineData("speed_up", "VELOCIDADE MAIS 30")]
    [InlineData("hp_up", "CORACAO MAIS 1")]
    [InlineData("fire_rate", "TIRO MAIS 1.5")]
    [InlineData("triple", "TIRO TRIPLO")]
    [InlineData("greedy", "MULT MOEDAS MAIS 1")]
    [InlineData("range_up", "ALCANCE MAIS 80")]
    [InlineData("sharpness", "DANO MAIS 2")]
    [InlineData("haste", "VELOCIDADE MAIS 15 TIRO MAIS 2")]
    [InlineData("vigor", "CORACAO MAIS 1 CURA MAIS 2")]
    [InlineData("fortune", "MULT MOEDAS MAIS 2")]
    public void FormatEffect_MatchesSpec(string id, string expected)
    {
        Assert.Equal(expected, ItemPrompt.FormatEffect(Items.Defs[id]));
    }

    [Theory]
    [InlineData("damage_up", "COMUM")]
    [InlineData("triple", "RARO")]
    [InlineData("fortune", "LENDARIO")]
    public void FormatDetail_IncludesRarity(string id, string rarity)
    {
        var detail = ItemPrompt.FormatDetail(Items.Defs[id], null);
        Assert.StartsWith($"{rarity} - ", detail);
    }
}
