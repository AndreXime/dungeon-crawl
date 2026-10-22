using System;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Collision
{
    private const int SampleCount = 8;

    public static bool CirclesOverlap(float x1, float y1, float r1, float x2, float y2, float r2)
    {
        var dx = x1 - x2;
        var dy = y1 - y2;
        var radiusSum = r1 + r2;
        return dx * dx + dy * dy < radiusSum * radiusSum;
    }

    public static bool CircleHitsSolid(float x, float y, float radius, int[,] tiles)
    {
        for (var i = 0; i < SampleCount; i++)
        {
            var angle = MathF.PI * 0.25f * i;
            var tx = (int)MathF.Floor((x + MathF.Cos(angle) * radius) / GameConstants.Cell);
            var ty = (int)MathF.Floor((y + MathF.Sin(angle) * radius) / GameConstants.Cell);
            if (IsTileSolid(tx, ty, tiles)) return true;
        }
        return false;
    }

    private static bool IsTileSolid(int tx, int ty, int[,] tiles)
    {
        if (ty < 0 || ty >= tiles.GetLength(0) || tx < 0 || tx >= tiles.GetLength(1)) return true;
        var tile = tiles[ty, tx];
        return tile == GameConstants.TileWall || tile == GameConstants.TileObstacle;
    }
}
