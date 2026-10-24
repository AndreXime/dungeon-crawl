namespace ChamberCrawl.Models;

public static class GameConstants
{
    public const int RoomW = 15;
    public const int RoomH = 11;
    public const int Cell = 32;
    public const float PlayerRadius = 10f;
    public const float ProjectileRadius = 4f;
    public const float IframeMs = 600f;
    public const int MaxDepth = 5;

    public const int TileFloor = 0;
    public const int TileWall = 1;
    public const int TileDoor = 2;
    public const int TileObstacle = 3;
    public const int TileCrate = 4;
    public const int TileBarrel = 5;
    public const int TileSpike = 6;
    public const int TilePuddle = 7;
}
