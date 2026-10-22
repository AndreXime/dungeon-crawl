# Item HUD Prompt Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Mostrar no HUD (abaixo dos corações) nome e efeito do item de upgrade mais próximo do jogador.

**Architecture:** Helper puro `ItemPrompt` escolhe o pickup e formata textos. `HudRenderer` só desenha com `GlyphFont`. Sem estado novo em `RunState`.

**Tech Stack:** C# / .NET 9, MonoGame DesktopGL, xUnit.

## Global Constraints

- GlyphFont: só A-Z, 0-9, espaço, `-`, `:`, `?` (usar `MAIS`, não `+`, e `1-5` para 1.5).
- Todos os `PickupKind.Item` (item room e loja).
- Um prompt por vez, o mais perto do jogador.
- Spec: `docs/superpowers/specs/2026-10-22-item-hud-prompt-design.md`.

---

### Task 1: Helper ItemPrompt + testes

**Files:**
- Create: `ChamberCrawl/Systems/ItemPrompt.cs`
- Create: `ChamberCrawl.Tests/ItemPromptTests.cs`

**Interfaces:**
- Produces: `ItemPrompt.TryResolve(IEnumerable<Entity> entities, out string name, out string detail) -> bool`
- Produces: `ItemPrompt.FormatEffect(ItemDef def) -> string`
- Produces: `ItemPrompt.FormatDetail(ItemDef def, int? price) -> string`

- [ ] **Step 1: Write failing tests**

```csharp
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
        Assert.Equal("DENTE AFIADO", name);
        Assert.Equal("DANO MAIS 1", detail);
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
        Assert.Equal("DANO MAIS 1 - 5", detail);
    }

    [Theory]
    [InlineData("damage_up", "DANO MAIS 1")]
    [InlineData("speed_up", "VELOCIDADE MAIS 30")]
    [InlineData("hp_up", "CORACAO MAIS 1")]
    [InlineData("fire_rate", "TIRO MAIS 1-5")]
    [InlineData("triple", "TIRO TRIPLO")]
    [InlineData("greedy", "MULT MOEDAS MAIS 1")]
    public void FormatEffect_MatchesSpec(string id, string expected)
    {
        Assert.Equal(expected, ItemPrompt.FormatEffect(Items.Defs[id]));
    }
}
```

- [ ] **Step 2: Run tests — expect FAIL** (tipo `ItemPrompt` inexistente)

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~ItemPromptTests" --nologo
```

- [ ] **Step 3: Implement `ItemPrompt.cs`**

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class ItemPrompt
{
    public static bool TryResolve(IEnumerable<Entity> entities, out string name, out string detail)
    {
        name = "";
        detail = "";
        var list = entities as IList<Entity> ?? entities.ToList();
        var player = list.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return false;

        PickupEntity? best = null;
        var bestDist = float.MaxValue;
        foreach (var entity in list)
        {
            if (entity is not PickupEntity { Pickup: PickupKind.Item, ItemId: { } } item)
                continue;
            var dx = item.X - player.X;
            var dy = item.Y - player.Y;
            var dist = dx * dx + dy * dy;
            if (dist >= bestDist) continue;
            bestDist = dist;
            best = item;
        }
        if (best?.ItemId is null || !Items.Defs.TryGetValue(best.ItemId, out var def))
            return false;

        name = Normalize(def.Name);
        detail = FormatDetail(def, best.Price);
        return true;
    }

    public static string FormatDetail(ItemDef def, int? price)
    {
        var effect = FormatEffect(def);
        if (price is > 0) return $"{effect} - {price.Value}";
        return effect;
    }

    public static string FormatEffect(ItemDef def)
    {
        if (def.Damage is { } d) return $"DANO MAIS {FormatNum(d)}";
        if (def.Speed is { } s) return $"VELOCIDADE MAIS {FormatNum(s)}";
        if (def.MaxHp is { } hp) return $"CORACAO MAIS {FormatNum(hp / 2f)}";
        if (def.FireRate is { } fr) return $"TIRO MAIS {FormatNum(fr)}";
        if (def.TripleShot == true) return "TIRO TRIPLO";
        if (def.CoinMultiplier is { } c) return $"MULT MOEDAS MAIS {FormatNum(c)}";
        return "UPGRADE";
    }

    private static string FormatNum(float value)
    {
        if (Math.Abs(value - MathF.Round(value)) < 0.001f)
            return ((int)MathF.Round(value)).ToString(CultureInfo.InvariantCulture);
        var tenths = (int)MathF.Round(value * 10f);
        return $"{tenths / 10}-{tenths % 10}";
    }

    private static string Normalize(string text) =>
        text.ToUpperInvariant()
            .Replace('Á', 'A').Replace('À', 'A').Replace('Â', 'A').Replace('Ã', 'A')
            .Replace('É', 'E').Replace('Ê', 'E')
            .Replace('Í', 'I')
            .Replace('Ó', 'O').Replace('Ô', 'O').Replace('Õ', 'O')
            .Replace('Ú', 'U').Replace('Ü', 'U')
            .Replace('Ç', 'C');
}
```

- [ ] **Step 4: Run tests — expect PASS**

- [ ] **Step 5: Commit** (só se o usuário pedir)

---

### Task 2: Desenhar no HudRenderer

**Files:**
- Modify: `ChamberCrawl/Rendering/HudRenderer.cs`

**Interfaces:**
- Consumes: `ItemPrompt.TryResolve`

- [ ] **Step 1: Em `DrawHud`, após a linha de status, chamar desenho do prompt**

Após `DrawMinimap(...)`, adicionar:

```csharp
DrawItemPrompt(spriteBatch, state, atlas);
```

E o método:

```csharp
private static void DrawItemPrompt(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
{
    if (!ItemPrompt.TryResolve(state.Entities, out var name, out var detail))
        return;

    var x = HudPad;
    var y = HudPad + HeartSize + 6;
    GlyphFont.Draw(spriteBatch, atlas.Pixel, name, x, y, Color.White, scale: 1);
    GlyphFont.Draw(
        spriteBatch,
        atlas.Pixel,
        detail,
        x,
        y + GlyphFont.GlyphH + 4,
        new Color(200, 210, 230),
        scale: 1);
}
```

Adicionar `using ChamberCrawl.Systems;` se necessário.

- [ ] **Step 2: Build**

```bash
dotnet build ChamberCrawl/ChamberCrawl.csproj --nologo
```

Expected: êxito.

- [ ] **Step 3: Commit** (só se o usuário pedir)

---

## Spec coverage

| Spec | Task |
|------|------|
| Mais próximo | Task 1 |
| Nome + efeito | Task 1 |
| Loja com preço | Task 1 |
| Abaixo dos corações | Task 2 |
| GlyphFont-safe | Task 1 FormatNum/Normalize |
| Testes unitários | Task 1 |
