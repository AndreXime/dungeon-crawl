import type { PlayerStats } from "../game/types";

export interface ItemDef {
  id: string;
  name: string;
  damage?: number;
  speed?: number;
  maxHp?: number;
  heal?: number;
  fireRate?: number;
  tripleShot?: boolean;
  coinMultiplier?: number;
  svg: string;
}

export const ITEMS: Record<string, ItemDef> = {
  damage_up: { id: "damage_up", name: "Dente Afiado", damage: 1, svg: "item-damage.svg" },
  speed_up: { id: "speed_up", name: "Botas", speed: 30, svg: "item-speed.svg" },
  hp_up: { id: "hp_up", name: "Coração Extra", maxHp: 2, heal: 2, svg: "item-hp.svg" },
  fire_rate: { id: "fire_rate", name: "Cafeína", fireRate: 1.5, svg: "item-firerate.svg" },
  triple: { id: "triple", name: "Tridente", tripleShot: true, svg: "item-triple.svg" },
  greedy: { id: "greedy", name: "Ímã", coinMultiplier: 1, svg: "item-greedy.svg" },
};

export function applyItem(stats: PlayerStats, itemId: string): PlayerStats {
  const item = ITEMS[itemId];
  const next: PlayerStats = { ...stats };
  if (!item) return next;

  if (item.damage !== undefined) next.damage += item.damage;
  if (item.speed !== undefined) next.speed += item.speed;
  if (item.maxHp !== undefined) next.maxHp += item.maxHp;
  if (item.heal !== undefined) next.hp = Math.min(next.maxHp, next.hp + item.heal);
  if (item.fireRate !== undefined) next.fireRate += item.fireRate;
  if (item.tripleShot !== undefined) next.tripleShot = item.tripleShot;
  if (item.coinMultiplier !== undefined) next.coinMultiplier += item.coinMultiplier;

  return next;
}
