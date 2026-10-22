import { describe, expect, it } from "vitest";
import { createRun } from "../src/game/run";
import type { PickupEntity, PlayerEntity, RunState } from "../src/game/types";
import { updatePickups } from "../src/systems/pickups";

function getPlayer(state: RunState): PlayerEntity {
  const player = state.entities.find((e) => e.kind === "player");
  if (player?.kind !== "player") throw new Error("sem jogador");
  return player;
}

describe("updatePickups", () => {
  it("coleta moedas e soma amount", () => {
    const state = createRun(42);
    const player = getPlayer(state);
    const coin: PickupEntity = {
      id: "coin-1",
      kind: "pickup",
      pickup: "coin",
      amount: 3,
      x: player.x,
      y: player.y,
      vx: 0,
      vy: 0,
      radius: 8,
    };
    state.entities.push(coin);

    updatePickups(state);

    expect(state.coins).toBe(3);
    expect(state.entities.some((e) => e.id === "coin-1")).toBe(false);
  });

  it("coleta item gratuito, aplica atributos e adiciona ao inventário", () => {
    const state = createRun(42);
    const player = getPlayer(state);
    const baseDamage = state.playerStats.damage;
    const item: PickupEntity = {
      id: "item-1",
      kind: "pickup",
      pickup: "item",
      itemId: "damage_up",
      x: player.x,
      y: player.y,
      vx: 0,
      vy: 0,
      radius: 10,
    };
    state.entities.push(item);

    updatePickups(state);

    expect(state.playerStats.damage).toBe(baseDamage + 1);
    expect(state.inventory).toEqual(["damage_up"]);
    expect(state.entities.some((e) => e.id === "item-1")).toBe(false);
  });

  it("compra item da loja apenas se tiver moedas suficientes", () => {
    const state = createRun(42);
    const player = getPlayer(state);
    const shopItem: PickupEntity = {
      id: "shop-1",
      kind: "pickup",
      pickup: "item",
      itemId: "hp_up",
      price: 6,
      x: player.x,
      y: player.y,
      vx: 0,
      vy: 0,
      radius: 10,
    };
    state.entities.push(shopItem);
    state.coins = 5;

    updatePickups(state);

    expect(state.coins).toBe(5);
    expect(state.inventory).toEqual([]);
    expect(state.entities.some((e) => e.id === "shop-1")).toBe(true);

    state.coins = 8;
    updatePickups(state);

    expect(state.coins).toBe(2);
    expect(state.inventory).toEqual(["hp_up"]);
    expect(state.playerStats.maxHp).toBe(8);
    expect(player.hp).toBe(8);
    expect(state.entities.some((e) => e.id === "shop-1")).toBe(false);
  });

  it("ignora o portal para deixar com updateProgression", () => {
    const state = createRun(42);
    const player = getPlayer(state);
    const portal: PickupEntity = {
      id: "portal",
      kind: "pickup",
      pickup: "portal",
      x: player.x,
      y: player.y,
      vx: 0,
      vy: 0,
      radius: 14,
    };
    state.entities.push(portal);

    updatePickups(state);

    expect(state.entities.some((e) => e.id === "portal")).toBe(true);
  });
});
