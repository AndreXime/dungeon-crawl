using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Doors
{
    public static bool IsRoomCleared(IEnumerable<Entity> entities) =>
        !entities.Any(e => e is EnemyEntity { Hp: > 0 });

    public static void Update(RunState state)
    {
        if (state.Status != RunStatus.Playing) return;
        if (!state.Floor.Rooms.TryGetValue(state.CurrentRoomId, out var node)) return;

        if (IsRoomCleared(state.Entities))
        {
            node.Cleared = true;
            CollectLooseCoins(state);
            foreach (var entity in state.Entities)
            {
                if (entity is DoorEntity door) door.Locked = false;
            }
        }

        var player = state.Entities.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return;

        foreach (var entity in state.Entities)
        {
            if (entity is not DoorEntity { Locked: false } door) continue;
            if (!node.Neighbors.TryGetValue(door.Dir, out var neighborId)) continue;
            if (!Collision.CirclesOverlap(player.X, player.Y, player.Radius, door.X, door.Y, door.Radius))
                continue;

            RunFactory.EnterRoom(state, neighborId, door.Dir);
            return;
        }
    }

    private static void CollectLooseCoins(RunState state)
    {
        var remaining = new List<Entity>();
        foreach (var entity in state.Entities)
        {
            if (entity is PickupEntity { Pickup: PickupKind.Coin } coin)
                state.Coins += coin.Amount;
            else
                remaining.Add(entity);
        }
        state.Entities = remaining;
    }
}
