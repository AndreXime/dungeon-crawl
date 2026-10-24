using System.Collections.Generic;

namespace ChamberCrawl.Models;

public enum Dir { N, S, E, W }
public enum RoomType { Normal, Item, Shop, Boss, Start }
public enum RunStatus { Playing, Dead, Won }
public enum EnemyKind { Bat, Skeleton, Slime, Specter, Rat, Guard, Acolyte, Ogre, Necromancer, SlimeKing, IronGolem, RuneLord }
public enum EntityKind { Player, Enemy, Projectile, Pickup, Door }
public enum PickupKind { Coin, Item, Portal }

public sealed class PlayerStats
{
    public int MaxHp { get; set; }
    public int Hp { get; set; }
    public float Speed { get; set; }
    public float Damage { get; set; }
    public float FireRate { get; set; }
    public float Range { get; set; }
    public bool TripleShot { get; set; }
    public int CoinMultiplier { get; set; }
    public float IframeUntil { get; set; }
}

public sealed class FloorRoomNode
{
    public string Id { get; set; } = "";
    public RoomType Type { get; set; }
    public Dictionary<Dir, string> Neighbors { get; set; } = new();
    public bool Cleared { get; set; }
    public bool Visited { get; set; }
    public int RoomSeed { get; set; }
    public List<int> ClaimedSpawns { get; set; } = new();
    public List<LooseCoin> LooseCoins { get; set; } = new();
}

public sealed class LooseCoin
{
    public float X { get; set; }
    public float Y { get; set; }
    public int Amount { get; set; }
}

public sealed class Floor
{
    public int Depth { get; set; } // 1 .. GameConstants.MaxDepth
    public Dictionary<string, FloorRoomNode> Rooms { get; set; } = new();
    public string StartRoomId { get; set; } = "";
    public string BossRoomId { get; set; } = "";
}

public sealed class RoomData
{
    public int[,] Tiles { get; set; } = new int[GameConstants.RoomH, GameConstants.RoomW];
    public List<SpawnSpec> Spawns { get; set; } = new();
}

public abstract class SpawnSpec { }
public sealed class EnemySpawn : SpawnSpec
{
    public EnemyKind Enemy { get; set; }
    public int Tx { get; set; }
    public int Ty { get; set; }
}
public sealed class ItemSpawn : SpawnSpec
{
    public string ItemId { get; set; } = "";
    public int Tx { get; set; }
    public int Ty { get; set; }
    public int? Price { get; set; }
    public int SpawnIndex { get; set; }
}
public sealed class DoorSpawn : SpawnSpec
{
    public Dir Dir { get; set; }
    public int Tx { get; set; }
    public int Ty { get; set; }
}

public abstract class Entity
{
    public string Id { get; set; } = "";
    public EntityKind Kind { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Vx { get; set; }
    public float Vy { get; set; }
    public float Radius { get; set; }
}

public sealed class PlayerEntity : Entity
{
    public int Hp { get; set; }
    public PlayerEntity() { Kind = EntityKind.Player; }
}

public sealed class EnemyEntity : Entity
{
    public EnemyKind Enemy { get; set; }
    public float Hp { get; set; }
    public float Cooldown { get; set; }
    public float Phase { get; set; }
    public bool CanSplit { get; set; }
    public float IframeUntil { get; set; }
    public EnemyEntity() { Kind = EntityKind.Enemy; }
}

public sealed class ProjectileEntity : Entity
{
    public bool FromPlayer { get; set; }
    public float Damage { get; set; }
    public float Life { get; set; }
    public ProjectileEntity() { Kind = EntityKind.Projectile; }
}

public sealed class PickupEntity : Entity
{
    public PickupKind Pickup { get; set; }
    public string? ItemId { get; set; }
    public int? Price { get; set; }
    public int Amount { get; set; } = 1;
    public int? SpawnIndex { get; set; }
    public float? ActiveAt { get; set; }
    public PickupEntity() { Kind = EntityKind.Pickup; }
}

public sealed class DoorEntity : Entity
{
    public Dir Dir { get; set; }
    public bool Locked { get; set; }
    public DoorEntity() { Kind = EntityKind.Door; }
}

public sealed class RunState
{
    public int Seed { get; set; }
    public Floor Floor { get; set; } = new();
    public string CurrentRoomId { get; set; } = "";
    public RoomData Room { get; set; } = new();
    public List<Entity> Entities { get; set; } = new();
    public PlayerStats PlayerStats { get; set; } = new();
    public int Coins { get; set; }
    public List<string> Inventory { get; set; } = new();
    public RunStatus Status { get; set; } = RunStatus.Playing;
    public float ShootCooldown { get; set; }
    public float TimeMs { get; set; }
}
