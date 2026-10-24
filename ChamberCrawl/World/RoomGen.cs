using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Data;
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.World;

public static class RoomGen
{
    /// <summary>Distância Manhattan mínima entre inimigo e cada tile de entrada (porta).</summary>
    public const int MinEnemySpawnDistFromEntry = 4;

    private static readonly EnemyKind[] EnemyPoolDepth1 =
    {
        EnemyKind.Bat, EnemyKind.Skeleton, EnemyKind.Slime,
    };

    private static readonly EnemyKind[] EnemyPoolDepth2 =
    {
        EnemyKind.Skeleton, EnemyKind.Skeleton, EnemyKind.Bat, EnemyKind.Specter, EnemyKind.Specter,
    };

    private static readonly EnemyKind[] EnemyPoolDepth3 =
    {
        EnemyKind.Slime, EnemyKind.Slime, EnemyKind.Slime, EnemyKind.Bat, EnemyKind.Rat, EnemyKind.Rat,
    };

    private static readonly EnemyKind[] EnemyPoolDepth4 =
    {
        EnemyKind.Guard, EnemyKind.Guard, EnemyKind.Skeleton, EnemyKind.Skeleton, EnemyKind.Slime,
    };

    private static readonly int[] DungeonObstacleTiles =
    {
        GameConstants.TileCrate,
        GameConstants.TileBarrel,
        GameConstants.TileSpike,
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

    public static RoomData Generate(FloorRoomNode node, IReadOnlyList<Dir> neighbors, int depth = 1)
    {
        const int w = GameConstants.RoomW;
        const int h = GameConstants.RoomH;
        var rng = new Random(node.RoomSeed);
        var enemyPool = EnemyPoolFor(depth);

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
                    tiles[ty, tx] = DungeonObstacleTiles[rng.Next(DungeonObstacleTiles.Length)];
                }

                if (depth == 3)
                    ScatterPuddles(tiles, rng, centerX, centerY, count: 4 + rng.Next(5));

                var enemyCount = 3 + rng.Next(3)
                    + (depth >= 2 ? 1 : 0)
                    + (depth >= 3 ? 1 : 0)
                    + (depth >= 4 ? 1 : 0);
                var entries = EntryTiles(neighbors);
                var occupied = new HashSet<(int, int)> { (centerX, centerY) };
                foreach (var entry in entries)
                    occupied.Add(entry);

                for (var i = 0; i < enemyCount; i++)
                {
                    var enemy = enemyPool[rng.Next(enemyPool.Length)];
                    var placed = false;
                    for (var attempt = 0; attempt < 40 && !placed; attempt++)
                    {
                        var (tx, ty) = (2 + rng.Next(w - 4), 2 + rng.Next(h - 4));
                        if (tiles[ty, tx] is not (GameConstants.TileFloor or GameConstants.TilePuddle))
                            continue;
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
            {
                var free = Items.PickWeighted(rng);
                spawns.Add(new ItemSpawn
                {
                    ItemId = free.Id,
                    Tx = centerX,
                    Ty = centerY,
                    SpawnIndex = itemIndex++,
                });
                break;
            }
            case RoomType.Shop:
            {
                var count = 2 + rng.Next(2);
                var picked = new List<string>();
                var startX = centerX - count / 2 * 2;
                for (var i = 0; i < count; i++)
                {
                    var def = Items.PickWeighted(rng, picked);
                    picked.Add(def.Id);
                    spawns.Add(new ItemSpawn
                    {
                        ItemId = def.Id,
                        Tx = startX + i * 2,
                        Ty = centerY,
                        Price = Items.ShopPrice(def.Rarity, rng),
                        SpawnIndex = itemIndex++,
                    });
                }
                break;
            }
            case RoomType.Boss:
                if (depth == 3)
                    ScatterPuddles(tiles, rng, centerX, centerY, count: 3 + rng.Next(3), avoidCenter: true);
                spawns.Add(new EnemySpawn { Enemy = BossFor(depth), Tx = centerX, Ty = centerY });
                break;
        }

        return new RoomData { Tiles = tiles, Spawns = spawns };
    }

    private static void ScatterPuddles(
        int[,] tiles,
        Random rng,
        int centerX,
        int centerY,
        int count,
        bool avoidCenter = false)
    {
        var h = tiles.GetLength(0);
        var w = tiles.GetLength(1);
        for (var i = 0; i < count; i++)
        {
            for (var attempt = 0; attempt < 30; attempt++)
            {
                var tx = 2 + rng.Next(w - 4);
                var ty = 2 + rng.Next(h - 4);
                if (avoidCenter && tx == centerX && ty == centerY) continue;
                if (tiles[ty, tx] != GameConstants.TileFloor) continue;
                tiles[ty, tx] = GameConstants.TilePuddle;
                break;
            }
        }
    }

    public static EnemyKind[] EnemyPoolFor(int depth) => depth switch
    {
        >= 4 => EnemyPoolDepth4,
        >= 3 => EnemyPoolDepth3,
        >= 2 => EnemyPoolDepth2,
        _ => EnemyPoolDepth1,
    };

    public static EnemyKind BossFor(int depth) => depth switch
    {
        >= 4 => EnemyKind.IronGolem,
        >= 3 => EnemyKind.SlimeKing,
        >= 2 => EnemyKind.Necromancer,
        _ => EnemyKind.Ogre,
    };

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
                if (tiles[ty, tx] is not (GameConstants.TileFloor or GameConstants.TilePuddle)) continue;
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
