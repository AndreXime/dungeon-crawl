#nullable enable
using System;
using ChamberCrawl.Models;
using ChamberCrawl.Rendering;
using ChamberCrawl.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ChamberCrawl;

public class Game1 : Game
{
    private const int HudMargin = 24;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private TextureAtlas _atlas = null!;
    private RunState _state = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = GameConstants.RoomW * GameConstants.Cell;
        _graphics.PreferredBackBufferHeight = GameConstants.RoomH * GameConstants.Cell + HudMargin;
    }

    protected override void Initialize()
    {
        _state = RunFactory.Create(Environment.TickCount);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _atlas = TextureAtlas.Load(GraphicsDevice, AppContext.BaseDirectory);
    }

    protected override void UnloadContent()
    {
        _atlas.Dispose();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var dt = MathF.Min(0.05f, (float)gameTime.ElapsedGameTime.TotalSeconds);
        var input = InputSystem.Poll();
        if (_state.Status != RunStatus.Playing)
        {
            if (input.Restart)
                _state = RunFactory.Create(Environment.TickCount);
        }
        else
        {
            _state.TimeMs += dt * 1000f;
            Movement.Update(_state, input, dt);
            Ai.Update(_state, dt);
            Combat.Update(_state, input, dt);
            Pickups.Update(_state);
            Doors.Update(_state);
            Progression.Update(_state);
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        GameRenderer.Draw(_spriteBatch, _state, _atlas);
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
