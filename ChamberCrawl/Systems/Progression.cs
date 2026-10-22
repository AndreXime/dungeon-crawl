using System;
using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.World;

namespace ChamberCrawl.Systems;

public static class Progression
{
    private const string PortalId = "portal";
    private const float PortalRadius = 14f;

    public static void AdvanceFloor(RunState state)
    {
        state.Floor = FloorGen.Generate(state.Seed, 2);
        RunFactory.EnterRoom(state, state.Floor.StartRoomId, null);
    }

    public static void Update(RunState state)
    {
        if (state.Status != RunStatus.Playing) return;
        if (!state.Floor.Rooms.TryGetValue(state.CurrentRoomId, out var node)) return;
        if (node.Type != RoomType.Boss || !Doors.IsRoomCleared(state.Entities)) return;

        if (state.Floor.Depth == 2)
        {
            state.Status = RunStatus.Won;
            return;
        }

        var portal = state.Entities.OfType<PickupEntity>()
            .FirstOrDefault(e => e.Id == PortalId);
        if (portal is null)
        {
            portal = new PickupEntity
            {
                Id = PortalId,
                Pickup = PickupKind.Portal,
                X = (MathF.Floor(GameConstants.RoomW / 2f) + 0.5f) * GameConstants.Cell,
                Y = (MathF.Floor(GameConstants.RoomH / 2f) + 1.5f) * GameConstants.Cell,
                Radius = PortalRadius,
                ActiveAt = state.TimeMs + GameConstants.PortalDelayMs,
            };
            state.Entities.Add(portal);
        }

        var player = state.Entities.OfType<PlayerEntity>().FirstOrDefault();
        if (player is null) return;
        if (state.TimeMs < (portal.ActiveAt ?? 0f)) return;
        if (!Collision.CirclesOverlap(player.X, player.Y, player.Radius, portal.X, portal.Y, portal.Radius))
            return;

        AdvanceFloor(state);
    }
}
