using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class PickupsTests
{
    private static PlayerEntity Player(RunState state) =>
        state.Entities.OfType<PlayerEntity>().Single();

    private static string ItemRoomId(RunState state) =>
        state.Floor.Rooms.Values.First(r => r.Type == RoomType.Item).Id;

    private static void MoveOnto(PlayerEntity player, Entity target)
    {
        player.X = target.X;
        player.Y = target.Y;
    }

    [Fact]
    public void Update_ClaimedItemDoesNotRespawnOnReenter()
    {
        var state = RunFactory.Create(42);
        var roomId = ItemRoomId(state);
        RunFactory.EnterRoom(state, roomId, null);

        var item = state.Entities.OfType<PickupEntity>()
            .First(p => p.Pickup == PickupKind.Item);
        MoveOnto(Player(state), item);
        Pickups.Update(state);

        Assert.Single(state.Inventory);

        RunFactory.EnterRoom(state, state.Floor.StartRoomId, null);
        RunFactory.EnterRoom(state, roomId, null);

        Assert.DoesNotContain(
            state.Entities.OfType<PickupEntity>(),
            p => p.Pickup == PickupKind.Item);
    }

    [Fact]
    public void Update_CollectsCoinsAndAddsAmount()
    {
        var state = RunFactory.Create(42);
        var player = Player(state);
        state.Entities.Add(new PickupEntity
        {
            Id = "coin-1",
            Pickup = PickupKind.Coin,
            Amount = 3,
            X = player.X,
            Y = player.Y,
            Radius = 8,
        });

        Pickups.Update(state);

        Assert.Equal(3, state.Coins);
        Assert.DoesNotContain(state.Entities, e => e.Id == "coin-1");
    }

    [Fact]
    public void Update_CollectsFreeItemAppliesStatsAndInventory()
    {
        var state = RunFactory.Create(42);
        var player = Player(state);
        var baseDamage = state.PlayerStats.Damage;
        state.Entities.Add(new PickupEntity
        {
            Id = "item-1",
            Pickup = PickupKind.Item,
            ItemId = "damage_up",
            X = player.X,
            Y = player.Y,
            Radius = 10,
        });

        Pickups.Update(state);

        Assert.Equal(baseDamage + 1, state.PlayerStats.Damage);
        Assert.Equal(new[] { "damage_up" }, state.Inventory);
        Assert.DoesNotContain(state.Entities, e => e.Id == "item-1");
    }

    [Fact]
    public void Update_BuysShopItemOnlyWithEnoughCoins()
    {
        var state = RunFactory.Create(42);
        var player = Player(state);
        state.Entities.Add(new PickupEntity
        {
            Id = "shop-1",
            Pickup = PickupKind.Item,
            ItemId = "hp_up",
            Price = 6,
            X = player.X,
            Y = player.Y,
            Radius = 10,
        });
        state.Coins = 5;

        Pickups.Update(state);

        Assert.Equal(5, state.Coins);
        Assert.Empty(state.Inventory);
        Assert.Contains(state.Entities, e => e.Id == "shop-1");

        state.Coins = 8;
        Pickups.Update(state);

        Assert.Equal(2, state.Coins);
        Assert.Equal(new[] { "hp_up" }, state.Inventory);
        Assert.Equal(8, state.PlayerStats.MaxHp);
        Assert.Equal(8, player.Hp);
        Assert.DoesNotContain(state.Entities, e => e.Id == "shop-1");
    }

    [Fact]
    public void Update_IgnoresInactivePortal()
    {
        var state = RunFactory.Create(42);
        var player = Player(state);
        state.Entities.Add(new PickupEntity
        {
            Id = "portal",
            Pickup = PickupKind.Portal,
            X = player.X,
            Y = player.Y,
            Radius = 14,
            ActiveAt = state.TimeMs + 1000,
        });

        Pickups.Update(state);

        Assert.Equal(1, state.Floor.Depth);
        Assert.Contains(state.Entities, e => e.Id == "portal");
    }

    [Fact]
    public void Update_ActivePortalAdvancesFloor()
    {
        var state = RunFactory.Create(42);
        var player = Player(state);
        state.Entities.Add(new PickupEntity
        {
            Id = "portal",
            Pickup = PickupKind.Portal,
            X = player.X,
            Y = player.Y,
            Radius = 14,
            ActiveAt = state.TimeMs,
        });

        Pickups.Update(state);

        Assert.Equal(2, state.Floor.Depth);
        Assert.Equal(RunStatus.Playing, state.Status);
    }
}
