namespace ChamberCrawl.Models;

public static class GameConstants
{
    public const int RoomW = 15;
    public const int RoomH = 11;
    public const int Cell = 32;
    public const float PlayerRadius = 10f;
    public const float ProjectileRadius = 4f;
    public const float IframeMs = 600f;

    public const int TileFloor = 0;
    public const int TileWall = 1;
    public const int TileDoor = 2;
    public const int TileObstacle = 3;
    public const int TileFolder = 4;
    public const int TilePopup = 5;
    public const int TileBadSector = 6;
}
