using ChamberCrawl.Data;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChamberCrawl.Rendering;

public static class MenuRenderer
{
    public static void DrawMain(
        SpriteBatch spriteBatch,
        TextureAtlas atlas,
        MenuController menu,
        int screenW,
        int screenH)
    {
        DrawBackdrop(spriteBatch, atlas, screenW, screenH);

        var title = "DISKETTE";
        var titleX = (screenW - GlyphFont.MeasureWidth(title)) / 2;
        GlyphFont.Draw(spriteBatch, atlas.Pixel, title, titleX, 48, new Color(120, 200, 255));

        DrawItemList(
            spriteBatch,
            atlas,
            menu.MainLabels,
            menu.MainIndex,
            screenW,
            startY: 140,
            _ => null);
    }

    public static void DrawSettings(
        SpriteBatch spriteBatch,
        TextureAtlas atlas,
        MenuController menu,
        GameSettings settings,
        int screenW,
        int screenH)
    {
        DrawBackdrop(spriteBatch, atlas, screenW, screenH);

        var title = "CONFIGURACOES";
        var titleX = (screenW - GlyphFont.MeasureWidth(title)) / 2;
        GlyphFont.Draw(spriteBatch, atlas.Pixel, title, titleX, 48, new Color(255, 220, 120));

        DrawItemList(
            spriteBatch,
            atlas,
            menu.SettingsLabels,
            menu.SettingsIndex,
            screenW,
            startY: 140,
            i => i switch
            {
                0 => settings.Fullscreen ? "SIM" : "NAO",
                1 => settings.Fullscreen ? "AJUSTE" : $"{settings.ClampedWindowScale}X",
                _ => null,
            });
    }

    public static void DrawPause(
        SpriteBatch spriteBatch,
        TextureAtlas atlas,
        MenuController menu,
        int screenW,
        int screenH)
    {
        spriteBatch.Draw(atlas.Pixel, new Rectangle(0, 0, screenW, screenH), new Color(0, 0, 0, 160));

        var title = "PAUSE";
        var titleX = (screenW - GlyphFont.MeasureWidth(title)) / 2;
        GlyphFont.Draw(spriteBatch, atlas.Pixel, title, titleX, 100, new Color(255, 220, 120));

        DrawItemList(
            spriteBatch,
            atlas,
            menu.PauseLabels,
            menu.PauseIndex,
            screenW,
            startY: 160,
            _ => null);
    }

    private static void DrawBackdrop(SpriteBatch spriteBatch, TextureAtlas atlas, int w, int h)
    {
        spriteBatch.Draw(atlas.Pixel, new Rectangle(0, 0, w, h), new Color(12, 14, 22));
        spriteBatch.Draw(atlas.Pixel, new Rectangle(24, 24, w - 48, h - 48), new Color(22, 26, 40));
        spriteBatch.Draw(atlas.Pixel, new Rectangle(24, 24, w - 48, 3), new Color(80, 120, 180));
    }

    private static void DrawItemList(
        SpriteBatch spriteBatch,
        TextureAtlas atlas,
        IReadOnlyList<string> labels,
        int selected,
        int screenW,
        int startY,
        Func<int, string?> valueForIndex)
    {
        for (var i = 0; i < labels.Count; i++)
        {
            var selectedRow = i == selected;
            var label = labels[i];
            var value = valueForIndex(i);
            var text = value is null ? label : $"{label}: {value}";
            var color = selectedRow ? Color.White : new Color(160, 170, 190);
            var width = GlyphFont.MeasureWidth(text);
            var x = (screenW - width) / 2;
            var y = startY + i * 36;

            if (selectedRow)
            {
                spriteBatch.Draw(
                    atlas.Pixel,
                    new Rectangle(x - 16, y - 4, width + 32, GlyphFont.CellH + 8),
                    new Color(40, 70, 120, 180));
                GlyphFont.Draw(spriteBatch, atlas.Pixel, ">", x - 14, y, new Color(120, 220, 255));
            }

            GlyphFont.Draw(spriteBatch, atlas.Pixel, text, x, y, color);
        }

        var hint = "W/S  ENTER  ESC";
        var hx = (screenW - GlyphFont.MeasureWidth(hint, scale: 1)) / 2;
        GlyphFont.Draw(
            spriteBatch,
            atlas.Pixel,
            hint,
            hx,
            startY + labels.Count * 36 + 40,
            new Color(100, 110, 130),
            scale: 1);
    }
}
