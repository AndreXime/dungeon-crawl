import { ENEMIES } from "../data/enemies";
import { generateFloor } from "../world/floorGen";
import { generateRoom } from "../world/roomGen";
import { CELL, PLAYER_RADIUS, ROOM_H, ROOM_W } from "./constants";
import type {
  Dir,
  Entity,
  FloorRoomNode,
  PlayerStats,
  RoomData,
  RunState,
} from "./types";

const OPPOSITE: Record<Dir, Dir> = { N: "S", S: "N", E: "W", W: "E" };
const DOOR_RADIUS = CELL / 2;
const PICKUP_RADIUS = 10;

function tileCenter(tx: number, ty: number): { x: number; y: number } {
  return { x: (tx + 0.5) * CELL, y: (ty + 0.5) * CELL };
}

function initialStats(): PlayerStats {
  return {
    maxHp: 6,
    hp: 6,
    speed: 120,
    damage: 1,
    fireRate: 4,
    range: 320,
    tripleShot: false,
    coinMultiplier: 1,
    iframeUntil: 0,
  };
}

function entrySpawnTile(entryDir: Dir): { tx: number; ty: number } {
  const midX = Math.floor(ROOM_W / 2);
  const midY = Math.floor(ROOM_H / 2);
  switch (OPPOSITE[entryDir]) {
    case "N":
      return { tx: midX, ty: 1 };
    case "S":
      return { tx: midX, ty: ROOM_H - 2 };
    case "W":
      return { tx: 1, ty: midY };
    case "E":
      return { tx: ROOM_W - 2, ty: midY };
  }
}

function neighborDirs(node: FloorRoomNode): Dir[] {
  return (Object.keys(node.neighbors) as Dir[]).filter(
    (dir) => node.neighbors[dir] !== undefined,
  );
}

/**
 * `entryDir` é a direção em que o jogador atravessou a porta anterior.
 * Ele surge ao lado da porta oposta da nova sala.
 */
export function spawnRoomEntities(
  room: RoomData,
  node: FloorRoomNode,
  entryDir?: Dir,
): Entity[] {
  const spawnTile = entryDir
    ? entrySpawnTile(entryDir)
    : { tx: Math.floor(ROOM_W / 2), ty: Math.floor(ROOM_H / 2) };
  const entities: Entity[] = [
    {
      kind: "player",
      id: "player",
      ...tileCenter(spawnTile.tx, spawnTile.ty),
      vx: 0,
      vy: 0,
      radius: PLAYER_RADIUS,
      hp: 0,
    },
  ];

  const hasEnemies = !node.cleared && room.spawns.some((s) => s.kind === "enemy");

  room.spawns.forEach((spawn, index) => {
    const pos = tileCenter(spawn.tx, spawn.ty);
    switch (spawn.kind) {
      case "enemy": {
        if (node.cleared) return;
        const def = ENEMIES[spawn.enemy];
        entities.push({
          kind: "enemy",
          id: `enemy-${index}`,
          enemy: spawn.enemy,
          ...pos,
          vx: 0,
          vy: 0,
          radius: def.radius,
          hp: def.hp,
          cooldown: def.fireCooldownMs,
          ...(spawn.enemy === "boss" ? { phase: 1 } : {}),
        });
        break;
      }
      case "item":
        entities.push({
          kind: "pickup",
          id: `item-${index}`,
          pickup: "item",
          itemId: spawn.itemId,
          ...(spawn.price !== undefined ? { price: spawn.price } : {}),
          ...pos,
          vx: 0,
          vy: 0,
          radius: PICKUP_RADIUS,
        });
        break;
      case "door":
        entities.push({
          kind: "door",
          id: `door-${spawn.dir}`,
          dir: spawn.dir,
          locked: hasEnemies,
          ...pos,
          vx: 0,
          vy: 0,
          radius: DOOR_RADIUS,
        });
        break;
    }
  });

  return entities;
}

function loadRoom(state: RunState, roomId: string, entryDir?: Dir): void {
  const node = state.floor.rooms[roomId];
  if (!node) throw new Error(`Sala inexistente: ${roomId}`);

  node.visited = true;
  state.currentRoomId = roomId;
  state.room = generateRoom(node, neighborDirs(node));
  state.entities = spawnRoomEntities(state.room, node, entryDir);

  const player = state.entities.find((e) => e.kind === "player");
  if (player?.kind === "player") player.hp = state.playerStats.hp;
}

export function enterRoom(
  state: RunState,
  roomId: string,
  entryDir?: Dir,
): void {
  loadRoom(state, roomId, entryDir);
}

export function createRun(seed: number): RunState {
  const floor = generateFloor(seed, 1);
  const startNode = floor.rooms[floor.startRoomId];
  if (!startNode) throw new Error("Andar sem sala inicial");

  const state: RunState = {
    seed,
    floor,
    currentRoomId: floor.startRoomId,
    room: { tiles: [], spawns: [] },
    entities: [],
    playerStats: initialStats(),
    coins: 0,
    inventory: [],
    status: "playing",
    shootCooldown: 0,
    timeMs: 0,
  };

  loadRoom(state, floor.startRoomId);
  return state;
}
