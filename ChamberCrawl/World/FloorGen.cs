using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.World;

public static class FloorGen
{
    private static readonly Dir[] AllDirs = { Dir.N, Dir.S, Dir.E, Dir.W };

    private static Dir Opposite(Dir d) => d switch
    {
        Dir.N => Dir.S,
        Dir.S => Dir.N,
        Dir.E => Dir.W,
        _ => Dir.E,
    };

    private static (int Dx, int Dy) Delta(Dir d) => d switch
    {
        Dir.N => (0, -1),
        Dir.S => (0, 1),
        Dir.E => (1, 0),
        _ => (-1, 0),
    };

    private static string Key(int x, int y) => $"{x},{y}";

    private static void Shuffle<T>(IList<T> list, Random rng)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static Floor Generate(int seed, int depth)
    {
        var rng = new Random(unchecked(seed + depth * 10007));
        var target = 6 + rng.Next(3);

        var order = new List<string>();
        var positions = new Dictionary<string, (int X, int Y)>();
        positions[Key(0, 0)] = (0, 0);
        order.Add(Key(0, 0));
        var frontier = new List<(int X, int Y)> { (0, 0) };

        while (positions.Count < target && frontier.Count > 0)
        {
            var i = rng.Next(frontier.Count);
            var cur = frontier[i];
            var dirs = (Dir[])AllDirs.Clone();
            Shuffle(dirs, rng);

            var grew = false;
            foreach (var d in dirs)
            {
                var (dx, dy) = Delta(d);
                var nx = cur.X + dx;
                var ny = cur.Y + dy;
                var k = Key(nx, ny);
                if (positions.ContainsKey(k)) continue;
                positions[k] = (nx, ny);
                order.Add(k);
                frontier.Add((nx, ny));
                grew = true;
                break;
            }
            if (!grew) frontier.RemoveAt(i);
        }

        var rooms = new Dictionary<string, FloorRoomNode>();
        foreach (var id in order)
        {
            var (x, y) = positions[id];
            rooms[id] = new FloorRoomNode
            {
                Id = id,
                Type = RoomType.Normal,
                Cleared = false,
                Visited = false,
                ClaimedSpawns = new(),
                RoomSeed = unchecked(seed ^ (x * 73856093) ^ (y * 19349663) ^ depth),
            };
        }

        foreach (var id in order)
        {
            var (x, y) = positions[id];
            foreach (var d in AllDirs)
            {
                var (dx, dy) = Delta(d);
                var nid = Key(x + dx, y + dy);
                if (!rooms.TryGetValue(nid, out var neighbor)) continue;
                rooms[id].Neighbors[d] = nid;
                neighbor.Neighbors[Opposite(d)] = id;
            }
        }

        var startRoomId = Key(0, 0);
        var bossRoomId = startRoomId;
        var best = -1;
        foreach (var id in order)
        {
            var (x, y) = positions[id];
            var dist = Math.Abs(x) + Math.Abs(y);
            if (dist > best)
            {
                best = dist;
                bossRoomId = id;
            }
        }

        rooms[startRoomId].Type = RoomType.Start;
        rooms[startRoomId].Cleared = true;
        rooms[bossRoomId].Type = RoomType.Boss;

        var others = order.Where(id => id != startRoomId && id != bossRoomId).ToList();
        Shuffle(others, rng);
        if (others.Count > 0) rooms[others[0]].Type = RoomType.Shop;
        if (others.Count > 1) rooms[others[1]].Type = RoomType.Item;

        return new Floor
        {
            Depth = depth,
            Rooms = rooms,
            StartRoomId = startRoomId,
            BossRoomId = bossRoomId,
        };
    }
}
