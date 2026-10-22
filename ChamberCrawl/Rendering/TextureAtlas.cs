#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChamberCrawl.Rendering;

public sealed class TextureAtlas : IDisposable
{
    private readonly Dictionary<string, Texture2D> _byName;
    private readonly Texture2D[] _digits;
    private bool _disposed;

    public Texture2D Pixel { get; }

    private TextureAtlas(
        Dictionary<string, Texture2D> byName,
        Texture2D pixel,
        Texture2D[] digits)
    {
        _byName = byName;
        Pixel = pixel;
        _digits = digits;
    }

    public static TextureAtlas Load(GraphicsDevice device, string contentRootPath)
    {
        var texturesDir = Path.Combine(contentRootPath, "Assets", "Textures");
        if (!Directory.Exists(texturesDir))
            throw new DirectoryNotFoundException($"Texturas não encontradas: {texturesDir}");

        var byName = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.EnumerateFiles(texturesDir, "*.png"))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            using var stream = File.OpenRead(path);
            byName[name] = Texture2D.FromStream(device, stream);
        }

        var pixel = new Texture2D(device, 1, 1);
        pixel.SetData(new[] { Color.White });

        var digits = new Texture2D[10];
        for (var d = 0; d < 10; d++)
            digits[d] = CreateDigit(device, d);

        return new TextureAtlas(byName, pixel, digits);
    }

    public Texture2D Get(string name)
    {
        if (_byName.TryGetValue(name, out var tex)) return tex;
        throw new KeyNotFoundException($"Textura ausente: {name}");
    }

    public bool TryGet(string name, out Texture2D texture) =>
        _byName.TryGetValue(name, out texture!);

    public Texture2D Digit(int value)
    {
        var d = Math.Clamp(value, 0, 9);
        return _digits[d];
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var tex in _byName.Values) tex.Dispose();
        foreach (var dig in _digits) dig.Dispose();
        Pixel.Dispose();
    }

    private static Texture2D CreateDigit(GraphicsDevice device, int digit)
    {
        // 5x7 monochrome glyphs, scaled to 10x14 textures.
        ReadOnlySpan<string> glyphs =
        [
            "01110 10001 10001 10001 10001 10001 01110",
            "00100 01100 00100 00100 00100 00100 01110",
            "01110 10001 00001 00110 01000 10000 11111",
            "01110 10001 00001 00110 00001 10001 01110",
            "00010 00110 01010 10010 11111 00010 00010",
            "11111 10000 11110 00001 00001 10001 01110",
            "01110 10000 11110 10001 10001 10001 01110",
            "11111 00001 00010 00100 01000 01000 01000",
            "01110 10001 10001 01110 10001 10001 01110",
            "01110 10001 10001 01111 00001 00001 01110",
        ];

        const int gw = 5, gh = 7, scale = 2;
        var tex = new Texture2D(device, gw * scale, gh * scale);
        var data = new Color[gw * scale * gh * scale];
        var rows = glyphs[digit].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var y = 0; y < gh; y++)
        {
            var row = rows[y];
            for (var x = 0; x < gw; x++)
            {
                if (row[x] != '1') continue;
                for (var sy = 0; sy < scale; sy++)
                for (var sx = 0; sx < scale; sx++)
                    data[(y * scale + sy) * gw * scale + (x * scale + sx)] = Color.White;
            }
        }
        tex.SetData(data);
        return tex;
    }
}
