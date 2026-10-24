using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public static class Progression
{
    public const float PortalActivateDelayMs = 800f;
    public const float PortalRadius = 14f;

    public static void Update(RunState state)
    {
        if (state.Status != RunStatus.Playing) return;
        if (!state.Floor.Rooms.TryGetValue(state.CurrentRoomId, out var node)) return;
        if (node.Type != RoomType.Boss || !Doors.IsRoomCleared(state.Entities)) return;

        if (state.Floor.Depth >= GameConstants.MaxDepth)
        {
            state.Status = RunStatus.Won;
            return;
        }

        if (state.Entities.Any(e => e is PickupEntity { Pickup: PickupKind.Portal }))
            return;

        var cx = (GameConstants.RoomW / 2 + 0.5f) * GameConstants.Cell;
        var cy = (GameConstants.RoomH / 2 + 0.5f) * GameConstants.Cell;
        state.Entities.Add(new PickupEntity
        {
            Id = "portal",
            Pickup = PickupKind.Portal,
            X = cx,
            Y = cy,
            Radius = PortalRadius,
            ActiveAt = state.TimeMs + PortalActivateDelayMs,
        });
    }
}
