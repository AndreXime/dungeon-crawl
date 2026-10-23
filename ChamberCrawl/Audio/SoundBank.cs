using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace ChamberCrawl.Audio;

public sealed class SoundBank : IDisposable
{
    private readonly Dictionary<string, SoundEffect> _effects = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public static SoundBank Load(string contentRootPath)
    {
        var audioDir = Path.Combine(contentRootPath, "Assets", "Audio");
        var bank = new SoundBank();
        if (!Directory.Exists(audioDir))
            return bank;

        foreach (var path in Directory.EnumerateFiles(audioDir, "*.wav", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            using var stream = File.OpenRead(path);
            bank._effects[name] = SoundEffect.FromStream(stream);
        }

        return bank;
    }

    public void Play(string name, float volume = 0.7f)
    {
        if (_disposed) return;
        if (!_effects.TryGetValue(name, out var effect)) return;
        effect.Play(volume, 0f, 0f);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var effect in _effects.Values)
            effect.Dispose();
        _effects.Clear();
    }
}
