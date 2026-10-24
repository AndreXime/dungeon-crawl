using ChamberCrawl.Models;

namespace ChamberCrawl.Data;

public static class FloorThemes
{
    public const float TitleDurationMs = 2800f;

    public static string NameForDepth(int depth) => depth switch
    {
        1 => "MASMORRA",
        2 => "CATACUMBAS",
        3 => "ESGOTOS",
        4 => "ARSENAL",
        _ => "SANTUARIO",
    };

    public static string MusicForDepth(int depth) => depth switch
    {
        1 => "FirstFloorTheme",
        2 => "CatacombsTheme",
        3 => "SewersTheme",
        4 => "ArmoryTheme",
        _ => "SanctumTheme",
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

    public const string DoorOpenTexture = "door-open";

    public static string TextureForTile(int tile, int depth = 1) => tile switch
    {
        GameConstants.TileWall => DepthTexture("wall", depth),
        // Porta aberta = wall do andar + overlay door-open (ver TilemapRenderer).
        GameConstants.TileDoor => DepthTexture("wall", depth),
        GameConstants.TileCrate => "crate",
        GameConstants.TileBarrel => "barrel",
        GameConstants.TileSpike => "spike",
        GameConstants.TileObstacle => "obstacle",
        GameConstants.TilePuddle => "puddle",
        GameConstants.TileRune => "rune",
        GameConstants.TilePedestal => "pedestal",
        _ => DepthTexture("floor", depth),
    };

    /// <summary>Sprite da porta fechada/trancada (entidade), por andar.</summary>
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
            GameConstants.TilePuddle => "puddle",
            GameConstants.TileRune => "rune",
            GameConstants.TilePedestal => "pedestal",
            _ => "",
        };
        return textureName.Length > 0;
    }

    private static string DepthTexture(string baseName, int depth)
    {
        var d = depth < 1 ? 1 : depth > 5 ? 5 : depth;
        return $"{baseName}-{d}";
    }
}
