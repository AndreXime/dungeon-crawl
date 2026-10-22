import { describe, it, expect } from "vitest";
import { applyItem, ITEMS } from "../src/data/items";
import type { PlayerStats } from "../src/game/types";

const base = (): PlayerStats => ({
  maxHp: 6,
  hp: 4,
  speed: 120,
  damage: 1,
  fireRate: 4,
  range: 320,
  tripleShot: false,
  coinMultiplier: 1,
  iframeUntil: 0,
});

describe("applyItem", () => {
  it("aplica boost de dano", () => {
    const s = applyItem(base(), "damage_up");
    expect(s.damage).toBe(base().damage + (ITEMS.damage_up.damage ?? 0));
  });

  it("aplica todos os itens sem mutar o estado original", () => {
    const original = base();
    const sSpeed = applyItem(original, "speed_up");
    expect(sSpeed.speed).toBe(150);
    expect(original.speed).toBe(120);

    const sHp = applyItem(original, "hp_up");
    expect(sHp.maxHp).toBe(8);
    expect(sHp.hp).toBe(6);

    const sFireRate = applyItem(original, "fire_rate");
    expect(sFireRate.fireRate).toBe(5.5);

    const sTriple = applyItem(original, "triple");
    expect(sTriple.tripleShot).toBe(true);

    const sGreedy = applyItem(original, "greedy");
    expect(sGreedy.coinMultiplier).toBe(2);
  });
});
