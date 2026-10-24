using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Movement
{
    public const float SlowZoneSpeedFactor = 0.5f;

    public static void Update(RunState state, InputSnapshot input, float dtSeconds)
    {
        var player = state.Entities.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return;

        var speed = state.PlayerStats.Speed * SpeedScale(player.X, player.Y, state.Room.Tiles);
        player.Vx = input.MoveX * speed;
        player.Vy = input.MoveY * speed;

        var nextX = player.X + player.Vx * dtSeconds;
        var nextY = player.Y + player.Vy * dtSeconds;
        var tiles = state.Room.Tiles;
        var radius = GameConstants.PlayerRadius;

        if (!Collision.CircleHitsSolid(nextX, nextY, radius, tiles))
        {
            player.X = nextX;
            player.Y = nextY;
            return;
        }

        if (!Collision.CircleHitsSolid(nextX, player.Y, radius, tiles)) player.X = nextX;
        else player.Vx = 0;

        if (!Collision.CircleHitsSolid(player.X, nextY, radius, tiles)) player.Y = nextY;
        else player.Vy = 0;
    }

    public static float SpeedScale(float x, float y, int[,] tiles, EnemyKind? kind = null)
    {
        if (kind is EnemyKind.Slime or EnemyKind.SlimeKing) return 1f;
        return Collision.IsOnSlowTile(x, y, tiles) ? SlowZoneSpeedFactor : 1f;
    }
}
