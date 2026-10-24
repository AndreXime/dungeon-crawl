using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChamberCrawl.Rendering;

/// <summary>Fonte bitmap 5x7 para textos de menu (A-Z, 0-9, espaço e pontuação básica).</summary>
public static class GlyphFont
{
    public const int GlyphW = 5;
    public const int GlyphH = 7;
    public const int Scale = 2;
    public const int CellW = GlyphW * Scale + 2;
    public const int CellH = GlyphH * Scale;

    private static readonly Dictionary<char, string[]> Glyphs = BuildGlyphs();

    public static int MeasureWidth(string text, int scale = Scale)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        var cell = GlyphW * scale + 2;
        return text.Length * cell - 2;
    }

    public static void Draw(
        SpriteBatch spriteBatch,
        Texture2D pixel,
        string text,
        int x,
        int y,
        Color color,
        int scale = Scale)
    {
        var cursor = x;
        var cell = GlyphW * scale + 2;
        foreach (var raw in text.ToUpperInvariant())
        {
            var ch = Normalize(raw);
            if (Glyphs.TryGetValue(ch, out var rows))
                DrawGlyph(spriteBatch, pixel, rows, cursor, y, color, scale);
            cursor += cell;
        }
    }

    private static void DrawGlyph(
        SpriteBatch spriteBatch,
        Texture2D pixel,
        string[] rows,
        int x,
        int y,
        Color color,
        int scale)
    {
        for (var row = 0; row < rows.Length; row++)
        {
            var line = rows[row];
            for (var col = 0; col < line.Length; col++)
            {
                if (line[col] != '1') continue;
                spriteBatch.Draw(
                    pixel,
                    new Rectangle(x + col * scale, y + row * scale, scale, scale),
                    color);
            }
        }
    }

    private static char Normalize(char ch) => ch switch
    {
        'Á' or 'À' or 'Â' or 'Ã' => 'A',
        'É' or 'Ê' => 'E',
        'Í' => 'I',
        'Ó' or 'Ô' or 'Õ' => 'O',
        'Ú' or 'Ü' => 'U',
        'Ç' => 'C',
        _ => ch,
    };

    private static Dictionary<char, string[]> BuildGlyphs()
    {
        var map = new Dictionary<char, string[]>();
        void Add(char c, params string[] rows) => map[c] = rows;

        Add(' ', "00000", "00000", "00000", "00000", "00000", "00000", "00000");
        Add(':', "00000", "00100", "00100", "00000", "00100", "00100", "00000");
        Add('.', "00000", "00000", "00000", "00000", "00000", "01100", "01100");
        Add('-', "00000", "00000", "00000", "11111", "00000", "00000", "00000");
        Add('?', "01110", "10001", "00001", "00110", "00100", "00000", "00100");

        Add('0', "01110", "10001", "10001", "10001", "10001", "10001", "01110");
        Add('1', "00100", "01100", "00100", "00100", "00100", "00100", "01110");
        Add('2', "01110", "10001", "00001", "00110", "01000", "10000", "11111");
        Add('3', "01110", "10001", "00001", "00110", "00001", "10001", "01110");
        Add('4', "00010", "00110", "01010", "10010", "11111", "00010", "00010");
        Add('5', "11111", "10000", "11110", "00001", "00001", "10001", "01110");
        Add('6', "01110", "10000", "11110", "10001", "10001", "10001", "01110");
        Add('7', "11111", "00001", "00010", "00100", "01000", "01000", "01000");
        Add('8', "01110", "10001", "10001", "01110", "10001", "10001", "01110");
        Add('9', "01110", "10001", "10001", "01111", "00001", "00001", "01110");

        Add('A', "01110", "10001", "10001", "11111", "10001", "10001", "10001");
        Add('B', "11110", "10001", "10001", "11110", "10001", "10001", "11110");
        Add('C', "01110", "10001", "10000", "10000", "10000", "10001", "01110");
        Add('D', "11110", "10001", "10001", "10001", "10001", "10001", "11110");
        Add('E', "11111", "10000", "10000", "11110", "10000", "10000", "11111");
        Add('F', "11111", "10000", "10000", "11110", "10000", "10000", "10000");
        Add('G', "01110", "10001", "10000", "10111", "10001", "10001", "01110");
        Add('H', "10001", "10001", "10001", "11111", "10001", "10001", "10001");
        Add('I', "01110", "00100", "00100", "00100", "00100", "00100", "01110");
        Add('J', "00111", "00010", "00010", "00010", "00010", "10010", "01100");
        Add('K', "10001", "10010", "10100", "11000", "10100", "10010", "10001");
        Add('L', "10000", "10000", "10000", "10000", "10000", "10000", "11111");
        Add('M', "10001", "11011", "10101", "10001", "10001", "10001", "10001");
        Add('N', "10001", "11001", "10101", "10011", "10001", "10001", "10001");
        Add('O', "01110", "10001", "10001", "10001", "10001", "10001", "01110");
        Add('P', "11110", "10001", "10001", "11110", "10000", "10000", "10000");
        Add('Q', "01110", "10001", "10001", "10001", "10101", "10010", "01101");
        Add('R', "11110", "10001", "10001", "11110", "10100", "10010", "10001");
        Add('S', "01110", "10001", "10000", "01110", "00001", "10001", "01110");
        Add('T', "11111", "00100", "00100", "00100", "00100", "00100", "00100");
        Add('U', "10001", "10001", "10001", "10001", "10001", "10001", "01110");
        Add('V', "10001", "10001", "10001", "10001", "10001", "01010", "00100");
        Add('W', "10001", "10001", "10001", "10001", "10101", "11011", "10001");
        Add('X', "10001", "10001", "01010", "00100", "01010", "10001", "10001");
        Add('Y', "10001", "10001", "01010", "00100", "00100", "00100", "00100");
        Add('Z', "11111", "00001", "00010", "00100", "01000", "10000", "11111");

        return map;
    }
}
