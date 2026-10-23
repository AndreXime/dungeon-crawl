namespace ChamberCrawl.Audio;

/// <summary>Facade estática para sistemas tocarem SFX sem carregar MonoGame nos testes.</summary>
public static class Sfx
{
    public static SoundBank? Bank { get; set; }

    public static void Play(string name, float volume = 0.7f) =>
        Bank?.Play(name, volume);
}
