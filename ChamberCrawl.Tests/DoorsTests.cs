using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class DoorsTests
{
    private static (Dir Dir, string Id) FirstNormalRoom(RunState state)
    {
        var start = state.Floor.Rooms[state.Floor.StartRoomId];
        foreach (var (dir, id) in start.Neighbors)
        {
            if (state.Floor.Rooms[id].Type == RoomType.Normal)
                return (dir, id);
        }
        throw new System.InvalidOperationException("sem sala normal vizinha");
    }

    private static void MoveOnto(RunState state, float x, float y)
    {
        var player = state.Entities.OfType<PlayerEntity>().Single();
        player.X = x;
        player.Y = y;
    }

    [Fact]
    public void IsRoomCleared_TrueWithoutLivingEnemies()
    {
        Assert.True(Doors.IsRoomCleared(System.Array.Empty<Entity>()));
        Assert.True(Doors.IsRoomCleared(new[] { new EnemyEntity { Id = "1", Hp = 0 } }));
        Assert.False(Doors.IsRoomCleared(new[] { new EnemyEntity { Id = "1", Hp = 1 } }));
    }

    [Fact]
    public void Update_DoorsStartLocked_AndUnlockWhenCleared()
    {
        var state = RunFactory.Create(42);
        var target = FirstNormalRoom(state);
        RunFactory.EnterRoom(state, target.Id, target.Dir);

        var doors = state.Entities.OfType<DoorEntity>().ToList();
        Assert.NotEmpty(doors);
        Assert.All(doors, d => Assert.True(d.Locked));

        Doors.Update(state);
        Assert.False(state.Floor.Rooms[target.Id].Cleared);

        state.Entities = state.Entities.Where(e => e is not EnemyEntity).ToList();
        Doors.Update(state);

        Assert.True(state.Floor.Rooms[target.Id].Cleared);
        Assert.All(state.Entities.OfType<DoorEntity>(), d => Assert.False(d.Locked));
    }

    [Fact]
    public void Update_CollectsLooseCoinsOnClear()
    {
        var state = RunFactory.Create(42);
        var target = FirstNormalRoom(state);
        RunFactory.EnterRoom(state, target.Id, target.Dir);
        state.Entities = state.Entities.Where(e => e is not EnemyEntity).ToList();
        state.Entities.Add(new PickupEntity
        {
            Id = "coin-x",
            Pickup = PickupKind.Coin,
            Amount = 2,
            Radius = 8,
        });

        Doors.Update(state);

        Assert.Equal(2, state.Coins);
        Assert.DoesNotContain(state.Entities, e => e.Id == "coin-x");
    }

    [Fact]
    public void Update_DoesNotEnterThroughLockedDoor()
    {
        var state = RunFactory.Create(42);
        var target = FirstNormalRoom(state);
        RunFactory.EnterRoom(state, target.Id, target.Dir);
        var door = state.Entities.OfType<DoorEntity>().First();
        MoveOnto(state, door.X, door.Y);

        Doors.Update(state);

        Assert.Equal(target.Id, state.CurrentRoomId);
    }

    [Fact]
    public void Update_EntersNeighborWhenTouchingUnlockedDoor()
    {
        var state = RunFactory.Create(42);
        var start = state.Floor.Rooms[state.CurrentRoomId];
        var door = state.Entities.OfType<DoorEntity>().First();
        MoveOnto(state, door.X, door.Y);

        Doors.Update(state);

        Assert.Equal(start.Neighbors[door.Dir], state.CurrentRoomId);
        Assert.True(state.Floor.Rooms[state.CurrentRoomId].Visited);
    }
}
