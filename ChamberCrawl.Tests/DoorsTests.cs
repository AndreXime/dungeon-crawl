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
    public void Update_KeepsLooseCoinsOnClear()
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
            X = 80,
            Y = 80,
            Radius = 8,
        });
        var coinsBefore = state.Coins;

        Doors.Update(state);

        Assert.Equal(coinsBefore, state.Coins);
        Assert.Contains(state.Entities, e => e.Id == "coin-x");
        Assert.True(state.Floor.Rooms[target.Id].Cleared);
    }

    [Fact]
    public void EnterRoom_PreservesLooseCoinsWhenReturning()
    {
        var state = RunFactory.Create(42);
        var startId = state.CurrentRoomId;
        var neighbor = state.Floor.Rooms[startId].Neighbors.First();
        state.Entities.Add(new PickupEntity
        {
            Id = "coin-stay",
            Pickup = PickupKind.Coin,
            Amount = 3,
            X = 96,
            Y = 96,
            Radius = 8,
        });

        RunFactory.EnterRoom(state, neighbor.Value, neighbor.Key);
        RunFactory.EnterRoom(state, startId, Opp(neighbor.Key));

        var coin = Assert.Single(state.Entities.OfType<PickupEntity>(), p => p.Pickup == PickupKind.Coin);
        Assert.Equal(3, coin.Amount);
        Assert.Equal(96, coin.X);
        Assert.Equal(96, coin.Y);
    }

    private static Dir Opp(Dir dir) => dir switch
    {
        Dir.N => Dir.S,
        Dir.S => Dir.N,
        Dir.E => Dir.W,
        _ => Dir.E,
    };

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
