using ChamberCrawl.Data;
using ChamberCrawl.Models;
using ChamberCrawl.Rendering;
using ChamberCrawl.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChamberCrawl;

public class Game1 : Game
{
    public const int HudMargin = 24;
    public static int VirtualWidth => GameConstants.RoomW * GameConstants.Cell;
    public static int VirtualHeight => GameConstants.RoomH * GameConstants.Cell + HudMargin;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private TextureAtlas _atlas = null!;
    private RenderTarget2D _scene = null!;
    private RunState _state = null!;
    private GameSettings _settings = null!;
    private readonly MenuController _menu = new();
    private AppScreen _screen = AppScreen.MainMenu;
    private KeyboardState _prevKeyboard;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = VirtualWidth * GameSettings.DefaultWindowScale;
        _graphics.PreferredBackBufferHeight = VirtualHeight * GameSettings.DefaultWindowScale;
        _graphics.HardwareModeSwitch = false;
        Window.AllowUserResizing = false;
    }

    protected override void Initialize()
    {
        Window.Title = "Diskette";
        _settings = GameSettings.Load();
        ApplyDisplaySettings();
        _state = RunFactory.Create(Environment.TickCount);
        _menu.ResetSelection();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _atlas = TextureAtlas.Load(GraphicsDevice, AppContext.BaseDirectory);
        EnsureSceneTarget();
    }

    protected override void UnloadContent()
    {
        _scene?.Dispose();
        _atlas.Dispose();
    }

    protected override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        var dt = MathF.Min(0.05f, (float)gameTime.ElapsedGameTime.TotalSeconds);

        switch (_screen)
        {
            case AppScreen.MainMenu:
            case AppScreen.Settings:
                _screen = _menu.Update(
                    _screen,
                    _settings,
                    ApplyDisplaySettings,
                    StartRun,
                    Exit,
                    kb);
                break;

            case AppScreen.Playing:
                UpdatePlaying(kb, dt);
                break;

            case AppScreen.Paused:
                _screen = _menu.UpdatePause(_screen, () => { }, ReturnToMenu, kb);
                break;
        }

        _prevKeyboard = kb;
        base.Update(gameTime);
    }

    private void UpdatePlaying(KeyboardState kb, float dt)
    {
        if (Pressed(kb, Keys.Escape))
        {
            _menu.ResetPauseSelection();
            _menu.SyncInput(kb);
            _screen = AppScreen.Paused;
            return;
        }

        var input = InputSystem.Poll();
        if (_state.Status != RunStatus.Playing)
        {
            if (input.Restart)
                _state = RunFactory.Create(Environment.TickCount);
            return;
        }

        _state.TimeMs += dt * 1000f;
        Movement.Update(_state, input, dt);
        Ai.Update(_state, dt);
        Combat.Update(_state, input, dt);
        Pickups.Update(_state);
        Doors.Update(_state);
        Progression.Update(_state);
    }

    private void StartRun()
    {
        _state = RunFactory.Create(Environment.TickCount);
        _screen = AppScreen.Playing;
    }

    private void ReturnToMenu()
    {
        _menu.ResetSelection();
        _screen = AppScreen.MainMenu;
    }

    private void ApplyDisplaySettings()
    {
        if (_settings.Fullscreen)
        {
            var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth = display.Width;
            _graphics.PreferredBackBufferHeight = display.Height;
            _graphics.IsFullScreen = true;
        }
        else
        {
            var scale = _settings.ClampedWindowScale;
            _graphics.PreferredBackBufferWidth = VirtualWidth * scale;
            _graphics.PreferredBackBufferHeight = VirtualHeight * scale;
            _graphics.IsFullScreen = false;
        }

        _graphics.ApplyChanges();
        if (GraphicsDevice is not null)
            EnsureSceneTarget();
    }

    private void EnsureSceneTarget()
    {
        if (_scene is not null &&
            _scene.Width == VirtualWidth &&
            _scene.Height == VirtualHeight &&
            !_scene.IsDisposed)
            return;

        _scene?.Dispose();
        _scene = new RenderTarget2D(
            GraphicsDevice,
            VirtualWidth,
            VirtualHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None);
    }

    private bool Pressed(KeyboardState kb, Keys key) =>
        kb.IsKeyDown(key) && _prevKeyboard.IsKeyUp(key);

    protected override void Draw(GameTime gameTime)
    {
        EnsureSceneTarget();

        GraphicsDevice.SetRenderTarget(_scene);
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        switch (_screen)
        {
            case AppScreen.MainMenu:
                MenuRenderer.DrawMain(_spriteBatch, _atlas, _menu, VirtualWidth, VirtualHeight);
                break;
            case AppScreen.Settings:
                MenuRenderer.DrawSettings(_spriteBatch, _atlas, _menu, _settings, VirtualWidth, VirtualHeight);
                break;
            case AppScreen.Playing:
                GameRenderer.Draw(_spriteBatch, _state, _atlas);
                break;
            case AppScreen.Paused:
                GameRenderer.Draw(_spriteBatch, _state, _atlas);
                MenuRenderer.DrawPause(_spriteBatch, _atlas, _menu, VirtualWidth, VirtualHeight);
                break;
        }

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        var dest = FitSceneDestination(
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height,
            VirtualWidth,
            VirtualHeight,
            integerScale: !_settings.Fullscreen);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_scene, dest, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    /// <summary>
    /// Janela: escala inteira (1x-4x). Tela cheia: maior escala contínua sem cortar o conteúdo.
    /// </summary>
    public static Rectangle FitSceneDestination(
        int backW,
        int backH,
        int virtualW,
        int virtualH,
        bool integerScale)
    {
        if (virtualW <= 0 || virtualH <= 0 || backW <= 0 || backH <= 0)
            return Rectangle.Empty;

        float scale;
        if (integerScale)
            scale = Math.Max(1, Math.Min(backW / virtualW, backH / virtualH));
        else
            scale = Math.Min((float)backW / virtualW, (float)backH / virtualH);

        var destW = Math.Min(backW, (int)MathF.Round(virtualW * scale));
        var destH = Math.Min(backH, (int)MathF.Round(virtualH * scale));
        return new Rectangle((backW - destW) / 2, (backH - destH) / 2, destW, destH);
    }
}
