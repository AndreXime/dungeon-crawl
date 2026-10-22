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
            if (entity is not PickupEntity { Pickup: PickupKind.Item, ItemId: not null } item)
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
