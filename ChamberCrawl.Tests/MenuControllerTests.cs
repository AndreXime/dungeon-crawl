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
}
