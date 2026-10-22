import { RNG } from "rot-js";
import type { Dir, Floor, FloorRoomNode } from "../game/types";

const OPP: Record<Dir, Dir> = { N: "S", S: "N", E: "W", W: "E" };
const DELTA: Record<Dir, [number, number]> = {
  N: [0, -1],
  S: [0, 1],
  E: [1, 0],
  W: [-1, 0],
};

function key(x: number, y: number): string {
  return `${x},${y}`;
}

export function generateFloor(seed: number, depth: 1 | 2): Floor {
  RNG.setSeed(seed + depth * 10007);
  const target = 6 + Math.floor(RNG.getUniform() * 3);
  const positions = new Map<string, { x: number; y: number }>();
  positions.set(key(0, 0), { x: 0, y: 0 });
  const frontier = [{ x: 0, y: 0 }];

  while (positions.size < target && frontier.length > 0) {
    const i = Math.floor(RNG.getUniform() * frontier.length);
    const cur = frontier[i];
    if (!cur) break;
    const dirs = (["N", "S", "E", "W"] as Dir[]).sort(
      () => RNG.getUniform() - 0.5,
    );
    let grew = false;
    for (const d of dirs) {
      const [dx, dy] = DELTA[d];
      const nx = cur.x + dx;
      const ny = cur.y + dy;
      const k = key(nx, ny);
      if (positions.has(k)) continue;
      positions.set(k, { x: nx, y: ny });
      frontier.push({ x: nx, y: ny });
      grew = true;
      break;
    }
    if (!grew) frontier.splice(i, 1);
  }

  const rooms: Record<string, FloorRoomNode> = {};
  for (const [id, pos] of positions) {
    rooms[id] = {
      id,
      type: "normal",
      neighbors: {},
      cleared: false,
      claimedSpawns: [],
      visited: false,
      roomSeed: (seed ^ (pos.x * 73856093) ^ (pos.y * 19349663) ^ depth) >>> 0,
    };
  }

  for (const [id, pos] of positions) {
    const room = rooms[id];
    if (!room) continue;
    for (const d of ["N", "S", "E", "W"] as Dir[]) {
      const [dx, dy] = DELTA[d];
      const nid = key(pos.x + dx, pos.y + dy);
      const neighborRoom = rooms[nid];
      if (!neighborRoom) continue;
      room.neighbors[d] = nid;
      neighborRoom.neighbors[OPP[d]] = id;
    }
  }

  const startRoomId = key(0, 0);
  let bossRoomId = startRoomId;
  let best = -1;
  for (const [id, pos] of positions) {
    const dist = Math.abs(pos.x) + Math.abs(pos.y);
    if (dist > best) {
      best = dist;
      bossRoomId = id;
    }
  }
  const startRoom = rooms[startRoomId];
  if (startRoom) {
    startRoom.type = "start";
    startRoom.cleared = true;
  }
  const bossRoom = rooms[bossRoomId];
  if (bossRoom) {
    bossRoom.type = "boss";
  }

  const others = Object.keys(rooms).filter(
    (id) => id !== startRoomId && id !== bossRoomId,
  );
  const shuffle = [...others].sort(() => RNG.getUniform() - 0.5);
  const shopId = shuffle[0];
  if (shopId && rooms[shopId]) rooms[shopId].type = "shop";
  const itemId = shuffle[1];
  if (itemId && rooms[itemId]) rooms[itemId].type = "item";

  return { depth, rooms, startRoomId, bossRoomId };
}
