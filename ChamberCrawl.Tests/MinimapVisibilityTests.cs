using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class MinimapVisibilityTests
{
    [Fact]
    public void Collect_IncludesAdjacentUnvisitedAsUnknown()
    {
        var start = new FloorRoomNode
        {
            Id = "0,0",
            Visited = true,
            Neighbors = { [Dir.E] = "1,0" },
        };
        var east = new FloorRoomNode
        {
            Id = "1,0",
            Visited = false,
            Neighbors = { [Dir.W] = "0,0", [Dir.E] = "2,0" },
        };
        var far = new FloorRoomNode
        {
            Id = "2,0",
            Visited = false,
            Neighbors = { [Dir.W] = "1,0" },
        };
        var state = new RunState
        {
            CurrentRoomId = "0,0",
            Floor = new Floor
            {
                StartRoomId = "0,0",
                Rooms =
                {
                    ["0,0"] = start,
                    ["1,0"] = east,
                    ["2,0"] = far,
                },
            },
        };

        var cells = MinimapVisibility.Collect(state);

        Assert.Equal(2, cells.Count);
        Assert.Contains(cells, c => c.Room.Id == "0,0" && !c.Unknown);
        Assert.Contains(cells, c => c.Room.Id == "1,0" && c.Unknown);
        Assert.DoesNotContain(cells, c => c.Room.Id == "2,0");
    }
}
