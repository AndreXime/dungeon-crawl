using ChamberCrawl.Models;

namespace ChamberCrawl.Data;

public static class FloorThemes
{
    public static string MusicForDepth(int depth) => depth switch
    {
        _ => "FirstFloorTheme",
    };

    public static string MusicFor(RunState state)
    {
        if (!state.Floor.Rooms.TryGetValue(state.CurrentRoomId, out var node))
            return MusicForDepth(state.Floor.Depth);

        return node.Type switch
        {
            RoomType.Shop => "ShopTheme",
            RoomType.Boss => "BossTheme",
            _ => MusicForDepth(state.Floor.Depth),
        };
    }

    public static string TextureForTile(int tile, int depth = 1) => tile switch
    {
        GameConstants.TileWall => DepthTexture("wall", depth),
        GameConstants.TileDoor => DepthTexture("door-tile", depth),
        GameConstants.TileCrate => "crate",
        GameConstants.TileBarrel => "barrel",
        GameConstants.TileSpike => "spike",
        GameConstants.TileObstacle => "obstacle",
        _ => DepthTexture("floor", depth),
    };

    public static string DoorTexture(int depth) => DepthTexture("door", depth);

    /// <summary>Obstáculos são sprites sobre o chão; parede/porta/chão não usam overlay.</summary>
    public static bool TryOverlayTexture(int tile, out string textureName)
    {
        textureName = tile switch
        {
            GameConstants.TileCrate => "crate",
            GameConstants.TileBarrel => "barrel",
            GameConstants.TileSpike => "spike",
            GameConstants.TileObstacle => "obstacle",
            _ => "",
        };
        return textureName.Length > 0;
    }

    private static string DepthTexture(string baseName, int depth) =>
        depth >= 2 ? $"{baseName}-2" : baseName;
}
