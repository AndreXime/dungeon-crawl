import { describe, it, expect } from "vitest";
import { circlesOverlap, circleHitsSolid, isRoomCleared } from "../src/systems/collision";
import { isRoomCleared as isRoomClearedFromDoors } from "../src/systems/doors";
import { TILE, CELL } from "../src/game/constants";
import type { Entity } from "../src/game/types";

describe("collision", () => {
  it("detecta overlap de círculos", () => {
    expect(circlesOverlap(0, 0, 5, 6, 0, 5)).toBe(true);
    expect(circlesOverlap(0, 0, 5, 20, 0, 5)).toBe(false);
  });

  it("bloqueia círculo contra tile parede", () => {
    const tiles = [
      [TILE.wall, TILE.wall],
      [TILE.wall, TILE.floor],
    ];
    expect(circleHitsSolid(CELL * 1.5, CELL * 1.5, 8, tiles)).toBe(false);
    expect(circleHitsSolid(8, 8, 8, tiles)).toBe(true);
  });

  it("bloqueia círculo contra tile obstáculo ou fora do mapa", () => {
    const tiles = [
      [TILE.floor, TILE.obstacle],
      [TILE.door, TILE.floor],
    ];
    expect(circleHitsSolid(CELL * 1.5, CELL * 0.5, 6, tiles)).toBe(true);
    expect(circleHitsSolid(CELL * 0.5, CELL * 1.5, 6, tiles)).toBe(false);
    expect(circleHitsSolid(-10, -10, 5, tiles)).toBe(true);
  });

  it("verifica se a sala está limpa de inimigos vivos", () => {
    const entitiesWithAliveEnemy: Entity[] = [
      { id: "p1", kind: "player", x: 50, y: 50, vx: 0, vy: 0, radius: 10, hp: 6 },
      { id: "e1", kind: "enemy", enemy: "chaser", x: 100, y: 100, vx: 0, vy: 0, radius: 10, hp: 2, cooldown: 0 },
    ];
    const entitiesCleared: Entity[] = [
      { id: "p1", kind: "player", x: 50, y: 50, vx: 0, vy: 0, radius: 10, hp: 6 },
      { id: "e1", kind: "enemy", enemy: "chaser", x: 100, y: 100, vx: 0, vy: 0, radius: 10, hp: 0, cooldown: 0 },
    ];

    expect(isRoomCleared(entitiesWithAliveEnemy)).toBe(false);
    expect(isRoomCleared(entitiesCleared)).toBe(true);
    expect(isRoomClearedFromDoors(entitiesWithAliveEnemy)).toBe(false);
    expect(isRoomClearedFromDoors(entitiesCleared)).toBe(true);
  });
});
