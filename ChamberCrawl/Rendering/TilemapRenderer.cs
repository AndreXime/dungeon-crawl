using ChamberCrawl.Data;
using ChamberCrawl.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChamberCrawl.Rendering;

public static class TilemapRenderer
{
    public static void Draw(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
    {
        var tiles = state.Room.Tiles;
        for (var ty = 0; ty < GameConstants.RoomH; ty++)
        {
            for (var tx = 0; tx < GameConstants.RoomW; tx++)
            {
                var name = FloorThemes.TextureForTile(tiles[ty, tx]);
                var dest = new Rectangle(
                    tx * GameConstants.Cell,
                    ty * GameConstants.Cell,
                    GameConstants.Cell,
                    GameConstants.Cell);
                spriteBatch.Draw(atlas.Get(name), dest, Color.White);
            }
        }
    }
}
