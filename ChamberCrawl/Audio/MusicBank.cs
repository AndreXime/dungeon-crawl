using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace ChamberCrawl.Audio;

public sealed class MusicBank : IDisposable
{
    private readonly Dictionary<string, SoundEffect> _effects = new(StringComparer.OrdinalIgnoreCase);
    private SoundEffectInstance? _current;
    private string? _currentName;
    private bool _disposed;

    public static MusicBank Load(string contentRootPath)
    {
        var musicDir = Path.Combine(contentRootPath, "Assets", "Music");
        var bank = new MusicBank();
        if (!Directory.Exists(musicDir))
            return bank;

        foreach (var path in Directory.EnumerateFiles(musicDir, "*.wav", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            using var stream = File.OpenRead(path);
            bank._effects[name] = SoundEffect.FromStream(stream);
        }

        return bank;
    }

    public void PlayLoop(string name, float volume = 0.55f)
    {
        if (_disposed) return;
        if (string.Equals(_currentName, name, StringComparison.OrdinalIgnoreCase)
            && _current is { State: SoundState.Playing or SoundState.Paused })
        {
            _current.Volume = volume;
            if (_current.State == SoundState.Paused)
                _current.Resume();
            return;
        }

        Stop();
        if (!_effects.TryGetValue(name, out var effect))
            return;

        _current = effect.CreateInstance();
        _current.IsLooped = true;
        _current.Volume = volume;
        _current.Play();
        _currentName = name;
    }

    public void Pause()
    {
        if (_current?.State == SoundState.Playing)
            _current.Pause();
    }

    public void Resume()
    {
        if (_current?.State == SoundState.Paused)
            _current.Resume();
    }

    public void Stop()
    {
        if (_current is null) return;
        _current.Stop();
        _current.Dispose();
        _current = null;
        _currentName = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        foreach (var effect in _effects.Values)
            effect.Dispose();
        _effects.Clear();
    }
}
