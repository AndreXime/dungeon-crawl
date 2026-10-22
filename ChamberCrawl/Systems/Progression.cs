using ChamberCrawl.Models;
using ChamberCrawl.World;

namespace ChamberCrawl.Systems;

public static class Progression
{
    public static void Update(RunState state)
    {
        if (state.Status != RunStatus.Playing) return;
        if (!state.Floor.Rooms.TryGetValue(state.CurrentRoomId, out var node)) return;
        if (node.Type != RoomType.Boss || !Doors.IsRoomCleared(state.Entities)) return;
        state.Status = RunStatus.Won;
    }
}
