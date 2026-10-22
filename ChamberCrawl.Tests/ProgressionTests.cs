using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class ProgressionTests
{
    private static RunState InBossRoom()
    {
        var state = RunFactory.Create(42);
        RunFactory.EnterRoom(state, state.Floor.BossRoomId, Dir.N);
        state.Entities = state.Entities.Where(e => e is not EnemyEntity).ToList();
        return state;
    }

    [Fact]
    public void Update_WinsOnDepth1BossClear()
    {
        var state = InBossRoom();
        Progression.Update(state);
        Assert.Equal(RunStatus.Won, state.Status);
        Assert.DoesNotContain(state.Entities, e => e.Id == "portal");
    }

    [Fact]
    public void Update_DoesNotWinWhileBossAlive()
    {
        var state = RunFactory.Create(42);
        RunFactory.EnterRoom(state, state.Floor.BossRoomId, null);
        Progression.Update(state);
        Assert.Equal(RunStatus.Playing, state.Status);
    }
}
