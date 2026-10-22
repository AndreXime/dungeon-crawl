using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.World;

public static class RoomGen
{
    /// <summary>Distância Manhattan mínima entre inimigo e cada tile de entrada (porta).</summary>
    public const int MinEnemySpawnDistFromEntry = 4;

    private static readonly string[] ItemIds =
    {
        "damage_up", "speed_up", "hp_up", "fire_rate", "triple", "greedy",
    };

    private static readonly EnemyKind[] EnemyPool =
    {
        EnemyKind.Pspam, EnemyKind.Trojan,
    };

    private static readonly int[] PartitionObstacleTiles =
    {
        GameConstants.TileFolder,
        GameConstants.TilePopup,
        GameConstants.TileBadSector,
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
                    tiles[ty, tx] = PartitionObstacleTiles[rng.Next(PartitionObstacleTiles.Length)];
                }

                var enemyCount = 3 + rng.Next(3);
                var entries = EntryTiles(neighbors);
                var occupied = new HashSet<(int, int)> { (centerX, centerY) };
                foreach (var entry in entries)
                    occupied.Add(entry);

                for (var i = 0; i < enemyCount; i++)
                {
                    var enemy = EnemyPool[rng.Next(EnemyPool.Length)];
                    var placed = false;
                    for (var attempt = 0; attempt < 40 && !placed; attempt++)
                    {
                        var (tx, ty) = enemy == EnemyKind.Trojan
                            ? PickTrojanTile(rng, w, h)
                            : (2 + rng.Next(w - 4), 2 + rng.Next(h - 4));
                        if (tiles[ty, tx] != GameConstants.TileFloor) continue;
                        if (!FarFromEntries(tx, ty, entries, MinEnemySpawnDistFromEntry)) continue;
                        if (!occupied.Add((tx, ty))) continue;
                        spawns.Add(new EnemySpawn { Enemy = enemy, Tx = tx, Ty = ty });
                        placed = true;
                    }
                    if (!placed)
                    {
                        var fallback = FindSafeEnemyTile(tiles, occupied, entries, w, h);
                        if (fallback is { } tile)
                        {
                            occupied.Add(tile);
                            spawns.Add(new EnemySpawn { Enemy = enemy, Tx = tile.Tx, Ty = tile.Ty });
                        }
                    }
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

    /// <summary>Prefere cantos / longe do centro da sala para Trojan.</summary>
    private static (int Tx, int Ty) PickTrojanTile(Random rng, int w, int h)
    {
        var corner = rng.Next(4);
        var (baseX, baseY) = corner switch
        {
            0 => (2, 2),
            1 => (w - 5, 2),
            2 => (2, h - 5),
            _ => (w - 5, h - 5),
        };
        return (baseX + rng.Next(3), baseY + rng.Next(3));
    }

    /// <summary>Tile onde o jogador aparece ao entrar por aquela porta da sala.</summary>
    public static (int Tx, int Ty) EntryTileForDoor(Dir doorDir)
    {
        var midX = GameConstants.RoomW / 2;
        var midY = GameConstants.RoomH / 2;
        return doorDir switch
        {
            Dir.N => (midX, 1),
            Dir.S => (midX, GameConstants.RoomH - 2),
            Dir.W => (1, midY),
            _ => (GameConstants.RoomW - 2, midY),
        };
    }

    private static List<(int Tx, int Ty)> EntryTiles(IReadOnlyList<Dir> neighbors)
    {
        var entries = new List<(int Tx, int Ty)>(neighbors.Count);
        foreach (var dir in neighbors)
            entries.Add(EntryTileForDoor(dir));
        return entries;
    }

    private static bool FarFromEntries(
        int tx,
        int ty,
        IReadOnlyList<(int Tx, int Ty)> entries,
        int minDist)
    {
        foreach (var (ex, ey) in entries)
        {
            if (Math.Abs(tx - ex) + Math.Abs(ty - ey) < minDist)
                return false;
        }
        return true;
    }

    private static (int Tx, int Ty)? FindSafeEnemyTile(
        int[,] tiles,
        HashSet<(int, int)> occupied,
        IReadOnlyList<(int Tx, int Ty)> entries,
        int w,
        int h)
    {
        (int Tx, int Ty)? best = null;
        var bestScore = -1;
        for (var ty = 2; ty < h - 2; ty++)
        {
            for (var tx = 2; tx < w - 2; tx++)
            {
                if (tiles[ty, tx] != GameConstants.TileFloor) continue;
                if (occupied.Contains((tx, ty))) continue;
                var minEntry = int.MaxValue;
                foreach (var (ex, ey) in entries)
                    minEntry = Math.Min(minEntry, Math.Abs(tx - ex) + Math.Abs(ty - ey));
                if (minEntry < MinEnemySpawnDistFromEntry) continue;
                if (minEntry > bestScore)
                {
                    bestScore = minEntry;
                    best = (tx, ty);
                }
            }
        }
        return best;
    }
}
