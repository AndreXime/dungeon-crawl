using System;
using Microsoft.Xna.Framework.Input;

namespace ChamberCrawl.Systems;

public readonly record struct InputSnapshot(float MoveX, float MoveY, float ShootX, float ShootY, bool Restart);

public static class InputSystem
{
    public static InputSnapshot Poll() => FromKeyboardState(Keyboard.GetState());

    public static InputSnapshot FromKeyboardState(KeyboardState keyboard)
    {
        var (moveX, moveY) = Normalize(
            Axis(keyboard, Keys.A, Keys.D),
            Axis(keyboard, Keys.W, Keys.S));
        var (shootX, shootY) = Normalize(
            Axis(keyboard, Keys.Left, Keys.Right),
            Axis(keyboard, Keys.Up, Keys.Down));
        return new InputSnapshot(moveX, moveY, shootX, shootY, keyboard.IsKeyDown(Keys.R));
    }

    private static float Axis(KeyboardState keyboard, Keys negative, Keys positive) =>
        (keyboard.IsKeyDown(positive) ? 1 : 0) - (keyboard.IsKeyDown(negative) ? 1 : 0);

    private static (float X, float Y) Normalize(float x, float y)
    {
        var length = MathF.Sqrt(x * x + y * y);
        return length == 0 ? (0, 0) : (x / length, y / length);
    }
}
