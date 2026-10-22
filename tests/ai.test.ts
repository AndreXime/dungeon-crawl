import { describe, expect, it } from "vitest";
import { ENEMIES } from "../src/data/enemies";
import { createRun } from "../src/game/run";
import type {
  EnemyEntity,
  EnemyKind,
  PlayerEntity,
  RunState,
} from "../src/game/types";
import { updateAi } from "../src/systems/ai";

const DT = 1 / 60;

function setup(
  kind: EnemyKind,
  enemyPos: { x: number; y: number },
  playerPos = { x: 240, y: 176 },
): { state: RunState; player: PlayerEntity; enemy: EnemyEntity } {
  const state = createRun(42);
  state.entities = state.entities.filter((e) => e.kind === "player");
  const player = state.entities[0] as PlayerEntity;
  player.x = playerPos.x;
  player.y = playerPos.y;
  const def = ENEMIES[kind];
  const enemy: EnemyEntity = {
    kind: "enemy",
    id: "e1",
    enemy: kind,
    ...enemyPos,
    vx: 0,
    vy: 0,
    radius: def.radius,
    hp: def.hp,
    cooldown: 0,
  };
  state.entities.push(enemy);
  return { state, player, enemy };
}

const enemyProjectiles = (state: RunState) =>
  state.entities.filter((e) => e.kind === "projectile" && e.owner === "enemy");

describe("ai", () => {
  it("chaser se aproxima do player", () => {
    const { state, player, enemy } = setup("chaser", { x: 100, y: 176 });
    const before = Math.hypot(player.x - enemy.x, player.y - enemy.y);
    for (let i = 0; i < 30; i++) updateAi(state, DT);
    expect(Math.hypot(player.x - enemy.x, player.y - enemy.y)).toBeLessThan(
      before,
    );
  });

  it("chaser não atravessa parede", () => {
    const { state, enemy } = setup("chaser", { x: 50, y: 100 }, { x: -200, y: 100 });
    for (let i = 0; i < 120; i++) updateAi(state, DT);
    expect(enemy.x).toBeGreaterThanOrEqual(32 + enemy.radius - 1);
  });

  it("wander sorteia direção e se move", () => {
    const { state, enemy } = setup("wander", { x: 240, y: 176 });
    updateAi(state, DT, () => 0.25);
    expect(enemy.vx).toBeCloseTo(0, 5);
    expect(enemy.vy).toBeGreaterThan(0);
    expect(enemy.cooldown).toBeGreaterThanOrEqual(500);
    expect(enemy.cooldown).toBeLessThanOrEqual(1500);
  });

  it("shooter atira no cooldown e mantém distância", () => {
    const { state, enemy } = setup("shooter", { x: 200, y: 176 }, { x: 240, y: 176 });
    updateAi(state, DT);
    expect(enemyProjectiles(state)).toHaveLength(1);
    expect(enemy.cooldown).toBeGreaterThan(1000);
    updateAi(state, DT);
    expect(enemyProjectiles(state)).toHaveLength(1);
    expect(enemy.x).toBeLessThan(200);
  });

  it("boss alterna charge e rajada de 8 direções", () => {
    const { state, enemy } = setup("boss", { x: 100, y: 176 });
    enemy.phase = 0;
    for (let i = 0; i < 119; i++) updateAi(state, DT);
    expect(enemyProjectiles(state)).toHaveLength(0);
    expect(enemy.x).toBeGreaterThan(100);
    for (let i = 0; i < 3; i++) updateAi(state, DT);
    expect(enemyProjectiles(state)).toHaveLength(8);
    const x = enemy.x;
    updateAi(state, DT);
    expect(enemy.x).toBe(x);
  });
});
