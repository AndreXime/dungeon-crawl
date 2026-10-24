using System;
using System.Collections.Generic;
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

    /// <summary>True se o trajeto entre dois pontos não cruza tile sólido (ex.: tiro).</summary>
    public static bool HasLineOfSight(
        float x0, float y0, float x1, float y1, float radius, int[,] tiles,
        IEnumerable<Entity>? entities = null)
    {
        var dx = x1 - x0;
        var dy = y1 - y0;
        var dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist < 0.001f) return true;
        var samples = Math.Max(4, (int)MathF.Ceiling(dist / (GameConstants.Cell * 0.25f)));
        for (var i = 1; i < samples; i++)
        {
            var t = (float)i / samples;
            if (CircleHitsSolid(x0 + dx * t, y0 + dy * t, radius, tiles, entities))
                return false;
        }
        return true;
    }

    public static bool CircleHitsSolid(float x, float y, float radius, int[,] tiles) =>
        CircleHitsSolid(x, y, radius, tiles, entities: null);

    public static bool CircleHitsSolid(
        float x, float y, float radius, int[,] tiles, IEnumerable<Entity>? entities)
    {
        for (var i = 0; i < SampleCount; i++)
        {
            var angle = MathF.PI * 0.25f * i;
            var tx = (int)MathF.Floor((x + MathF.Cos(angle) * radius) / GameConstants.Cell);
            var ty = (int)MathF.Floor((y + MathF.Sin(angle) * radius) / GameConstants.Cell);
            if (IsTileSolid(tx, ty, tiles)) return true;
        }

        if (entities is null) return false;
        foreach (var entity in entities)
        {
            if (entity is not DoorEntity { Locked: true } door) continue;
            if (CirclesOverlap(x, y, radius, door.X, door.Y, door.Radius))
                return true;
        }
        return false;
    }

    public static bool IsSolidTile(int tile) =>
        tile is GameConstants.TileWall
            or GameConstants.TileObstacle
            or GameConstants.TileCrate
            or GameConstants.TileBarrel
            or GameConstants.TileSpike
            or GameConstants.TilePedestal
            or GameConstants.TileBarricade
            or GameConstants.TileAnvil;

    public static bool IsSlowTile(int tile) => tile == GameConstants.TilePuddle;

    public static bool IsDamageTile(int tile) => tile == GameConstants.TileRune;

    public static bool IsWalkableFloor(int tile) =>
        tile is GameConstants.TileFloor
            or GameConstants.TilePuddle
            or GameConstants.TileRune
            or GameConstants.TileDoor;

    public static bool IsOnSlowTile(float x, float y, int[,] tiles) =>
        TileAt(x, y, tiles) is { } t && IsSlowTile(t);

    public static bool IsOnDamageTile(float x, float y, int[,] tiles) =>
        TileAt(x, y, tiles) is { } t && IsDamageTile(t);

    private static int? TileAt(float x, float y, int[,] tiles)
    {
        var tx = (int)MathF.Floor(x / GameConstants.Cell);
        var ty = (int)MathF.Floor(y / GameConstants.Cell);
        if (ty < 0 || ty >= tiles.GetLength(0) || tx < 0 || tx >= tiles.GetLength(1)) return null;
        return tiles[ty, tx];
    }

    private static bool IsTileSolid(int tx, int ty, int[,] tiles)
    {
        if (ty < 0 || ty >= tiles.GetLength(0) || tx < 0 || tx >= tiles.GetLength(1)) return true;
        return IsSolidTile(tiles[ty, tx]);
    }

    public static bool TrySampleSolidTile(
        float x, float y, float radius, int[,] tiles, out int tx, out int ty, out int tile)
    {
        tx = ty = 0;
        tile = GameConstants.TileFloor;
        for (var i = 0; i < SampleCount; i++)
        {
            var angle = MathF.PI * 0.25f * i;
            var sx = (int)MathF.Floor((x + MathF.Cos(angle) * radius) / GameConstants.Cell);
            var sy = (int)MathF.Floor((y + MathF.Sin(angle) * radius) / GameConstants.Cell);
            if (sy < 0 || sy >= tiles.GetLength(0) || sx < 0 || sx >= tiles.GetLength(1))
                continue;
            var t = tiles[sy, sx];
            if (!IsSolidTile(t)) continue;
            tx = sx;
            ty = sy;
            tile = t;
            return true;
        }
        return false;
    }
}
