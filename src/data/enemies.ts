import type { EnemyKind } from "../game/types";

export interface EnemyDef {
  hp: number;
  speed: number;
  damage: number;
  radius: number;
  fireCooldownMs: number;
  projectileSpeed: number;
  preferredDist?: number;
  chargeSpeed?: number;
  svg: string;
}

export const ENEMIES: Record<EnemyKind, EnemyDef> = {
  chaser: {
    hp: 3,
    speed: 70,
    damage: 1,
    radius: 10,
    fireCooldownMs: 0,
    projectileSpeed: 0,
    svg: "enemy-chaser.svg",
  },
  shooter: {
    hp: 2,
    speed: 40,
    damage: 1,
    radius: 10,
    fireCooldownMs: 1500,
    projectileSpeed: 140,
    preferredDist: 140,
    svg: "enemy-shooter.svg",
  },
  wander: {
    hp: 2,
    speed: 50,
    damage: 1,
    radius: 9,
    fireCooldownMs: 0,
    projectileSpeed: 0,
    svg: "enemy-wander.svg",
  },
  boss: {
    hp: 30,
    speed: 45,
    damage: 2,
    radius: 22,
    fireCooldownMs: 1200,
    projectileSpeed: 160,
    chargeSpeed: 110,
    svg: "enemy-boss.svg",
  },
};
