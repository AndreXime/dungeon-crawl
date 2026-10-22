namespace ChamberCrawl.Models;

public static class GameConstants
{
    public const int RoomW = 15;
    public const int RoomH = 11;
    public const int Cell = 32;
    public const float PlayerRadius = 10f;
    public const float ProjectileRadius = 4f;
    public const float IframeMs = 600f;
    public const float PortalDelayMs = 500f;

    public const int TileFloor = 0;
    public const int TileWall = 1;
    public const int TileDoor = 2;
    public const int TileObstacle = 3;
}
