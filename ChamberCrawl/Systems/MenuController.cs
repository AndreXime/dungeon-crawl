using ChamberCrawl.Data;
using ChamberCrawl.Models;
using Microsoft.Xna.Framework.Input;

namespace ChamberCrawl.Systems;

public sealed class MenuController
{
    private KeyboardState _prev;
    private int _mainIndex;
    private int _settingsIndex;
    private int _pauseIndex;

    private static readonly string[] MainItems = ["COMECAR", "CONFIGURACOES", "SAIR"];
    private static readonly string[] SettingsItems = ["TELA CHEIA", "VOLTAR"];
    private static readonly string[] PauseItems = ["CONTINUAR", "MENU"];

    public int MainIndex => _mainIndex;
    public int SettingsIndex => _settingsIndex;
    public int PauseIndex => _pauseIndex;
    public IReadOnlyList<string> MainLabels => MainItems;
    public IReadOnlyList<string> SettingsLabels => SettingsItems;
    public IReadOnlyList<string> PauseLabels => PauseItems;

    public void ResetSelection()
    {
        _mainIndex = 0;
        _settingsIndex = 0;
        _pauseIndex = 0;
    }

    public void ResetPauseSelection() => _pauseIndex = 0;

    public void SyncInput(KeyboardState kb) => _prev = kb;

    public AppScreen Update(
        AppScreen screen,
        GameSettings settings,
        Action applyDisplay,
        Action startGame,
        Action exitGame,
        KeyboardState kb)
    {
        switch (screen)
        {
            case AppScreen.MainMenu:
                return UpdateMain(kb, startGame, exitGame, screen);
            case AppScreen.Settings:
                return UpdateSettings(kb, settings, applyDisplay, screen);
            default:
                _prev = kb;
                return screen;
        }
    }

    public AppScreen UpdatePause(
        AppScreen screen,
        Action continueGame,
        Action returnToMenu,
        KeyboardState kb)
    {
        if (Pressed(kb, Keys.Escape))
        {
            _prev = kb;
            continueGame();
            return AppScreen.Playing;
        }

        if (Pressed(kb, Keys.Down) || Pressed(kb, Keys.S))
            _pauseIndex = (_pauseIndex + 1) % PauseItems.Length;
        if (Pressed(kb, Keys.Up) || Pressed(kb, Keys.W))
            _pauseIndex = (_pauseIndex + PauseItems.Length - 1) % PauseItems.Length;

        if (Pressed(kb, Keys.Enter) || Pressed(kb, Keys.Space))
        {
            _prev = kb;
            switch (_pauseIndex)
            {
                case 0:
                    continueGame();
                    return AppScreen.Playing;
                case 1:
                    returnToMenu();
                    return AppScreen.MainMenu;
            }
        }

        _prev = kb;
        return screen;
    }

    private AppScreen UpdateMain(
        KeyboardState kb,
        Action startGame,
        Action exitGame,
        AppScreen screen)
    {
        if (Pressed(kb, Keys.Down) || Pressed(kb, Keys.S))
            _mainIndex = (_mainIndex + 1) % MainItems.Length;
        if (Pressed(kb, Keys.Up) || Pressed(kb, Keys.W))
            _mainIndex = (_mainIndex + MainItems.Length - 1) % MainItems.Length;

        if (Pressed(kb, Keys.Enter) || Pressed(kb, Keys.Space))
        {
            switch (_mainIndex)
            {
                case 0:
                    startGame();
                    _prev = kb;
                    return AppScreen.Playing;
                case 1:
                    _settingsIndex = 0;
                    _prev = kb;
                    return AppScreen.Settings;
                case 2:
                    exitGame();
                    break;
            }
        }

        _prev = kb;
        return screen;
    }

    private AppScreen UpdateSettings(
        KeyboardState kb,
        GameSettings settings,
        Action applyDisplay,
        AppScreen screen)
    {
        if (Pressed(kb, Keys.Escape))
        {
            _prev = kb;
            return AppScreen.MainMenu;
        }

        if (Pressed(kb, Keys.Down) || Pressed(kb, Keys.S))
            _settingsIndex = (_settingsIndex + 1) % SettingsItems.Length;
        if (Pressed(kb, Keys.Up) || Pressed(kb, Keys.W))
            _settingsIndex = (_settingsIndex + SettingsItems.Length - 1) % SettingsItems.Length;

        if (_settingsIndex == 0 &&
            (Pressed(kb, Keys.Left) || Pressed(kb, Keys.Right) ||
             Pressed(kb, Keys.Enter) || Pressed(kb, Keys.Space) ||
             Pressed(kb, Keys.A) || Pressed(kb, Keys.D)))
        {
            settings.Fullscreen = !settings.Fullscreen;
            settings.Save();
            applyDisplay();
        }

        if (_settingsIndex == 1 && (Pressed(kb, Keys.Enter) || Pressed(kb, Keys.Space)))
        {
            _prev = kb;
            return AppScreen.MainMenu;
        }

        _prev = kb;
        return screen;
    }

    private bool Pressed(KeyboardState kb, Keys key) =>
        kb.IsKeyDown(key) && _prev.IsKeyUp(key);
}
