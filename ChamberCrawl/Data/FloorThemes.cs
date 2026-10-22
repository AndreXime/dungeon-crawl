using ChamberCrawl.Models;

namespace ChamberCrawl.Data;

public static class FloorThemes
{
    public static string TextureForTile(int tile) => tile switch
    {
        GameConstants.TileWall => "wall",
        GameConstants.TileDoor => "door-tile",
        GameConstants.TileFolder => "folder",
        GameConstants.TilePopup => "popup",
        GameConstants.TileBadSector => "bad-sector",
        GameConstants.TileObstacle => "obstacle",
        _ => "floor",
    };
}
