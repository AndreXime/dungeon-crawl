using System;
using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class ProgressionTests
{
    private static void MoveOnto(RunState state, float x, float y)
    {
        var player = state.Entities.OfType<PlayerEntity>().Single();
        player.X = x;
        player.Y = y;
    }

    private static RunState InBossRoom(int depth)
    {
        var state = RunFactory.Create(42);
        if (depth == 2)
            state.Floor.Depth = 2;
        RunFactory.EnterRoom(state, state.Floor.BossRoomId, Dir.N);
        state.Entities = state.Entities.Where(e => e is not EnemyEntity).ToList();
        return state;
    }

    [Fact]
    public void Update_AdvancesToFloor2WhenTouchingPortalAfterDelay()
    {
        var state = InBossRoom(1);
        Progression.Update(state);
        var portal = state.Entities.FirstOrDefault(e => e.Id == "portal");
        Assert.NotNull(portal);
        MoveOnto(state, portal!.X, portal.Y);

        Progression.Update(state);
        Assert.Equal(1, state.Floor.Depth);

        state.TimeMs += 600;
        Progression.Update(state);

        Assert.Equal(2, state.Floor.Depth);
        Assert.Equal(state.Floor.StartRoomId, state.CurrentRoomId);
        Assert.Equal(RunStatus.Playing, state.Status);
    }

    [Fact]
    public void Update_DoesNotAdvanceWhenPlayerStaysAtCenter()
    {
        var state = InBossRoom(1);
        var centerX = (MathF.Floor(GameConstants.RoomW / 2f) + 0.5f) * GameConstants.Cell;
        var centerY = (MathF.Floor(GameConstants.RoomH / 2f) + 0.5f) * GameConstants.Cell;
        MoveOnto(state, centerX, centerY);

        Progression.Update(state);
        state.TimeMs += 600;
        Progression.Update(state);

        Assert.Equal(1, state.Floor.Depth);
    }

    [Fact]
    public void Update_WinsOnDepth2BossClear()
    {
        var state = InBossRoom(2);
        Progression.Update(state);
        Assert.Equal(RunStatus.Won, state.Status);
    }

    [Fact]
    public void Update_DoesNotSpawnPortalWhileBossAlive()
    {
        var state = RunFactory.Create(42);
        RunFactory.EnterRoom(state, state.Floor.BossRoomId, null);

        Progression.Update(state);

        Assert.DoesNotContain(state.Entities, e => e.Id == "portal");
        Assert.Equal(RunStatus.Playing, state.Status);
    }

    [Fact]
    public void Portal_SpawnsOneTileSouthOfCenterWithActiveAtDelay()
    {
        var state = InBossRoom(1);
        state.TimeMs = 1000;

        Progression.Update(state);

        var portal = Assert.IsType<PickupEntity>(state.Entities.Single(e => e.Id == "portal"));
        var expectedX = (MathF.Floor(GameConstants.RoomW / 2f) + 0.5f) * GameConstants.Cell;
        var expectedY = (MathF.Floor(GameConstants.RoomH / 2f) + 1.5f) * GameConstants.Cell;
        Assert.Equal(expectedX, portal.X);
        Assert.Equal(expectedY, portal.Y);
        Assert.Equal(PickupKind.Portal, portal.Pickup);
        Assert.Equal(1000 + GameConstants.PortalDelayMs, portal.ActiveAt);
    }
}
