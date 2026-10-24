using System.Linq;
using ChamberCrawl.Data;
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
    public void Generate_Normal_UsesDungeonEnemyPoolAndObstacleTiles()
    {
        var room = RoomGen.Generate(Node(RoomType.Normal, seed: 7), new[] { Dir.N, Dir.E }, depth: 1);
        var enemies = room.Spawns.OfType<EnemySpawn>().Select(s => s.Enemy).ToHashSet();
        Assert.Subset(new HashSet<EnemyKind> { EnemyKind.Bat, EnemyKind.Skeleton, EnemyKind.Slime }, enemies);
        Assert.DoesNotContain(EnemyKind.Specter, enemies);
        Assert.DoesNotContain(EnemyKind.Ogre, enemies);
        Assert.Contains(EnemyKind.Specter, RoomGen.EnemyPoolFor(2));

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
    public void Generate_Normal_Depth2_CanSpawnSpecter()
    {
        var found = false;
        for (var seed = 0; seed < 80 && !found; seed++)
        {
            var room = RoomGen.Generate(Node(RoomType.Normal, seed), new[] { Dir.N, Dir.E }, depth: 2);
            found = room.Spawns.OfType<EnemySpawn>().Any(s => s.Enemy == EnemyKind.Specter);
        }
        Assert.True(found);

        var sample = RoomGen.Generate(Node(RoomType.Normal, seed: 3), new[] { Dir.N }, depth: 2);
        var enemies = sample.Spawns.OfType<EnemySpawn>().Select(s => s.Enemy).ToHashSet();
        Assert.Subset(new HashSet<EnemyKind>(RoomGen.EnemyPoolFor(2)), enemies);
        Assert.DoesNotContain(EnemyKind.Slime, enemies);
        Assert.DoesNotContain(EnemyKind.Ogre, enemies);
    }

    [Fact]
    public void Generate_Boss_SpawnsOgreOnDepth1()
    {
        var boss = RoomGen.Generate(Node(RoomType.Boss), new[] { Dir.N }, depth: 1);
        Assert.Contains(boss.Spawns.OfType<EnemySpawn>(), s => s.Enemy == EnemyKind.Ogre);
        Assert.Equal(EnemyKind.Ogre, RoomGen.BossFor(1));
    }

    [Fact]
    public void Generate_Boss_SpawnsNecromancerOnDepth2()
    {
        var boss = RoomGen.Generate(Node(RoomType.Boss), new[] { Dir.N }, depth: 2);
        Assert.Contains(boss.Spawns.OfType<EnemySpawn>(), s => s.Enemy == EnemyKind.Necromancer);
        Assert.DoesNotContain(boss.Spawns.OfType<EnemySpawn>(), s => s.Enemy == EnemyKind.Ogre);
        Assert.Equal(EnemyKind.Necromancer, RoomGen.BossFor(2));
    }

    [Fact]
    public void Generate_Normal_Depth3_UsesSewerPool()
    {
        var foundRat = false;
        for (var seed = 0; seed < 80 && !foundRat; seed++)
        {
            var room = RoomGen.Generate(Node(RoomType.Normal, seed), new[] { Dir.N }, depth: 3);
            foundRat = room.Spawns.OfType<EnemySpawn>().Any(s => s.Enemy == EnemyKind.Rat);
        }
        Assert.True(foundRat);

        var sample = RoomGen.Generate(Node(RoomType.Normal, seed: 11), new[] { Dir.N }, depth: 3);
        var enemies = sample.Spawns.OfType<EnemySpawn>().Select(s => s.Enemy).ToHashSet();
        Assert.Subset(new HashSet<EnemyKind>(RoomGen.EnemyPoolFor(3)), enemies);
        Assert.DoesNotContain(EnemyKind.Skeleton, enemies);
        Assert.DoesNotContain(EnemyKind.Specter, enemies);
        Assert.Contains(EnemyKind.Rat, RoomGen.EnemyPoolFor(3));
    }

    [Fact]
    public void Generate_Boss_SpawnsSlimeKingOnDepth3()
    {
        var boss = RoomGen.Generate(Node(RoomType.Boss), new[] { Dir.N }, depth: 3);
        Assert.Contains(boss.Spawns.OfType<EnemySpawn>(), s => s.Enemy == EnemyKind.SlimeKing);
        Assert.Equal(EnemyKind.SlimeKing, RoomGen.BossFor(3));
    }

    [Fact]
    public void Generate_Normal_Depth4_UsesArmoryPool()
    {
        var foundGuard = false;
        for (var seed = 0; seed < 80 && !foundGuard; seed++)
        {
            var room = RoomGen.Generate(Node(RoomType.Normal, seed), new[] { Dir.N }, depth: 4);
            foundGuard = room.Spawns.OfType<EnemySpawn>().Any(s => s.Enemy == EnemyKind.Guard);
        }
        Assert.True(foundGuard);

        var sample = RoomGen.Generate(Node(RoomType.Normal, seed: 5), new[] { Dir.N }, depth: 4);
        var enemies = sample.Spawns.OfType<EnemySpawn>().Select(s => s.Enemy).ToHashSet();
        Assert.Subset(new HashSet<EnemyKind>(RoomGen.EnemyPoolFor(4)), enemies);
        Assert.DoesNotContain(EnemyKind.Rat, enemies);
        Assert.DoesNotContain(EnemyKind.Specter, enemies);
        Assert.Contains(EnemyKind.Guard, RoomGen.EnemyPoolFor(4));
    }

    [Fact]
    public void Generate_Boss_SpawnsIronGolemOnDepth4()
    {
        var boss = RoomGen.Generate(Node(RoomType.Boss), new[] { Dir.N }, depth: 4);
        Assert.Contains(boss.Spawns.OfType<EnemySpawn>(), s => s.Enemy == EnemyKind.IronGolem);
        Assert.Equal(EnemyKind.IronGolem, RoomGen.BossFor(4));
    }

    [Fact]
    public void Generate_Normal_Depth4_HasNoPuddles()
    {
        var room = RoomGen.Generate(Node(RoomType.Normal, seed: 9), new[] { Dir.N }, depth: 4);
        for (var y = 0; y < GameConstants.RoomH; y++)
        for (var x = 0; x < GameConstants.RoomW; x++)
            Assert.NotEqual(GameConstants.TilePuddle, room.Tiles[y, x]);
    }

    [Fact]
    public void Generate_Normal_Depth3_PlacesPuddles()
    {
        var found = false;
        for (var seed = 0; seed < 40 && !found; seed++)
        {
            var room = RoomGen.Generate(Node(RoomType.Normal, seed), new[] { Dir.N }, depth: 3);
            for (var y = 0; y < GameConstants.RoomH && !found; y++)
            for (var x = 0; x < GameConstants.RoomW && !found; x++)
                found = room.Tiles[y, x] == GameConstants.TilePuddle;
        }
        Assert.True(found);

        var depth1 = RoomGen.Generate(Node(RoomType.Normal, seed: 7), new[] { Dir.N }, depth: 1);
        for (var y = 0; y < GameConstants.RoomH; y++)
        for (var x = 0; x < GameConstants.RoomW; x++)
            Assert.NotEqual(GameConstants.TilePuddle, depth1.Tiles[y, x]);
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
        Assert.True(Items.Defs.ContainsKey(freeItems[0].ItemId));

        var shop = RoomGen.Generate(Node(RoomType.Shop), new[] { Dir.E });
        var shopItems = shop.Spawns.OfType<ItemSpawn>().ToList();
        Assert.InRange(shopItems.Count, 2, 3);
        Assert.All(shopItems, s =>
        {
            Assert.True(s.Price > 0);
            Assert.True(Items.Defs.ContainsKey(s.ItemId));
            var rarity = Items.Defs[s.ItemId].Rarity;
            var price = s.Price!.Value;
            switch (rarity)
            {
                case ItemRarity.Common:
                    Assert.InRange(price, 4, 7);
                    break;
                case ItemRarity.Rare:
                    Assert.InRange(price, 8, 12);
                    break;
                case ItemRarity.Legendary:
                    Assert.InRange(price, 15, 20);
                    break;
            }
        });

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
