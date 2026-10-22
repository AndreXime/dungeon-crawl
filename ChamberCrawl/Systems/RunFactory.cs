using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Data;
using ChamberCrawl.Models;
using ChamberCrawl.World;

namespace ChamberCrawl.Systems;

public static class RunFactory
{
    private const float DoorRadius = GameConstants.Cell / 2f;
    private const float PickupRadius = 10f;

    public static RunState Create(int seed)
    {
        var floor = FloorGen.Generate(seed, 1);
        if (!floor.Rooms.ContainsKey(floor.StartRoomId)) throw new InvalidOperationException("Andar sem sala inicial");

        var state = new RunState
        {
            Seed = seed,
            Floor = floor,
            CurrentRoomId = floor.StartRoomId,
            PlayerStats = InitialStats(),
        };
        EnterRoom(state, floor.StartRoomId, null);
        return state;
    }

    public static void EnterRoom(RunState state, string roomId, Dir? entryDir)
    {
        if (!state.Floor.Rooms.TryGetValue(roomId, out var node))
            throw new InvalidOperationException($"Sala inexistente: {roomId}");

        node.Visited = true;
        state.CurrentRoomId = roomId;
        state.Room = RoomGen.Generate(node, node.Neighbors.Keys.ToList());
        state.Entities = SpawnRoomEntities(state.Room, node, entryDir);

        var player = state.Entities.OfType<PlayerEntity>().First();
        player.Hp = state.PlayerStats.Hp;
    }

    public static List<Entity> SpawnRoomEntities(RoomData room, FloorRoomNode node, Dir? entryDir)
    {
        var (spawnTx, spawnTy) = entryDir is { } dir ? EntrySpawnTile(dir) : (GameConstants.RoomW / 2, GameConstants.RoomH / 2);
        var (px, py) = TileCenter(spawnTx, spawnTy);
        var entities = new List<Entity>
        {
            new PlayerEntity { Id = "player", X = px, Y = py, Radius = GameConstants.PlayerRadius },
        };

        var hasEnemies = !node.Cleared && room.Spawns.Any(s => s is EnemySpawn);

        for (var index = 0; index < room.Spawns.Count; index++)
        {
            switch (room.Spawns[index])
            {
                case EnemySpawn enemy when !node.Cleared:
                    var def = Enemies.Defs[enemy.Enemy];
                    var (ex, ey) = TileCenter(enemy.Tx, enemy.Ty);
                    entities.Add(new EnemyEntity
                    {
                        Id = $"enemy-{index}",
                        Enemy = enemy.Enemy,
                        X = ex, Y = ey,
                        Radius = def.Radius,
                        Hp = def.Hp,
                        Cooldown = def.FireCooldownMs,
                        Phase = 0,
                    });
                    break;
                case ItemSpawn item when !node.ClaimedSpawns.Contains(index):
                    var (ix, iy) = TileCenter(item.Tx, item.Ty);
                    entities.Add(new PickupEntity
                    {
                        Id = $"item-{index}",
                        Pickup = PickupKind.Item,
                        SpawnIndex = index,
                        ItemId = item.ItemId,
                        Price = item.Price,
                        X = ix, Y = iy,
                        Radius = PickupRadius,
                    });
                    break;
                case DoorSpawn door:
                    var (dx, dy) = TileCenter(door.Tx, door.Ty);
                    entities.Add(new DoorEntity
                    {
                        Id = $"door-{door.Dir}",
                        Dir = door.Dir,
                        Locked = hasEnemies,
                        X = dx, Y = dy,
                        Radius = DoorRadius,
                    });
                    break;
            }
        }
        return entities;
    }

    private static PlayerStats InitialStats() => new()
    {
        MaxHp = 6,
        Hp = 6,
        Speed = 120,
        Damage = 1,
        FireRate = 4,
        Range = 320,
        TripleShot = false,
        CoinMultiplier = 1,
        IframeUntil = 0,
    };

    private static (float X, float Y) TileCenter(int tx, int ty) =>
        ((tx + 0.5f) * GameConstants.Cell, (ty + 0.5f) * GameConstants.Cell);

    private static (int Tx, int Ty) EntrySpawnTile(Dir entryDir)
    {
        var midX = GameConstants.RoomW / 2;
        var midY = GameConstants.RoomH / 2;
        return entryDir switch
        {
            Dir.S => (midX, 1),
            Dir.N => (midX, GameConstants.RoomH - 2),
            Dir.E => (1, midY),
            _ => (GameConstants.RoomW - 2, midY),
        };
    }
}
