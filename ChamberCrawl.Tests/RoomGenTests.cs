using System.Linq;
using ChamberCrawl.Models;
using ChamberCrawl.World;

namespace ChamberCrawl.Tests;

public class RoomGenTests
{
    private static FloorRoomNode Node(RoomType type = RoomType.Normal, int seed = 99) => new()
    {
        Id = "0,0",
        Type = type,
        Neighbors = new() { [Dir.N] = "0,-1", [Dir.E] = "1,0" },
        RoomSeed = seed,
    };

    [Fact]
    public void Generate_Normal_UsesPartitionEnemyPoolAndObstacleTiles()
    {
        var room = RoomGen.Generate(Node(RoomType.Normal, seed: 7), new[] { Dir.N, Dir.E });
        var enemies = room.Spawns.OfType<EnemySpawn>().Select(s => s.Enemy).ToHashSet();
        Assert.Subset(new HashSet<EnemyKind> { EnemyKind.Bat, EnemyKind.Skeleton, EnemyKind.Slime }, enemies);
        Assert.DoesNotContain(EnemyKind.Ogre, enemies);

        var obstacleTiles = new HashSet<int>();
        for (var y = 0; y < GameConstants.RoomH; y++)
            for (var x = 0; x < GameConstants.RoomW; x++)
            {
                var t = room.Tiles[y, x];
                if (t is GameConstants.TileCrate or GameConstants.TileBarrel or GameConstants.TileSpike)
                    obstacleTiles.Add(t);
            }
        Assert.NotEmpty(obstacleTiles);
    }

    [Fact]
    public void Generate_Boss_SpawnsOgre()
    {
        var boss = RoomGen.Generate(Node(RoomType.Boss), new[] { Dir.N });
        Assert.Contains(boss.Spawns.OfType<EnemySpawn>(), s => s.Enemy == EnemyKind.Ogre);
    }

    [Fact]
    public void Generate_Normal_EnemiesStayAwayFromDoorEntries()
    {
        var neighbors = new[] { Dir.N, Dir.E, Dir.S };
        var entries = neighbors.Select(RoomGen.EntryTileForDoor).ToList();

        for (var seed = 0; seed < 60; seed++)
        {
            var room = RoomGen.Generate(Node(RoomType.Normal, seed), neighbors);
            foreach (var enemy in room.Spawns.OfType<EnemySpawn>())
            {
                foreach (var (ex, ey) in entries)
                {
                    var dist = Math.Abs(enemy.Tx - ex) + Math.Abs(enemy.Ty - ey);
                    Assert.True(
                        dist >= RoomGen.MinEnemySpawnDistFromEntry,
                        $"seed {seed}: {enemy.Enemy} at ({enemy.Tx},{enemy.Ty}) dist {dist} from entry ({ex},{ey})");
                }
            }
        }
    }

    [Fact]
    public void Generate_CreatesWallBorderAndDoorsOnNeighbors()
    {
        var room = RoomGen.Generate(Node(), new[] { Dir.N, Dir.E });
        Assert.Equal(GameConstants.RoomH, room.Tiles.GetLength(0));
        Assert.Equal(GameConstants.RoomW, room.Tiles.GetLength(1));
        Assert.Equal(GameConstants.TileDoor, room.Tiles[0, GameConstants.RoomW / 2]);
        Assert.Equal(GameConstants.TileDoor, room.Tiles[GameConstants.RoomH / 2, GameConstants.RoomW - 1]);
        Assert.Equal(GameConstants.TileWall, room.Tiles[0, 0]);
        Assert.Contains(room.Spawns, s => s is EnemySpawn);
        Assert.Equal(2, room.Spawns.OfType<DoorSpawn>().Count());
    }

    [Fact]
    public void Generate_ProducesSpawnsPerRoomType()
    {
        var start = RoomGen.Generate(Node(RoomType.Start), new[] { Dir.S });
        Assert.DoesNotContain(start.Spawns, s => s is EnemySpawn);

        var normal = RoomGen.Generate(Node(RoomType.Normal), new[] { Dir.N, Dir.S });
        Assert.InRange(normal.Spawns.OfType<EnemySpawn>().Count(), 3, 5);

        var item = RoomGen.Generate(Node(RoomType.Item), new[] { Dir.W });
        var freeItems = item.Spawns.OfType<ItemSpawn>().ToList();
        Assert.Single(freeItems);
        Assert.Null(freeItems[0].Price);

        var shop = RoomGen.Generate(Node(RoomType.Shop), new[] { Dir.E });
        var shopItems = shop.Spawns.OfType<ItemSpawn>().ToList();
        Assert.InRange(shopItems.Count, 2, 3);
        Assert.All(shopItems, s => Assert.True(s.Price > 0));

        var boss = RoomGen.Generate(Node(RoomType.Boss), new[] { Dir.N });
        var bosses = boss.Spawns.OfType<EnemySpawn>().Where(s => s.Enemy == EnemyKind.Ogre).ToList();
        Assert.Single(bosses);
        Assert.Equal(GameConstants.RoomW / 2, bosses[0].Tx);
        Assert.Equal(GameConstants.RoomH / 2, bosses[0].Ty);
    }

    [Fact]
    public void Generate_AssignsSequentialSpawnIndexToItems()
    {
        var shop = RoomGen.Generate(Node(RoomType.Shop), new[] { Dir.E });
        var indexes = shop.Spawns.OfType<ItemSpawn>().Select(s => s.SpawnIndex).ToList();
        Assert.Equal(Enumerable.Range(0, indexes.Count), indexes);
    }

    [Fact]
    public void Generate_ShopItemsAreDistinct()
    {
        var shop = RoomGen.Generate(Node(RoomType.Shop), new[] { Dir.E });
        var ids = shop.Spawns.OfType<ItemSpawn>().Select(s => s.ItemId).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Generate_IsDeterministicForSameRoomSeed()
    {
        var a = RoomGen.Generate(Node(), new[] { Dir.N, Dir.E });
        var b = RoomGen.Generate(Node(), new[] { Dir.N, Dir.E });
        Assert.Equal(a.Tiles.Cast<int>(), b.Tiles.Cast<int>());
        Assert.Equal(Describe(a), Describe(b));
    }

    private static string[] Describe(RoomData room) => room.Spawns.Select(s => s switch
    {
        EnemySpawn e => $"E:{e.Enemy}:{e.Tx}:{e.Ty}",
        ItemSpawn i => $"I:{i.ItemId}:{i.Tx}:{i.Ty}:{i.Price}:{i.SpawnIndex}",
        DoorSpawn d => $"D:{d.Dir}:{d.Tx}:{d.Ty}",
        _ => "?",
    }).ToArray();
}
