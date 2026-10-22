using ChamberCrawl.Data;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;
using Microsoft.Xna.Framework.Input;

namespace ChamberCrawl.Tests;

public class MenuControllerTests
{
    [Fact]
    public void MainMenu_Escape_DoesNotExit()
    {
        var menu = new MenuController();
        var exited = false;
        var settings = new GameSettings();

        menu.Update(AppScreen.MainMenu, settings, () => { }, () => { }, () => { }, new KeyboardState());
        var screen = menu.Update(
            AppScreen.MainMenu,
            settings,
            () => { },
            () => { },
            () => exited = true,
            new KeyboardState(Keys.Escape));

        Assert.Equal(AppScreen.MainMenu, screen);
        Assert.False(exited);
    }

    [Fact]
    public void Pause_Escape_ReturnsToPlaying()
    {
        var menu = new MenuController();
        menu.UpdatePause(AppScreen.Paused, () => { }, () => { }, new KeyboardState());
        var screen = menu.UpdatePause(
            AppScreen.Paused,
            () => { },
            () => { },
            new KeyboardState(Keys.Escape));

        Assert.Equal(AppScreen.Playing, screen);
    }

    [Fact]
    public void Pause_Continuar_ReturnsToPlaying()
    {
        var menu = new MenuController();
        menu.ResetPauseSelection();
        menu.UpdatePause(AppScreen.Paused, () => { }, () => { }, new KeyboardState());
        var screen = menu.UpdatePause(
            AppScreen.Paused,
            () => { },
            () => { },
            new KeyboardState(Keys.Enter));

        Assert.Equal(AppScreen.Playing, screen);
    }

    [Fact]
    public void Pause_Menu_ReturnsToMainMenu()
    {
        var menu = new MenuController();
        menu.ResetPauseSelection();
        menu.UpdatePause(AppScreen.Paused, () => { }, () => { }, new KeyboardState());
        menu.UpdatePause(AppScreen.Paused, () => { }, () => { }, new KeyboardState(Keys.Down));
        menu.UpdatePause(AppScreen.Paused, () => { }, () => { }, new KeyboardState());

        var returned = false;
        var screen = menu.UpdatePause(
            AppScreen.Paused,
            () => { },
            () => returned = true,
            new KeyboardState(Keys.Enter));

        Assert.Equal(AppScreen.MainMenu, screen);
        Assert.True(returned);
    }

    [Fact]
    public void Settings_RightOnScale_Cycles1To4()
    {
        var menu = new MenuController();
        var settings = new GameSettings { WindowScale = 2 };
        var applied = 0;
        void Apply() => applied++;

        menu.Update(AppScreen.Settings, settings, Apply, () => { }, () => { }, new KeyboardState());
        menu.Update(AppScreen.Settings, settings, Apply, () => { }, () => { }, new KeyboardState(Keys.Down));
        menu.Update(AppScreen.Settings, settings, Apply, () => { }, () => { }, new KeyboardState());
        menu.Update(AppScreen.Settings, settings, Apply, () => { }, () => { }, new KeyboardState(Keys.Right));

        Assert.Equal(3, settings.WindowScale);
        Assert.True(applied >= 1);

        menu.Update(AppScreen.Settings, settings, Apply, () => { }, () => { }, new KeyboardState());
        menu.Update(AppScreen.Settings, settings, Apply, () => { }, () => { }, new KeyboardState(Keys.Right));
        menu.Update(AppScreen.Settings, settings, Apply, () => { }, () => { }, new KeyboardState());
        menu.Update(AppScreen.Settings, settings, Apply, () => { }, () => { }, new KeyboardState(Keys.Right));
        Assert.Equal(1, settings.WindowScale);
    }

    [Fact]
    public void Settings_Scale_IgnoresInputWhenFullscreen()
    {
        var menu = new MenuController();
        var settings = new GameSettings { Fullscreen = true, WindowScale = 2 };
        var applied = 0;

        menu.Update(AppScreen.Settings, settings, () => applied++, () => { }, () => { }, new KeyboardState());
        menu.Update(AppScreen.Settings, settings, () => applied++, () => { }, () => { }, new KeyboardState(Keys.Down));
        menu.Update(AppScreen.Settings, settings, () => applied++, () => { }, () => { }, new KeyboardState());
        menu.Update(AppScreen.Settings, settings, () => applied++, () => { }, () => { }, new KeyboardState(Keys.Right));

        Assert.Equal(2, settings.WindowScale);
        Assert.Equal(0, applied);
    }
}
