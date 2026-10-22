import { describe, expect, it } from "vitest";
import { CELL, ROOM_H, ROOM_W } from "../src/game/constants";
import { createRun, enterRoom } from "../src/game/run";
import type { Dir, EnemyEntity, PlayerEntity, RunState } from "../src/game/types";
import { isRoomCleared, updateDoors } from "../src/systems/doors";
import { updateProgression } from "../src/systems/progression";

const enemy = (hp: number): EnemyEntity => ({
  id: "1",
  kind: "enemy",
  enemy: "chaser",
  x: 0,
  y: 0,
  vx: 0,
  vy: 0,
  radius: 8,
  hp,
  cooldown: 0,
});

const playerOf = (state: RunState): PlayerEntity => {
  const player = state.entities.find((e) => e.kind === "player");
  if (player?.kind !== "player") throw new Error("sem jogador");
  return player;
};

function firstNormalRoom(state: RunState): { dir: Dir; id: string } {
  const start = state.floor.rooms[state.floor.startRoomId];
  for (const [dir, id] of Object.entries(start?.neighbors ?? {})) {
    if (id && state.floor.rooms[id]?.type === "normal") return { dir: dir as Dir, id };
  }
  throw new Error("sem sala normal vizinha");
}

function moveOnto(state: RunState, x: number, y: number): void {
  const player = playerOf(state);
  player.x = x;
  player.y = y;
}

describe("isRoomCleared", () => {
  it("true sem inimigos vivos", () => {
    expect(isRoomCleared([])).toBe(true);
    expect(isRoomCleared([enemy(0)])).toBe(true);
    expect(isRoomCleared([enemy(1)])).toBe(false);
  });
});

describe("updateDoors", () => {
  it("portas começam trancadas e liberam ao limpar a sala", () => {
    const state = createRun(42);
    const target = firstNormalRoom(state);
    enterRoom(state, target.id, target.dir);
    const doors = state.entities.filter((e) => e.kind === "door");
    expect(doors.length).toBeGreaterThan(0);
    expect(doors.every((d) => d.kind === "door" && d.locked)).toBe(true);

    updateDoors(state);
    expect(state.floor.rooms[target.id]?.cleared).toBe(false);

    state.entities = state.entities.filter((e) => e.kind !== "enemy");
    updateDoors(state);
    expect(state.floor.rooms[target.id]?.cleared).toBe(true);
    expect(
      state.entities.filter((e) => e.kind === "door").every((d) => d.kind === "door" && !d.locked),
    ).toBe(true);
  });

  it("coleta moedas soltas ao limpar a sala", () => {
    const state = createRun(42);
    const target = firstNormalRoom(state);
    enterRoom(state, target.id, target.dir);
    state.entities = state.entities.filter((e) => e.kind !== "enemy");
    state.entities.push({
      id: "coin-x",
      kind: "pickup",
      pickup: "coin",
      amount: 2,
      x: 0,
      y: 0,
      vx: 0,
      vy: 0,
      radius: 8,
    });
    updateDoors(state);
    expect(state.coins).toBe(2);
    expect(state.entities.some((e) => e.id === "coin-x")).toBe(false);
  });

  it("não atravessa porta trancada", () => {
    const state = createRun(42);
    const target = firstNormalRoom(state);
    enterRoom(state, target.id, target.dir);
    const door = state.entities.find((e) => e.kind === "door");
    if (!door) throw new Error("sem porta");
    moveOnto(state, door.x, door.y);
    updateDoors(state);
    expect(state.currentRoomId).toBe(target.id);
  });

  it("entra na sala vizinha ao tocar porta destrancada", () => {
    const state = createRun(42);
    const start = state.floor.rooms[state.currentRoomId];
    const door = state.entities.find((e) => e.kind === "door");
    if (door?.kind !== "door" || !start) throw new Error("sem porta");
    moveOnto(state, door.x, door.y);
    updateDoors(state);
    expect(state.currentRoomId).toBe(start.neighbors[door.dir]);
    expect(state.floor.rooms[state.currentRoomId]?.visited).toBe(true);
  });
});

describe("updateProgression", () => {
  function inBossRoom(depth: 1 | 2): RunState {
    const state = createRun(42);
    if (depth === 2) {
      state.floor = { ...state.floor, depth: 2 };
    }
    enterRoom(state, state.floor.bossRoomId, "N");
    state.entities = state.entities.filter((e) => e.kind !== "enemy");
    return state;
  }

  it("avança para o andar 2 ao tocar o portal do boss", () => {
    const state = inBossRoom(1);
    updateProgression(state);
    const portal = state.entities.find((e) => e.id === "portal");
    expect(portal).toBeDefined();
    if (!portal) return;
    moveOnto(state, portal.x, portal.y);
    updateProgression(state);
    expect(state.floor.depth).toBe(1);
    state.timeMs += 600;
    updateProgression(state);
    expect(state.floor.depth).toBe(2);
    expect(state.currentRoomId).toBe(state.floor.startRoomId);
    expect(state.status).toBe("playing");
  });

  it("não avança se o jogador está no centro quando o boss morre", () => {
    const state = inBossRoom(1);
    const center = {
      x: (Math.floor(ROOM_W / 2) + 0.5) * CELL,
      y: (Math.floor(ROOM_H / 2) + 0.5) * CELL,
    };
    moveOnto(state, center.x, center.y);
    updateProgression(state);
    state.timeMs += 600;
    updateProgression(state);
    expect(state.floor.depth).toBe(1);
  });

  it("vence ao limpar o boss do andar 2", () => {
    const state = inBossRoom(2);
    updateProgression(state);
    expect(state.status).toBe("won");
  });

  it("não gera portal com o boss vivo", () => {
    const state = createRun(42);
    enterRoom(state, state.floor.bossRoomId);
    updateProgression(state);
    expect(state.entities.some((e) => e.id === "portal")).toBe(false);
    expect(state.status).toBe("playing");
  });
});
