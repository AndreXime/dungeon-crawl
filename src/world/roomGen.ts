import { RNG } from "rot-js";
import { ROOM_W, ROOM_H, TILE, type TileId } from "../game/constants";
import type {
  Dir,
  EnemyKind,
  FloorRoomNode,
  RoomData,
  SpawnSpec,
} from "../game/types";

const ITEM_IDS = [
  "damage_up",
  "speed_up",
  "hp_up",
  "fire_rate",
  "triple",
  "greedy",
] as const;

const ENEMY_POOL: readonly EnemyKind[] = ["chaser", "wander", "shooter"];

function doorCoords(dir: Dir): { tx: number; ty: number } {
  const midX = Math.floor(ROOM_W / 2);
  const midY = Math.floor(ROOM_H / 2);
  switch (dir) {
    case "N":
      return { tx: midX, ty: 0 };
    case "S":
      return { tx: midX, ty: ROOM_H - 1 };
    case "W":
      return { tx: 0, ty: midY };
    case "E":
      return { tx: ROOM_W - 1, ty: midY };
  }
}

export function generateRoom(
  node: FloorRoomNode,
  neighbors: Dir[],
): RoomData {
  RNG.setSeed(node.roomSeed);

  const tiles: TileId[][] = Array.from({ length: ROOM_H }, (_, y) =>
    Array.from({ length: ROOM_W }, (_, x) =>
      x === 0 || y === 0 || x === ROOM_W - 1 || y === ROOM_H - 1
        ? TILE.wall
        : TILE.floor,
    ),
  );

  const spawns: SpawnSpec[] = [];

  for (const dir of neighbors) {
    const { tx, ty } = doorCoords(dir);
    const row = tiles[ty];
    if (row) row[tx] = TILE.door;
    spawns.push({ kind: "door", dir, tx, ty });
  }

  const centerX = Math.floor(ROOM_W / 2);
  const centerY = Math.floor(ROOM_H / 2);

  if (node.type === "normal") {
    const obstacleCount = 2 + Math.floor(RNG.getUniform() * 4);
    for (let i = 0; i < obstacleCount; i++) {
      const tx = 2 + Math.floor(RNG.getUniform() * (ROOM_W - 4));
      const ty = 2 + Math.floor(RNG.getUniform() * (ROOM_H - 4));
      if (tx === centerX && ty === centerY) continue;
      const row = tiles[ty];
      if (row) row[tx] = TILE.obstacle;
    }

    const enemyCount = 3 + Math.floor(RNG.getUniform() * 3);
    const occupied = new Set<string>([`${centerX},${centerY}`]);

    for (let i = 0; i < enemyCount; i++) {
      const enemy =
        ENEMY_POOL[Math.floor(RNG.getUniform() * ENEMY_POOL.length)] ??
        "chaser";
      let placed = false;
      for (let attempt = 0; attempt < 20; attempt++) {
        const tx = 2 + Math.floor(RNG.getUniform() * (ROOM_W - 4));
        const ty = 2 + Math.floor(RNG.getUniform() * (ROOM_H - 4));
        const key = `${tx},${ty}`;
        if (tiles[ty]?.[tx] === TILE.floor && !occupied.has(key)) {
          occupied.add(key);
          spawns.push({ kind: "enemy", enemy, tx, ty });
          placed = true;
          break;
        }
      }
      if (!placed) {
        spawns.push({ kind: "enemy", enemy, tx: 2 + i, ty: 2 });
      }
    }
  } else if (node.type === "item") {
    const itemId =
      ITEM_IDS[Math.floor(RNG.getUniform() * ITEM_IDS.length)] ?? "damage_up";
    spawns.push({ kind: "item", itemId, tx: centerX, ty: centerY });
  } else if (node.type === "shop") {
    const count = 2 + Math.floor(RNG.getUniform() * 2);
    const pool = [...ITEM_IDS].sort(() => RNG.getUniform() - 0.5);
    const startX = centerX - Math.floor(count / 2) * 2;
    for (let i = 0; i < count; i++) {
      const itemId = pool[i] ?? "damage_up";
      const price = 5 + Math.floor(RNG.getUniform() * 6);
      spawns.push({
        kind: "item",
        itemId,
        tx: startX + i * 2,
        ty: centerY,
        price,
      });
    }
  } else if (node.type === "boss") {
    spawns.push({
      kind: "enemy",
      enemy: "boss",
      tx: centerX,
      ty: centerY,
    });
  }

  return { tiles, spawns };
}
