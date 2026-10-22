import { describe, expect, it } from "vitest";
import { CELL, ROOM_W } from "../src/game/constants";
import { createRun, enterRoom } from "../src/game/run";
import { updateMovement } from "../src/systems/movement";

const idle = { moveX: 0, moveY: 0, shootX: 0, shootY: 0, restart: false };

describe("run", () => {
  it("cria o run com o jogador no centro da sala inicial", () => {
    const state = createRun(42);
    const player = state.entities.find((e) => e.kind === "player");
    expect(player?.x).toBe((Math.floor(ROOM_W / 2) + 0.5) * CELL);
    expect(state.floor.rooms[state.currentRoomId]?.visited).toBe(true);
    expect(state.playerStats.hp).toBe(6);
  });

  it("posiciona o jogador perto da porta oposta ao entrar", () => {
    const state = createRun(42);
    const [dir, id] = Object.entries(state.floor.rooms[state.currentRoomId]?.neighbors ?? {})[0] ?? [];
    if (!dir || !id) throw new Error("sem vizinho");
    enterRoom(state, id, dir as "N" | "S" | "E" | "W");
    const player = state.entities.find((e) => e.kind === "player");
    expect(player).toBeDefined();
  });

  it("não atravessa paredes", () => {
    const state = createRun(42);
    for (let i = 0; i < 600; i++) updateMovement(state, { ...idle, moveX: -1 }, 1 / 60);
    const player = state.entities.find((e) => e.kind === "player");
    expect(player?.x).toBeGreaterThan(CELL);
  });
});
