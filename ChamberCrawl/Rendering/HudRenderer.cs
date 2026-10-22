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
        DrawItemPrompt(spriteBatch, state, atlas);
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
        var cells = new List<(FloorRoomNode Room, int X, int Y)>();
        foreach (var room in state.Floor.Rooms.Values)
        {
            if (!room.Visited && room.Id != state.CurrentRoomId) continue;
            if (!TryParseRoomCoords(room.Id, out var rx, out var ry)) continue;
            cells.Add((room, rx, ry));
        }
        if (cells.Count == 0) return;

        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;
        foreach (var (_, cx, cy) in cells)
        {
            if (cx < minX) minX = cx;
            if (cy < minY) minY = cy;
            if (cx > maxX) maxX = cx;
            if (cy > maxY) maxY = cy;
        }

        var mapW = (maxX - minX + 1) * MinimapCell;
        var mapH = (maxY - minY + 1) * MinimapCell;
        var originX = GameConstants.RoomW * GameConstants.Cell - mapW - HudPad;
        var originY = HudPad;

        spriteBatch.Draw(
            atlas.Pixel,
            new Rectangle(originX - 2, originY - 2, mapW + 4, mapH + 4),
            new Color(0, 0, 0, 140));

        foreach (var (room, cx, cy) in cells)
        {
            var current = room.Id == state.CurrentRoomId;
            var color = RoomColor(room.Type, current);
            var dest = new Rectangle(
                originX + (cx - minX) * MinimapCell + 1,
                originY + (cy - minY) * MinimapCell + 1,
                MinimapCell - 2,
                MinimapCell - 2);
            spriteBatch.Draw(atlas.Pixel, dest, color);
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

    private static bool TryParseRoomCoords(string id, out int x, out int y)
    {
        x = 0;
        y = 0;
        var comma = id.IndexOf(',');
        if (comma <= 0) return false;
        return int.TryParse(id.AsSpan(0, comma), NumberStyles.Integer, CultureInfo.InvariantCulture, out x)
            && int.TryParse(id.AsSpan(comma + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out y);
    }

    private static void DrawBanner(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
    {
        if (state.Status is not (RunStatus.Dead or RunStatus.Won)) return;

        var roomW = GameConstants.RoomW * GameConstants.Cell;
        var roomH = GameConstants.RoomH * GameConstants.Cell;
        spriteBatch.Draw(atlas.Pixel, new Rectangle(0, 0, roomW, roomH), new Color(0, 0, 0, 160));

        var bannerW = 220;
        var bannerH = 56;
        var bx = (roomW - bannerW) / 2;
        var by = (roomH - bannerH) / 2;
        var fill = state.Status == RunStatus.Won
            ? new Color(40, 120, 60, 230)
            : new Color(140, 40, 40, 230);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(bx, by, bannerW, bannerH), fill);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(bx, by, bannerW, 4), Color.White * 0.7f);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(bx, by + bannerH - 4, bannerW, 4), Color.White * 0.35f);

        var iconX = bx + bannerW / 2 - 20;
        var iconY = by + 16;
        spriteBatch.Draw(atlas.Get("coin"), new Rectangle(iconX, iconY, 20, 20), Color.White);
        DrawRestartMark(spriteBatch, atlas, iconX + 28, iconY + 2);
    }

    private static void DrawRestartMark(SpriteBatch spriteBatch, TextureAtlas atlas, int x, int y)
    {
        spriteBatch.Draw(atlas.Pixel, new Rectangle(x, y, 3, 16), Color.White);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(x, y, 10, 3), Color.White);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(x + 8, y, 3, 7), Color.White);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(x, y + 6, 10, 3), Color.White);
        spriteBatch.Draw(atlas.Pixel, new Rectangle(x + 6, y + 9, 3, 7), Color.White);
    }
}
