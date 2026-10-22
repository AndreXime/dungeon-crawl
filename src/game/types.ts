import type { TileId } from "./constants";

export type Dir = "N" | "S" | "E" | "W";
export type RoomType = "normal" | "item" | "shop" | "boss" | "start";
export type RunStatus = "playing" | "dead" | "won";
export type EnemyKind = "chaser" | "shooter" | "wander" | "boss";

export interface PlayerStats {
  maxHp: number;
  hp: number;
  speed: number;
  damage: number;
  fireRate: number;
  range: number;
  tripleShot: boolean;
  coinMultiplier: number;
  iframeUntil: number;
}

export interface FloorRoomNode {
  id: string;
  type: RoomType;
  neighbors: Partial<Record<Dir, string>>;
  cleared: boolean;
  visited: boolean;
  roomSeed: number;
  claimedSpawns: number[];
}

export interface Floor {
  depth: 1 | 2;
  rooms: Record<string, FloorRoomNode>;
  startRoomId: string;
  bossRoomId: string;
}

export interface RoomData {
  tiles: TileId[][];
  spawns: SpawnSpec[];
}

export type SpawnSpec =
  | { kind: "enemy"; enemy: EnemyKind; tx: number; ty: number }
  | { kind: "item"; itemId: string; tx: number; ty: number; price?: number }
  | { kind: "door"; dir: Dir; tx: number; ty: number };

export interface EntityBase {
  id: string;
  x: number;
  y: number;
  vx: number;
  vy: number;
  radius: number;
}

export interface PlayerEntity extends EntityBase {
  kind: "player";
  hp: number;
}

export interface EnemyEntity extends EntityBase {
  kind: "enemy";
  enemy: EnemyKind;
  hp: number;
  cooldown: number;
  phase?: number;
}

export interface ProjectileEntity extends EntityBase {
  kind: "projectile";
  owner: "player" | "enemy";
  damage: number;
  life: number;
}

export interface PickupEntity extends EntityBase {
  kind: "pickup";
  pickup: "coin" | "item" | "portal";
  itemId?: string;
  price?: number;
  amount?: number;
  spawnIndex?: number;
  activeAt?: number;
}

export interface DoorEntity extends EntityBase {
  kind: "door";
  dir: Dir;
  locked: boolean;
}

export type Entity =
  | PlayerEntity
  | EnemyEntity
  | ProjectileEntity
  | PickupEntity
  | DoorEntity;

export interface RunState {
  seed: number;
  floor: Floor;
  currentRoomId: string;
  room: RoomData;
  entities: Entity[];
  playerStats: PlayerStats;
  coins: number;
  inventory: string[];
  status: RunStatus;
  shootCooldown: number;
  timeMs: number;
}
