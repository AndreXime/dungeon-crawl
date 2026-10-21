import { describe, it, expect } from "vitest";
import { generateFloor } from "../src/world/floorGen";

describe("generateFloor", () => {
  it("gera 6 a 8 salas conectadas com start, boss e loja", () => {
    const floor = generateFloor(42, 1);
    const rooms = Object.values(floor.rooms);
    expect(rooms.length).toBeGreaterThanOrEqual(6);
    expect(rooms.length).toBeLessThanOrEqual(8);
    expect(floor.rooms[floor.startRoomId]?.type).toBe("start");
    expect(floor.rooms[floor.bossRoomId]?.type).toBe("boss");
    expect(rooms.some((r) => r.type === "shop")).toBe(true);
    expect(rooms.some((r) => r.type === "item")).toBe(true);

    const seen = new Set<string>();
    const stack = [floor.startRoomId];
    while (stack.length) {
      const id = stack.pop();
      if (!id || seen.has(id)) continue;
      seen.add(id);
      const node = floor.rooms[id];
      if (!node) continue;
      for (const n of Object.values(node.neighbors)) {
        if (n) stack.push(n);
      }
    }
    expect(seen.size).toBe(rooms.length);
  });

  it("é determinístico para a mesma seed", () => {
    expect(generateFloor(7, 2)).toEqual(generateFloor(7, 2));
  });
});
