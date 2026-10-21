import { describe, it, expect } from "vitest";
import { generateRoom } from "../src/world/roomGen";
import { ROOM_W, ROOM_H, TILE } from "../src/game/constants";
import type { FloorRoomNode } from "../src/game/types";

const base: FloorRoomNode = {
  id: "0,0",
  type: "normal",
  neighbors: { N: "0,-1", E: "1,0" },
  cleared: false,
  visited: false,
  roomSeed: 99,
};

describe("generateRoom", () => {
  it("cria borda de paredes e portas nos vizinhos", () => {
    const room = generateRoom(base, ["N", "E"]);
    expect(room.tiles.length).toBe(ROOM_H);
    expect(room.tiles[0]?.length).toBe(ROOM_W);
    expect(room.tiles[0]?.[Math.floor(ROOM_W / 2)]).toBe(TILE.door);
    expect(room.tiles[Math.floor(ROOM_H / 2)]?.[ROOM_W - 1]).toBe(TILE.door);
    expect(room.spawns.some((s) => s.kind === "enemy")).toBe(true);
    expect(room.spawns.filter((s) => s.kind === "door").length).toBe(2);
  });

  it("gera spawns corretos por tipo de sala", () => {
    const startRoom = generateRoom({ ...base, type: "start" }, ["S"]);
    expect(startRoom.spawns.some((s) => s.kind === "enemy")).toBe(false);

    const normalRoom = generateRoom({ ...base, type: "normal" }, ["N", "S"]);
    const normalEnemies = normalRoom.spawns.filter((s) => s.kind === "enemy");
    expect(normalEnemies.length).toBeGreaterThanOrEqual(3);
    expect(normalEnemies.length).toBeLessThanOrEqual(5);

    const itemRoom = generateRoom({ ...base, type: "item" }, ["W"]);
    const freeItems = itemRoom.spawns.filter((s) => s.kind === "item");
    expect(freeItems.length).toBe(1);
    expect(freeItems[0]?.price).toBeUndefined();

    const shopRoom = generateRoom({ ...base, type: "shop" }, ["E"]);
    const shopItems = shopRoom.spawns.filter((s) => s.kind === "item");
    expect(shopItems.length).toBeGreaterThanOrEqual(2);
    expect(shopItems.length).toBeLessThanOrEqual(3);
    expect(shopItems.every((s) => typeof s.price === "number" && s.price > 0)).toBe(true);

    const bossRoom = generateRoom({ ...base, type: "boss" }, ["N"]);
    const bosses = bossRoom.spawns.filter(
      (s) => s.kind === "enemy" && s.enemy === "boss",
    );
    expect(bosses.length).toBe(1);
    expect(bosses[0]?.tx).toBe(Math.floor(ROOM_W / 2));
    expect(bosses[0]?.ty).toBe(Math.floor(ROOM_H / 2));
  });

  it("é determinístico para a mesma roomSeed", () => {
    expect(generateRoom(base, ["N", "E"])).toEqual(
      generateRoom(base, ["N", "E"]),
    );
  });
});
