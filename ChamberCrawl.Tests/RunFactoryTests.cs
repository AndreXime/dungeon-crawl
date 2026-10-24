using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;
using Microsoft.Xna.Framework.Input;

namespace ChamberCrawl.Tests;

public class RunFactoryTests
{
    [Fact]
    public void Create_StartsWithInitialStatsAndPlayerInStartRoom()
    {
        var run = RunFactory.Create(42);

        Assert.Equal(run.Floor.StartRoomId, run.CurrentRoomId);
        Assert.True(run.Floor.Rooms[run.CurrentRoomId].Visited);
        Assert.Equal(6, run.PlayerStats.Hp);
        Assert.Equal(110, run.PlayerStats.Speed);
        Assert.Equal(200, run.PlayerStats.Range);
        Assert.Single(run.Entities.OfType<PlayerEntity>());
        Assert.Equal(RunStatus.Playing, run.Status);
    }

    [Fact]
    public void EnterRoom_SpawnsPlayerNextToOppositeDoor()
    {
        var run = RunFactory.Create(42);
        var (dir, neighborId) = run.Floor.Rooms[run.CurrentRoomId].Neighbors.First();

        RunFactory.EnterRoom(run, neighborId, dir);

        var player = run.Entities.OfType<PlayerEntity>().Single();
        var cell = GameConstants.Cell;
        var (tx, ty) = dir switch
        {
            Dir.S => (GameConstants.RoomW / 2, 1),
            Dir.N => (GameConstants.RoomW / 2, GameConstants.RoomH - 2),
            Dir.E => (1, GameConstants.RoomH / 2),
            _ => (GameConstants.RoomW - 2, GameConstants.RoomH / 2),
        };
        Assert.Equal((tx + 0.5f) * cell, player.X);
        Assert.Equal((ty + 0.5f) * cell, player.Y);
        Assert.Equal(neighborId, run.CurrentRoomId);
    }

    [Fact]
    public void EnterRoom_BossHasPhaseZero_AndClearedRoomSkipsEnemies()
    {
        var run = RunFactory.Create(7);
        RunFactory.EnterRoom(run, run.Floor.BossRoomId, null);
        var boss = run.Entities.OfType<EnemyEntity>().Single(e => e.Enemy == EnemyKind.Ogre);
        Assert.Equal(0, boss.Phase);
        Assert.Equal(Ai.OgrePhase1MaxMs, boss.Cooldown);

        run.Floor.Rooms[run.Floor.BossRoomId].Cleared = true;
        RunFactory.EnterRoom(run, run.Floor.BossRoomId, null);
        Assert.Empty(run.Entities.OfType<EnemyEntity>());
        Assert.All(run.Entities.OfType<DoorEntity>(), d => Assert.False(d.Locked));
    }

    [Fact]
    public void EnterRoom_SkipsClaimedItemSpawns()
    {
        var run = RunFactory.Create(3);
        var node = run.Floor.Rooms.Values.First(n => n.Type is RoomType.Item or RoomType.Shop);
        RunFactory.EnterRoom(run, node.Id, null);
        var items = run.Entities.OfType<PickupEntity>().Where(p => p.Pickup == PickupKind.Item).ToList();
        Assert.NotEmpty(items);

        node.ClaimedSpawns.Add(items[0].SpawnIndex!.Value);
        RunFactory.EnterRoom(run, node.Id, null);

        var remaining = run.Entities.OfType<PickupEntity>().Where(p => p.Pickup == PickupKind.Item);
        Assert.DoesNotContain(remaining, p => p.SpawnIndex == items[0].SpawnIndex);
    }

    [Fact]
    public void Movement_MovesPlayerAndSlidesAlongWalls()
    {
        var run = RunFactory.Create(42);
        var player = run.Entities.OfType<PlayerEntity>().Single();
        var startX = player.X;

        Movement.Update(run, new InputSnapshot(1, 0, 0, 0, false), 0.1f);
        Assert.Equal(startX + 11f, player.X, 3);

        player.X = GameConstants.Cell * 1.5f;
        player.Y = GameConstants.Cell * 1.5f;
        Movement.Update(run, new InputSnapshot(-0.7f, 0.7f, 0, 0, false), 1f);
        Assert.Equal(GameConstants.Cell * 1.5f, player.X);
        Assert.Equal(0, player.Vx);
    }

    [Fact]
    public void InputSystem_NormalizesDiagonalAndReadsRestart()
    {
        var snapshot = InputSystem.FromKeyboardState(new KeyboardState(Keys.W, Keys.D, Keys.Left, Keys.R));

        Assert.Equal(0.7071f, snapshot.MoveX, 3);
        Assert.Equal(-0.7071f, snapshot.MoveY, 3);
        Assert.Equal(-1f, snapshot.ShootX);
        Assert.Equal(0f, snapshot.ShootY);
        Assert.True(snapshot.Restart);
    }
}
