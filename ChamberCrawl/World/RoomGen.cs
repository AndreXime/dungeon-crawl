using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.World;

public static class RoomGen
{
    private static readonly string[] ItemIds =
    {
        "damage_up", "speed_up", "hp_up", "fire_rate", "triple", "greedy",
    };

    private static readonly EnemyKind[] EnemyPool =
    {
        EnemyKind.Pspam, EnemyKind.Trojan,
    };

    private static (int Tx, int Ty) DoorCoords(Dir dir)
    {
        var midX = GameConstants.RoomW / 2;
        var midY = GameConstants.RoomH / 2;
        return dir switch
        {
            Dir.N => (midX, 0),
            Dir.S => (midX, GameConstants.RoomH - 1),
            Dir.W => (0, midY),
            _ => (GameConstants.RoomW - 1, midY),
        };
    }

    public static RoomData Generate(FloorRoomNode node, IReadOnlyList<Dir> neighbors)
    {
        const int w = GameConstants.RoomW;
        const int h = GameConstants.RoomH;
        var rng = new Random(node.RoomSeed);

        var tiles = new int[h, w];
        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
                tiles[y, x] = x == 0 || y == 0 || x == w - 1 || y == h - 1
                    ? GameConstants.TileWall
                    : GameConstants.TileFloor;

        var spawns = new List<SpawnSpec>();

        foreach (var dir in neighbors)
        {
            var (tx, ty) = DoorCoords(dir);
            tiles[ty, tx] = GameConstants.TileDoor;
            spawns.Add(new DoorSpawn { Dir = dir, Tx = tx, Ty = ty });
        }

        var centerX = w / 2;
        var centerY = h / 2;
        var itemIndex = 0;

        switch (node.Type)
        {
            case RoomType.Normal:
            {
                var obstacleCount = 2 + rng.Next(4);
                for (var i = 0; i < obstacleCount; i++)
                {
                    var tx = 2 + rng.Next(w - 4);
                    var ty = 2 + rng.Next(h - 4);
                    if (tx == centerX && ty == centerY) continue;
                    tiles[ty, tx] = GameConstants.TileObstacle;
                }

                var enemyCount = 3 + rng.Next(3);
                var occupied = new HashSet<(int, int)> { (centerX, centerY) };

                for (var i = 0; i < enemyCount; i++)
                {
                    var enemy = EnemyPool[rng.Next(EnemyPool.Length)];
                    var placed = false;
                    for (var attempt = 0; attempt < 20 && !placed; attempt++)
                    {
                        var tx = 2 + rng.Next(w - 4);
                        var ty = 2 + rng.Next(h - 4);
                        if (tiles[ty, tx] != GameConstants.TileFloor || !occupied.Add((tx, ty))) continue;
                        spawns.Add(new EnemySpawn { Enemy = enemy, Tx = tx, Ty = ty });
                        placed = true;
                    }
                    if (!placed)
                        spawns.Add(new EnemySpawn { Enemy = enemy, Tx = 2 + i, Ty = 2 });
                }
                break;
            }
            case RoomType.Item:
                spawns.Add(new ItemSpawn
                {
                    ItemId = ItemIds[rng.Next(ItemIds.Length)],
                    Tx = centerX,
                    Ty = centerY,
                    SpawnIndex = itemIndex++,
                });
                break;
            case RoomType.Shop:
            {
                var count = 2 + rng.Next(2);
                var pool = ItemIds.OrderBy(_ => rng.NextDouble()).ToList();
                var startX = centerX - count / 2 * 2;
                for (var i = 0; i < count; i++)
                {
                    spawns.Add(new ItemSpawn
                    {
                        ItemId = pool[i],
                        Tx = startX + i * 2,
                        Ty = centerY,
                        Price = 5 + rng.Next(6),
                        SpawnIndex = itemIndex++,
                    });
                }
                break;
            }
            case RoomType.Boss:
                spawns.Add(new EnemySpawn { Enemy = EnemyKind.AdwareKing, Tx = centerX, Ty = centerY });
                break;
        }

        return new RoomData { Tiles = tiles, Spawns = spawns };
    }
}
