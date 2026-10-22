using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.Systems;

public readonly record struct MinimapCell(FloorRoomNode Room, int X, int Y, bool Unknown);

public static class MinimapVisibility
{
    public static List<MinimapCell> Collect(RunState state)
    {
        var rooms = state.Floor.Rooms;
        var knownIds = new HashSet<string>();
        foreach (var room in rooms.Values)
        {
            if (room.Visited || room.Id == state.CurrentRoomId)
                knownIds.Add(room.Id);
        }

        var cells = new Dictionary<string, MinimapCell>();
        foreach (var id in knownIds)
        {
            if (!rooms.TryGetValue(id, out var room)) continue;
            if (!TryParse(room.Id, out var x, out var y)) continue;
            cells[id] = new MinimapCell(room, x, y, Unknown: false);

            foreach (var neighborId in room.Neighbors.Values)
            {
                if (knownIds.Contains(neighborId)) continue;
                if (!rooms.TryGetValue(neighborId, out var neighbor)) continue;
                if (!TryParse(neighbor.Id, out var nx, out var ny)) continue;
                cells.TryAdd(neighborId, new MinimapCell(neighbor, nx, ny, Unknown: true));
            }
        }

        return cells.Values.ToList();
    }

    private static bool TryParse(string id, out int x, out int y)
    {
        x = 0;
        y = 0;
        var comma = id.IndexOf(',');
        if (comma <= 0) return false;
        return int.TryParse(id.AsSpan(0, comma), out x)
            && int.TryParse(id.AsSpan(comma + 1), out y);
    }
}
