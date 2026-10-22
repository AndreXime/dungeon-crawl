import { describe, expect, it } from "vitest";
import { IFRAME_MS } from "../src/game/constants";
import { createRun } from "../src/game/run";
import type { EnemyEntity, PlayerEntity, RunState } from "../src/game/types";
import { spawnEnemyProjectile, updateCombat } from "../src/systems/combat";

const idle = { moveX: 0, moveY: 0, shootX: 0, shootY: 0, restart: false };
const shootRight = { ...idle, shootX: 1 };
const DT = 1 / 60;

function setup(): { state: RunState; player: PlayerEntity } {
  const state = createRun(42);
  state.entities = state.entities.filter((e) => e.kind === "player");
  const player = state.entities[0] as PlayerEntity;
  return { state, player };
}

function addEnemy(state: RunState, x: number, y: number, hp = 1): EnemyEntity {
  const enemy: EnemyEntity = {
    kind: "enemy",
    id: "e1",
    enemy: "chaser",
    x,
    y,
    vx: 0,
    vy: 0,
    radius: 10,
    hp,
    cooldown: 0,
  };
  state.entities.push(enemy);
  return enemy;
}

const projectiles = (state: RunState) =>
  state.entities.filter((e) => e.kind === "projectile");

describe("combat", () => {
  it("dispara e respeita o cooldown", () => {
    const { state } = setup();
    updateCombat(state, shootRight, DT);
    updateCombat(state, shootRight, DT);
    expect(projectiles(state)).toHaveLength(1);
    expect(state.shootCooldown).toBeGreaterThan(0);
  });

  it("tripleShot cria 3 projéteis", () => {
    const { state } = setup();
    state.playerStats.tripleShot = true;
    updateCombat(state, shootRight, DT);
    expect(projectiles(state)).toHaveLength(3);
  });

  it("projétil some ao bater na parede", () => {
    const { state, player } = setup();
    player.x = 40;
    updateCombat(state, { ...idle, shootX: -1 }, DT);
    for (let i = 0; i < 30; i++) updateCombat(state, idle, DT);
    expect(projectiles(state)).toHaveLength(0);
  });

  it("mata inimigo e dropa moeda com coinMultiplier", () => {
    const { state, player } = setup();
    state.playerStats.coinMultiplier = 2;
    addEnemy(state, player.x + 60, player.y);
    for (let i = 0; i < 30; i++) updateCombat(state, i === 0 ? shootRight : idle, DT, () => 0);
    expect(state.entities.some((e) => e.kind === "enemy")).toBe(false);
    const coin = state.entities.find((e) => e.kind === "pickup");
    expect(coin?.kind === "pickup" && coin.amount).toBe(2);
  });

  it("projétil inimigo causa dano uma vez por janela de i-frames", () => {
    const { state, player } = setup();
    spawnEnemyProjectile(state, { x: player.x + 30, y: player.y, enemy: "shooter" }, player.x, player.y, 1);
    for (let i = 0; i < 20; i++) updateCombat(state, idle, DT);
    expect(state.playerStats.hp).toBe(5);
    expect(player.hp).toBe(5);

    spawnEnemyProjectile(state, { x: player.x + 30, y: player.y, enemy: "shooter" }, player.x, player.y, 1);
    for (let i = 0; i < 10; i++) updateCombat(state, idle, DT);
    expect(state.playerStats.hp).toBe(5);

    state.timeMs += IFRAME_MS;
    spawnEnemyProjectile(state, { x: player.x + 30, y: player.y, enemy: "shooter" }, player.x, player.y, 1);
    for (let i = 0; i < 20; i++) updateCombat(state, idle, DT);
    expect(state.playerStats.hp).toBe(4);
  });

  it("contato com inimigo causa dano e hp 0 vira dead", () => {
    const { state, player } = setup();
    state.playerStats.hp = 1;
    addEnemy(state, player.x, player.y, 5);
    updateCombat(state, idle, DT);
    expect(state.status).toBe("dead");
  });
});
