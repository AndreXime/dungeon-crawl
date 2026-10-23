using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Audio;
using ChamberCrawl.Data;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Pickups
{
    public static void Update(RunState state)
    {
        if (state.Status != RunStatus.Playing) return;

        var player = state.Entities.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return;

        state.Floor.Rooms.TryGetValue(state.CurrentRoomId, out var node);
        var removed = new HashSet<Entity>();

        foreach (var entity in state.Entities)
        {
            if (entity is not PickupEntity pickup || pickup.Pickup == PickupKind.Portal) continue;
            if (!Collision.CirclesOverlap(player.X, player.Y, player.Radius, pickup.X, pickup.Y, pickup.Radius))
                continue;

            if (pickup.Pickup == PickupKind.Coin)
            {
                state.Coins += pickup.Amount;
                Sfx.Play("pickupCoin");
                removed.Add(pickup);
                continue;
            }

            if (pickup.Pickup == PickupKind.Item && pickup.ItemId is { } itemId)
            {
                var price = pickup.Price ?? 0;
                if (price > 0 && state.Coins < price) continue;
                if (price > 0) state.Coins -= price;

                state.PlayerStats = Items.Apply(state.PlayerStats, itemId);
                player.Hp = state.PlayerStats.Hp;
                state.Inventory.Add(itemId);
                Sfx.Play("powerUp");

                if (node is not null && pickup.SpawnIndex is { } spawnIndex)
                    node.ClaimedSpawns.Add(spawnIndex);

                removed.Add(pickup);
            }
        }

        if (removed.Count > 0)
            state.Entities = state.Entities.Where(e => !removed.Contains(e)).ToList();
    }
}
