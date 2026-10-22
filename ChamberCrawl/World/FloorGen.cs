using System;
using System.Collections.Generic;
using System.Linq;
using ChamberCrawl.Models;

namespace ChamberCrawl.World;

public static class FloorGen
{
    private const int BossMinManhattan = 4;

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

    private static int Manhattan(int x, int y) => Math.Abs(x) + Math.Abs(y);

    private static int Manhattan(string id)
    {
        var comma = id.IndexOf(',');
        var x = int.Parse(id.AsSpan(0, comma));
        var y = int.Parse(id.AsSpan(comma + 1));
        return Manhattan(x, y);
    }

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

        LinkGridNeighbors(rooms, positions);

        var startRoomId = Key(0, 0);
        rooms[startRoomId].Type = RoomType.Start;
        rooms[startRoomId].Cleared = true;

        var reserved = new HashSet<string> { startRoomId };
        var bossRoomId = EnsureSpecialLeaf(
            rooms,
            positions,
            order,
            reserved,
            rng,
            seed,
            depth,
            requireManhattan: BossMinManhattan);
        rooms[bossRoomId].Type = RoomType.Boss;
        reserved.Add(bossRoomId);

        var shopId = EnsureSpecialLeaf(rooms, positions, order, reserved, rng, seed, depth, requireManhattan: 0);
        rooms[shopId].Type = RoomType.Shop;
        reserved.Add(shopId);

        var itemId = EnsureSpecialLeaf(rooms, positions, order, reserved, rng, seed, depth, requireManhattan: 0);
        rooms[itemId].Type = RoomType.Item;

        return new Floor
        {
            Depth = depth,
            Rooms = rooms,
            StartRoomId = startRoomId,
            BossRoomId = bossRoomId,
        };
    }

    private static void LinkGridNeighbors(
        Dictionary<string, FloorRoomNode> rooms,
        Dictionary<string, (int X, int Y)> positions)
    {
        foreach (var (id, node) in rooms)
            node.Neighbors.Clear();

        foreach (var (id, (x, y)) in positions)
        {
            foreach (var d in AllDirs)
            {
                var (dx, dy) = Delta(d);
                var nid = Key(x + dx, y + dy);
                if (!rooms.ContainsKey(nid)) continue;
                rooms[id].Neighbors[d] = nid;
                rooms[nid].Neighbors[Opposite(d)] = id;
            }
        }
    }

    private static string EnsureSpecialLeaf(
        Dictionary<string, FloorRoomNode> rooms,
        Dictionary<string, (int X, int Y)> positions,
        List<string> order,
        HashSet<string> reserved,
        Random rng,
        int seed,
        int depth,
        int requireManhattan)
    {
        var candidates = order
            .Where(id => !reserved.Contains(id))
            .Where(id => rooms[id].Neighbors.Count == 1)
            .Where(id => Manhattan(id) >= requireManhattan)
            .OrderByDescending(Manhattan)
            .ThenBy(id => id)
            .ToList();

        if (candidates.Count > 0)
            return candidates[0];

        return AttachNewLeaf(rooms, positions, order, reserved, rng, seed, depth, requireManhattan);
    }

    private static string AttachNewLeaf(
        Dictionary<string, FloorRoomNode> rooms,
        Dictionary<string, (int X, int Y)> positions,
        List<string> order,
        HashSet<string> reserved,
        Random rng,
        int seed,
        int depth,
        int requireManhattan)
    {
        var parents = order
            .Where(id => !reserved.Contains(id))
            .OrderByDescending(Manhattan)
            .ThenBy(id => id)
            .ToList();
        if (parents.Count == 0)
            throw new InvalidOperationException("Sem sala âncora para anexar especial");

        foreach (var parentId in parents)
        {
            var (px, py) = positions[parentId];
            var dirs = (Dir[])AllDirs.Clone();
            Shuffle(dirs, rng);

            foreach (var d in dirs)
            {
                var (dx, dy) = Delta(d);
                var nx = px + dx;
                var ny = py + dy;
                var nid = Key(nx, ny);
                if (positions.ContainsKey(nid)) continue;
                if (Manhattan(nx, ny) < requireManhattan) continue;
                if (!IsOnlyAdjacentTo(nx, ny, parentId, positions)) continue;

                return CreateAndLinkLeaf(
                    rooms, positions, order, parentId, d, nx, ny, nid, seed, depth);
            }
        }

        // Extende a partir da sala mais longe até atingir a Manhattan mínima.
        var anchorId = parents[0];
        while (true)
        {
            var (ax, ay) = positions[anchorId];
            Dir? bestDir = null;
            var bestManh = -1;
            var dirs = (Dir[])AllDirs.Clone();
            Shuffle(dirs, rng);
            foreach (var d in dirs)
            {
                var (dx, dy) = Delta(d);
                var nx = ax + dx;
                var ny = ay + dy;
                var nid = Key(nx, ny);
                if (positions.ContainsKey(nid)) continue;
                if (!IsOnlyAdjacentTo(nx, ny, anchorId, positions)) continue;
                var manh = Manhattan(nx, ny);
                if (manh < bestManh) continue;
                bestManh = manh;
                bestDir = d;
            }

            if (bestDir is null)
                throw new InvalidOperationException("Sem espaço para anexar sala especial");

            var (bdx, bdy) = Delta(bestDir.Value);
            var bx = ax + bdx;
            var by = ay + bdy;
            var bid = Key(bx, by);
            CreateAndLinkLeaf(
                rooms, positions, order, anchorId, bestDir.Value, bx, by, bid, seed, depth);

            if (Manhattan(bx, by) >= requireManhattan && !reserved.Contains(bid))
                return bid;

            anchorId = bid;
        }
    }

    private static bool IsOnlyAdjacentTo(
        int x,
        int y,
        string parentId,
        Dictionary<string, (int X, int Y)> positions)
    {
        string? only = null;
        var count = 0;
        foreach (var d in AllDirs)
        {
            var (dx, dy) = Delta(d);
            var nid = Key(x + dx, y + dy);
            if (!positions.ContainsKey(nid)) continue;
            count++;
            only = nid;
        }
        return count == 1 && only == parentId;
    }

    private static string CreateAndLinkLeaf(
        Dictionary<string, FloorRoomNode> rooms,
        Dictionary<string, (int X, int Y)> positions,
        List<string> order,
        string parentId,
        Dir dirFromParent,
        int x,
        int y,
        string id,
        int seed,
        int depth)
    {
        positions[id] = (x, y);
        order.Add(id);
        rooms[id] = new FloorRoomNode
        {
            Id = id,
            Type = RoomType.Normal,
            Cleared = false,
            Visited = false,
            ClaimedSpawns = new(),
            RoomSeed = unchecked(seed ^ (x * 73856093) ^ (y * 19349663) ^ depth),
        };
        rooms[parentId].Neighbors[dirFromParent] = id;
        rooms[id].Neighbors[Opposite(dirFromParent)] = parentId;
        return id;
    }
}
