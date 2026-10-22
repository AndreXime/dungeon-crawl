using ChamberCrawl.Models;
using Microsoft.Xna.Framework.Graphics;

namespace ChamberCrawl.Rendering;

public static class GameRenderer
{
    public static void Draw(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
    {
        TilemapRenderer.Draw(spriteBatch, state, atlas);
        EntityRenderer.Draw(spriteBatch, state, atlas);
        HudRenderer.Draw(spriteBatch, state, atlas);
    }
}
