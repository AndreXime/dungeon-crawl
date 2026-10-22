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
        var floor = atlas.Get("floor");
        for (var ty = 0; ty < GameConstants.RoomH; ty++)
        {
            for (var tx = 0; tx < GameConstants.RoomW; tx++)
            {
                var tile = tiles[ty, tx];
                var dest = new Rectangle(
                    tx * GameConstants.Cell,
                    ty * GameConstants.Cell,
                    GameConstants.Cell,
                    GameConstants.Cell);

                if (FloorThemes.TryOverlayTexture(tile, out var overlay))
                {
                    spriteBatch.Draw(floor, dest, Color.White);
                    spriteBatch.Draw(atlas.Get(overlay), dest, Color.White);
                    continue;
                }

                spriteBatch.Draw(atlas.Get(FloorThemes.TextureForTile(tile)), dest, Color.White);
            }
        }
    }
}
