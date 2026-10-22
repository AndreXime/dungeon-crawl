using ChamberCrawl.Models;
using ChamberCrawl.World;

namespace ChamberCrawl.Tests;

public class FloorGenTests
{
    [Fact]
    public void Generate_CreatesConnectedRoomsWithRequiredTypes()
    {
        var floor = FloorGen.Generate(42, 1);
        Assert.InRange(floor.Rooms.Count, 6, 8);
        Assert.Equal(RoomType.Start, floor.Rooms[floor.StartRoomId].Type);
        Assert.True(floor.Rooms[floor.StartRoomId].Cleared);
        Assert.Equal(RoomType.Boss, floor.Rooms[floor.BossRoomId].Type);
        Assert.Contains(floor.Rooms.Values, r => r.Type == RoomType.Shop);
        Assert.Contains(floor.Rooms.Values, r => r.Type == RoomType.Item);

        var seen = new HashSet<string>();
        var stack = new Stack<string>();
        stack.Push(floor.StartRoomId);
        while (stack.Count > 0)
        {
            var id = stack.Pop();
            if (!seen.Add(id)) continue;
            foreach (var n in floor.Rooms[id].Neighbors.Values)
                stack.Push(n);
        }
        Assert.Equal(floor.Rooms.Count, seen.Count);
    }

    [Fact]
    public void Generate_IsDeterministic()
    {
        var a = FloorGen.Generate(7, 2);
        var b = FloorGen.Generate(7, 2);
        Assert.Equal(a.Rooms.Keys.OrderBy(x => x), b.Rooms.Keys.OrderBy(x => x));
        Assert.Equal(a.BossRoomId, b.BossRoomId);
    }

    [Fact]
    public void Generate_GivesEachNodeItsOwnClaimedSpawns()
    {
        var floor = FloorGen.Generate(42, 1);
        var lists = floor.Rooms.Values.Select(r => r.ClaimedSpawns).ToList();
        Assert.All(lists, l => Assert.Empty(l));
        Assert.Equal(lists.Count, lists.Distinct().Count());
    }
}
