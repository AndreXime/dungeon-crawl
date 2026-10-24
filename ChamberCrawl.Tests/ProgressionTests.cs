using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class ProgressionTests
{
    private static RunState InBossRoom(int seed = 42)
    {
        var state = RunFactory.Create(seed);
        RunFactory.EnterRoom(state, state.Floor.BossRoomId, Dir.N);
        state.Entities = state.Entities.Where(e => e is not EnemyEntity).ToList();
        return state;
    }

    [Fact]
    public void Update_SpawnsPortalOnDepth1BossClear()
    {
        var state = InBossRoom();
        Assert.Equal(1, state.Floor.Depth);
        Progression.Update(state);
        Assert.Equal(RunStatus.Playing, state.Status);
        var portal = Assert.Single(state.Entities.OfType<PickupEntity>(), p => p.Pickup == PickupKind.Portal);
        Assert.Equal("portal", portal.Id);
        Assert.True(portal.ActiveAt > state.TimeMs);
    }

    [Fact]
    public void Update_DoesNotDuplicatePortal()
    {
        var state = InBossRoom();
        Progression.Update(state);
        Progression.Update(state);
        Assert.Equal(1, state.Entities.OfType<PickupEntity>().Count(p => p.Pickup == PickupKind.Portal));
    }

    [Fact]
    public void Update_WinsOnFinalDepthBossClear()
    {
        var state = InBossRoom();
        state.Floor.Depth = GameConstants.MaxDepth;
        Progression.Update(state);
        Assert.Equal(RunStatus.Won, state.Status);
        Assert.DoesNotContain(state.Entities, e => e is PickupEntity { Pickup: PickupKind.Portal });
    }

    [Fact]
    public void Update_DoesNotWinWhileBossAlive()
    {
        var state = RunFactory.Create(42);
        RunFactory.EnterRoom(state, state.Floor.BossRoomId, null);
        Progression.Update(state);
        Assert.Equal(RunStatus.Playing, state.Status);
        Assert.DoesNotContain(state.Entities, e => e is PickupEntity { Pickup: PickupKind.Portal });
    }

    [Fact]
    public void AdvanceFloor_PreservesRunProgressAndIncrementsDepth()
    {
        var state = RunFactory.Create(7);
        state.Coins = 12;
        state.Inventory.Add("damage_up");
        state.PlayerStats.Damage = 3;
        state.PlayerStats.Hp = 4;
        state.PlayerStats.MaxHp = 8;

        RunFactory.AdvanceFloor(state);

        Assert.Equal(2, state.Floor.Depth);
        Assert.Equal(7, state.Seed);
        Assert.Equal(12, state.Coins);
        Assert.Equal(new[] { "damage_up" }, state.Inventory);
        Assert.Equal(3, state.PlayerStats.Damage);
        Assert.Equal(4, state.PlayerStats.Hp);
        Assert.Equal(8, state.PlayerStats.MaxHp);
        Assert.Equal(state.Floor.StartRoomId, state.CurrentRoomId);
        Assert.Equal(RunStatus.Playing, state.Status);
        Assert.Contains(state.Entities, e => e is PlayerEntity);
    }

    [Fact]
    public void AdvanceFloor_OnMaxDepthSetsWon()
    {
        var state = RunFactory.Create(3);
        state.Floor.Depth = GameConstants.MaxDepth;
        RunFactory.AdvanceFloor(state);
        Assert.Equal(RunStatus.Won, state.Status);
        Assert.Equal(GameConstants.MaxDepth, state.Floor.Depth);
    }

    [Fact]
    public void Pickups_PortalAdvancesFloorWhenActive()
    {
        var state = InBossRoom();
        Progression.Update(state);
        var portal = state.Entities.OfType<PickupEntity>().Single(p => p.Pickup == PickupKind.Portal);
        state.TimeMs = portal.ActiveAt!.Value;
        var player = state.Entities.OfType<PlayerEntity>().First();
        player.X = portal.X;
        player.Y = portal.Y;

        Pickups.Update(state);

        Assert.Equal(2, state.Floor.Depth);
        Assert.Equal(RunStatus.Playing, state.Status);
    }
}
