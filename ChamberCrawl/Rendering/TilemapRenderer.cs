using System;
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
        var depth = state.Floor.Depth;
        var floor = atlas.Get(FloorThemes.TextureForTile(GameConstants.TileFloor, depth));
        var wall = atlas.Get(FloorThemes.TextureForTile(GameConstants.TileWall, depth));
        var doorOpen = atlas.Get(FloorThemes.DoorOpenTexture);

        // door-open só quando destrancada; trancada fica wall + sprite door por cima.
        Span<bool> lockedDoorAt = stackalloc bool[GameConstants.RoomW * GameConstants.RoomH];
        foreach (var entity in state.Entities)
        {
            if (entity is not DoorEntity { Locked: true } door) continue;
            var dtx = (int)MathF.Floor(door.X / GameConstants.Cell);
            var dty = (int)MathF.Floor(door.Y / GameConstants.Cell);
            if (dtx < 0 || dty < 0 || dtx >= GameConstants.RoomW || dty >= GameConstants.RoomH)
                continue;
            lockedDoorAt[dty * GameConstants.RoomW + dtx] = true;
        }

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

                if (tile == GameConstants.TileDoor)
                {
                    spriteBatch.Draw(wall, dest, Color.White);
                    if (!lockedDoorAt[ty * GameConstants.RoomW + tx])
                        spriteBatch.Draw(doorOpen, dest, Color.White);
                    continue;
                }

                if (FloorThemes.TryOverlayTexture(tile, depth, out var overlay))
                {
                    spriteBatch.Draw(floor, dest, Color.White);
                    spriteBatch.Draw(atlas.Get(overlay), dest, Color.White);
                    continue;
                }

                spriteBatch.Draw(atlas.Get(FloorThemes.TextureForTile(tile, depth)), dest, Color.White);
            }
        }
    }
}
