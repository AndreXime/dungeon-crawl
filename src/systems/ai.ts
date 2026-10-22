import { ENEMIES } from "../data/enemies";
import type { EnemyEntity, PlayerEntity, RunState } from "../game/types";
import { circleHitsSolid } from "./collision";
import { spawnEnemyProjectile } from "./combat";

export const WANDER_MIN_MS = 500;
export const WANDER_MAX_MS = 1500;
export const BOSS_CHARGE_S = 2;
export const BOSS_BURST_S = 1;
const SHOOTER_APPROACH_FACTOR = 0.3;
const SHOOTER_DEADZONE = 12;
const BOSS_BURST_DIRECTIONS = 8;

function moveWithTiles(
  state: RunState,
  enemy: EnemyEntity,
  dx: number,
  dy: number,
): void {
  const { tiles } = state.room;
  const nextX = enemy.x + dx;
  const nextY = enemy.y + dy;

  if (!circleHitsSolid(nextX, nextY, enemy.radius, tiles)) {
    enemy.x = nextX;
    enemy.y = nextY;
    return;
  }
  if (!circleHitsSolid(nextX, enemy.y, enemy.radius, tiles)) enemy.x = nextX;
  else enemy.vx = 0;
  if (!circleHitsSolid(enemy.x, nextY, enemy.radius, tiles)) enemy.y = nextY;
  else enemy.vy = 0;
}

function steer(
  state: RunState,
  enemy: EnemyEntity,
  dirX: number,
  dirY: number,
  speed: number,
  dt: number,
): void {
  enemy.vx = dirX * speed;
  enemy.vy = dirY * speed;
  moveWithTiles(state, enemy, enemy.vx * dt, enemy.vy * dt);
}

function updateChaser(
  state: RunState,
  enemy: EnemyEntity,
  toPlayer: { dirX: number; dirY: number },
  dt: number,
): void {
  steer(state, enemy, toPlayer.dirX, toPlayer.dirY, ENEMIES.chaser.speed, dt);
}

function updateWander(
  state: RunState,
  enemy: EnemyEntity,
  dt: number,
  rng: () => number,
): void {
  const def = ENEMIES.wander;
  enemy.cooldown -= dt * 1000;
  if (enemy.cooldown <= 0) {
    const angle = rng() * Math.PI * 2;
    enemy.vx = Math.cos(angle) * def.speed;
    enemy.vy = Math.sin(angle) * def.speed;
    enemy.cooldown = WANDER_MIN_MS + rng() * (WANDER_MAX_MS - WANDER_MIN_MS);
  }
  const x = enemy.x;
  const y = enemy.y;
  moveWithTiles(state, enemy, enemy.vx * dt, enemy.vy * dt);
  // Bateu na parede: sorteia nova direção no próximo tick.
  if (enemy.x === x && enemy.y === y) enemy.cooldown = 0;
}

function updateShooter(
  state: RunState,
  enemy: EnemyEntity,
  player: PlayerEntity,
  toPlayer: { dirX: number; dirY: number; dist: number },
  dt: number,
): void {
  const def = ENEMIES.shooter;
  const preferred = def.preferredDist ?? 0;
  const { dirX, dirY, dist } = toPlayer;

  if (dist < preferred - SHOOTER_DEADZONE) {
    steer(state, enemy, -dirX, -dirY, def.speed, dt);
  } else if (dist > preferred + SHOOTER_DEADZONE) {
    steer(state, enemy, dirX, dirY, def.speed * SHOOTER_APPROACH_FACTOR, dt);
  } else {
    enemy.vx = 0;
    enemy.vy = 0;
  }

  enemy.cooldown -= dt * 1000;
  if (enemy.cooldown <= 0) {
    spawnEnemyProjectile(state, enemy, player.x, player.y, def.damage);
    enemy.cooldown = def.fireCooldownMs;
  }
}

function fireBossBurst(state: RunState, enemy: EnemyEntity): void {
  const { damage } = ENEMIES.boss;
  for (let i = 0; i < BOSS_BURST_DIRECTIONS; i++) {
    const angle = (i / BOSS_BURST_DIRECTIONS) * Math.PI * 2;
    spawnEnemyProjectile(
      state,
      enemy,
      enemy.x + Math.cos(angle),
      enemy.y + Math.sin(angle),
      damage,
    );
  }
}

/** `phase` é o tempo (s) dentro do ciclo: charge, depois rajada radial. */
function updateBoss(
  state: RunState,
  enemy: EnemyEntity,
  toPlayer: { dirX: number; dirY: number },
  dt: number,
): void {
  const def = ENEMIES.boss;
  const before = enemy.phase ?? 0;
  let phase = before + dt;
  const cycle = BOSS_CHARGE_S + BOSS_BURST_S;
  if (phase >= cycle) phase -= cycle;
  enemy.phase = phase;

  if (before < BOSS_CHARGE_S && phase >= BOSS_CHARGE_S) {
    fireBossBurst(state, enemy);
  }

  if (phase < BOSS_CHARGE_S) {
    steer(
      state,
      enemy,
      toPlayer.dirX,
      toPlayer.dirY,
      def.chargeSpeed ?? def.speed,
      dt,
    );
  } else {
    enemy.vx = 0;
    enemy.vy = 0;
  }
}

/** `dt` em segundos. Dano de contato fica em `updateCombat`. */
export function updateAi(
  state: RunState,
  dt: number,
  rng: () => number = Math.random,
): void {
  if (state.status !== "playing") return;
  const player = state.entities.find(
    (e): e is PlayerEntity => e.kind === "player",
  );
  if (!player) return;

  for (const entity of state.entities) {
    if (entity.kind !== "enemy" || entity.hp <= 0) continue;

    const dx = player.x - entity.x;
    const dy = player.y - entity.y;
    const dist = Math.hypot(dx, dy);
    const toPlayer =
      dist === 0
        ? { dirX: 0, dirY: 0, dist }
        : { dirX: dx / dist, dirY: dy / dist, dist };

    switch (entity.enemy) {
      case "chaser":
        updateChaser(state, entity, toPlayer, dt);
        break;
      case "wander":
        updateWander(state, entity, dt, rng);
        break;
      case "shooter":
        updateShooter(state, entity, player, toPlayer, dt);
        break;
      case "boss":
        updateBoss(state, entity, toPlayer, dt);
        break;
    }
  }
}
