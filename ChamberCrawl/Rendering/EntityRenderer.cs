using ChamberCrawl.Data;
using ChamberCrawl.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChamberCrawl.Rendering;

public static class EntityRenderer
{
    public static void Draw(SpriteBatch spriteBatch, RunState state, TextureAtlas atlas)
    {
        foreach (var entity in state.Entities)
        {
            if (!TrySprite(entity, state, atlas, out var texture, out var tint, out var opacity))
                continue;

            var size = SpriteDrawSize(entity, state);
            var dest = new Rectangle(
                (int)MathF.Round(entity.X - size / 2f),
                (int)MathF.Round(entity.Y - size / 2f),
                (int)MathF.Round(size),
                (int)MathF.Round(size));
            spriteBatch.Draw(texture, dest, tint * opacity);
        }
    }

    private static float SpriteDrawSize(Entity entity, RunState state)
    {
        if (entity is EnemyEntity { Enemy: EnemyKind.Ogre } ogre)
            return MathF.Max(ogre.Radius * 3.2f, 40f);

        return entity switch
        {
            ProjectileEntity => MathF.Max(entity.Radius * 2.5f, 12f),
            PlayerEntity => MathF.Max(entity.Radius * 2.8f, 28f),
            EnemyEntity => MathF.Max(entity.Radius * 2.8f, 26f),
            PickupEntity { Pickup: PickupKind.Portal } => 36f,
            PickupEntity => 28f,
            DoorEntity => 30f,
            _ => MathF.Max(entity.Radius * 2f, 16f),
        };
    }

    private static bool TrySprite(
        Entity entity,
        RunState state,
        TextureAtlas atlas,
        out Texture2D texture,
        out Color tint,
        out float opacity)
    {
        texture = atlas.Pixel;
        tint = Color.White;
        opacity = 1f;

        switch (entity)
        {
            case PlayerEntity:
                texture = atlas.Get("player");
                if (state.TimeMs < state.PlayerStats.IframeUntil &&
                    ((int)(state.TimeMs / 80f) & 1) == 0)
                    opacity = 0.35f;
                return true;
            case EnemyEntity enemy:
                texture = atlas.Get(Enemies.Defs[enemy.Enemy].Texture);
                if (enemy.Enemy == EnemyKind.Specter)
                {
                    opacity = state.TimeMs < enemy.IframeUntil ? 0.25f : 0.7f;
                    tint = new Color(220, 200, 255);
                }
                return true;
            case ProjectileEntity proj:
                if (proj.FromPlayer)
                    texture = atlas.Get("bolt");
                else
                {
                    texture = atlas.Get("projectile");
                    tint = new Color(180, 120, 255);
                }
                return true;
            case PickupEntity pickup:
                texture = pickup.Pickup switch
                {
                    PickupKind.Coin => atlas.Get("coin"),
                    PickupKind.Portal => atlas.Get("portal"),
                    _ => ItemTexture(atlas, pickup.ItemId),
                };
                if (pickup.Pickup == PickupKind.Portal &&
                    pickup.ActiveAt is { } at && state.TimeMs < at)
                    opacity = 0.45f;
                return true;
            case DoorEntity door:
                if (!door.Locked)
                    return false;
                texture = atlas.Get(FloorThemes.DoorTexture(state.Floor.Depth));
                return true;
            default:
                return false;
        }
    }

    private static Texture2D ItemTexture(TextureAtlas atlas, string? itemId)
    {
        if (itemId is not null &&
            Items.Defs.TryGetValue(itemId, out var def) &&
            atlas.TryGet(def.Texture, out var tex))
            return tex;
        return atlas.Get("coin");
    }
}
