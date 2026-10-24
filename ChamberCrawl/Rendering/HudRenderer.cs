using System.Globalization;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChamberCrawl.Rendering;

public static class HudRenderer
{
    private const int HudPad = 8;
    private const int HeartSize = 20;
    private const int IconSize = 20;
    private const int DigitW = 10;
    private const int DigitH = 14;
    private const int DigitGap = 2;
    private const int MinimapCell = 10;

    public static void Draw(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
    {
        DrawHud(spriteBatch, state, atlas);
        DrawBanner(spriteBatch, state, atlas);
    }

    private static void DrawHud(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
    {
        var x = HudPad;
        var y = HudPad;

        DrawHearts(spriteBatch, atlas, state.PlayerStats.Hp, state.PlayerStats.MaxHp, x, y);
        x += HeartRowWidth(state.PlayerStats.MaxHp) + 12;

        spriteBatch.Draw(
            atlas.Get("coin"),
            new Rectangle(x, y, IconSize, IconSize),
            Color.White);
        DrawNumber(spriteBatch, atlas, state.Coins, x + IconSize + 4, y + 1, Color.Gold);
        x += IconSize + 4 + NumberWidth(state.Coins) + 16;

        spriteBatch.Draw(
            atlas.Pixel,
            new Rectangle(x, y + 2, 12, IconSize - 4),
            new Color(90, 160, 220));
        DrawNumber(spriteBatch, atlas, state.Floor.Depth, x + 16, y + 1, Color.LightSkyBlue);

        DrawMinimap(spriteBatch, state, atlas);
        DrawStatsPanel(spriteBatch, state, atlas);
        DrawItemPrompt(spriteBatch, state, atlas);
    }

    private static void DrawStatsPanel(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
    {
        var stats = state.PlayerStats;
        var lines = new List<string>
        {
            $"DANO {FormatStat(stats.Damage)}",
            $"VEL {FormatStat(stats.Speed)}",
            $"TIRO {FormatStat(stats.FireRate)}",
            $"ALC {FormatStat(stats.Range)}",
            $"MOED x{stats.CoinMultiplier}",
        };
        if (stats.TripleShot)
            lines.Add("TRIPLO");

        const int scale = 1;
        const int lineGap = 3;
        var lineH = GlyphFont.GlyphH * scale + lineGap;
        var panelH = lines.Count * lineH - lineGap;
        var roomW = GameConstants.RoomW * GameConstants.Cell;
        var roomH = GameConstants.RoomH * GameConstants.Cell;
        var maxW = 0;
        foreach (var line in lines)
            maxW = Math.Max(maxW, GlyphFont.MeasureWidth(line, scale));

        var originX = roomW - HudPad - maxW;
        var originY = roomH - HudPad - panelH;
        var color = new Color(210, 215, 230);

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var w = GlyphFont.MeasureWidth(line, scale);
            GlyphFont.Draw(
                spriteBatch,
                atlas.Pixel,
                line,
                originX + (maxW - w),
                originY + i * lineH,
                color,
                scale);
        }
    }

    private static string FormatStat(float value)
    {
        if (Math.Abs(value - MathF.Round(value)) < 0.001f)
            return ((int)MathF.Round(value)).ToString(CultureInfo.InvariantCulture);
        return value.ToString("0.0", CultureInfo.InvariantCulture);
    }

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

    private static void DrawHearts(
        SpriteBatch spriteBatch,
        TextureAtlas atlas,
        int hp,
        int maxHp,
        int x,
        int y)
    {
        var total = Math.Max(1, (int)Math.Ceiling(maxHp / 2.0));
        var heart = atlas.Get("heart");
        for (var i = 0; i < total; i++)
        {
            var filled = hp - i * 2;
            var dest = new Rectangle(x + i * (HeartSize + 2), y, HeartSize, HeartSize);
            if (filled >= 2)
            {
                spriteBatch.Draw(heart, dest, Color.White);
            }
            else if (filled == 1)
            {
                spriteBatch.Draw(heart, dest, new Color(80, 80, 80, 180));
                var half = new Rectangle(dest.X, dest.Y, HeartSize / 2, HeartSize);
                var src = new Rectangle(0, 0, heart.Width / 2, heart.Height);
                spriteBatch.Draw(heart, half, src, Color.White);
            }
            else
            {
                spriteBatch.Draw(heart, dest, new Color(50, 50, 50, 140));
            }
        }
    }

    private static int HeartRowWidth(int maxHp)
    {
        var total = Math.Max(1, (int)Math.Ceiling(maxHp / 2.0));
        return total * (HeartSize + 2) - 2;
    }

    private static void DrawNumber(
        SpriteBatch spriteBatch,
        TextureAtlas atlas,
        int value,
        int x,
        int y,
        Color color)
    {
        value = Math.Max(0, value);
        var text = value.ToString(CultureInfo.InvariantCulture);
        var cursor = x;
        foreach (var ch in text)
        {
            var dig = ch - '0';
            spriteBatch.Draw(atlas.Digit(dig), new Rectangle(cursor, y, DigitW, DigitH), color);
            cursor += DigitW + DigitGap;
        }
    }

    private static int NumberWidth(int value)
    {
        var digits = Math.Max(1, value.ToString(CultureInfo.InvariantCulture).Length);
        return digits * DigitW + (digits - 1) * DigitGap;
    }

    private static void DrawMinimap(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
    {
        var cells = MinimapVisibility.Collect(state);
        if (cells.Count == 0) return;

        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;
        foreach (var cell in cells)
        {
            if (cell.X < minX) minX = cell.X;
            if (cell.Y < minY) minY = cell.Y;
            if (cell.X > maxX) maxX = cell.X;
            if (cell.Y > maxY) maxY = cell.Y;
        }

        var mapW = (maxX - minX + 1) * MinimapCell;
        var originX = GameConstants.RoomW * GameConstants.Cell - mapW - HudPad;
        var originY = HudPad;

        foreach (var cell in cells)
        {
            var dest = new Rectangle(
                originX + (cell.X - minX) * MinimapCell + 1,
                originY + (cell.Y - minY) * MinimapCell + 1,
                MinimapCell - 2,
                MinimapCell - 2);

            if (cell.Unknown)
            {
                spriteBatch.Draw(atlas.Pixel, dest, new Color(40, 42, 50));
                var qx = dest.X + (dest.Width - GlyphFont.GlyphW) / 2;
                var qy = dest.Y + (dest.Height - GlyphFont.GlyphH) / 2;
                GlyphFont.Draw(spriteBatch, atlas.Pixel, "?", qx, qy, new Color(180, 185, 195), scale: 1);
                continue;
            }

            var current = cell.Room.Id == state.CurrentRoomId;
            spriteBatch.Draw(atlas.Pixel, dest, RoomColor(cell.Room.Type, current));
        }
    }

    private static Color RoomColor(RoomType type, bool current) => type switch
    {
        RoomType.Boss => current ? new Color(255, 140, 60) : new Color(180, 80, 30),
        RoomType.Shop => current ? new Color(255, 220, 80) : new Color(180, 150, 40),
        RoomType.Item => current ? new Color(120, 220, 255) : new Color(60, 120, 180),
        RoomType.Start => current ? new Color(140, 255, 140) : new Color(60, 140, 60),
        _ => current ? new Color(220, 220, 230) : new Color(100, 100, 120),
    };

    private static void DrawBanner(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
    {
        if (state.Status is not (RunStatus.Dead or RunStatus.Won)) return;

        var roomW = GameConstants.RoomW * GameConstants.Cell;
        var roomH = GameConstants.RoomH * GameConstants.Cell;
        spriteBatch.Draw(atlas.Pixel, new Rectangle(0, 0, roomW, roomH), new Color(0, 0, 0, 160));

        var won = state.Status == RunStatus.Won;
        var title = won ? "VITORIA" : "DERROTA";
        var hint = "R PARA REINICIAR";
        var titleColor = won ? new Color(140, 255, 160) : new Color(255, 140, 140);
        var hintColor = new Color(220, 220, 230);

        var titleScale = 3;
        var titleW = GlyphFont.MeasureWidth(title, titleScale);
        var hintW = GlyphFont.MeasureWidth(hint, scale: 1);
        var bannerW = Math.Max(titleW, hintW) + 32;
        var bannerH = 72;
        var bx = (roomW - bannerW) / 2;
        var by = (roomH - bannerH) / 2;
        var fill = won
            ? new Color(40, 120, 60, 230)
            : new Color(140, 40, 40, 230);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(bx, by, bannerW, bannerH), fill);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(bx, by, bannerW, 4), Color.White * 0.7f);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(bx, by + bannerH - 4, bannerW, 4), Color.White * 0.35f);

        GlyphFont.Draw(
            spriteBatch,
            atlas.Pixel,
            title,
            bx + (bannerW - titleW) / 2,
            by + 14,
            titleColor,
            titleScale);
        GlyphFont.Draw(
            spriteBatch,
            atlas.Pixel,
            hint,
            bx + (bannerW - hintW) / 2,
            by + 46,
            hintColor,
            scale: 1);
    }
}
