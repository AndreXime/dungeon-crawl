import { ENEMIES } from "../data/enemies";
import { IFRAME_MS, PROJECTILE_RADIUS } from "../game/constants";
import type { InputState } from "../game/input";
import type {
  EnemyEntity,
  Entity,
  PickupEntity,
  PlayerEntity,
  ProjectileEntity,
  RunState,
} from "../game/types";
import { circleHitsSolid, circlesOverlap } from "./collision";

export const PLAYER_PROJECTILE_SPEED = 300;
export const ENEMY_PROJECTILE_RANGE = 400;
export const COIN_DROP_CHANCE = 0.5;
const COIN_RADIUS = 8;
const TRIPLE_SHOT_SPREAD = (15 * Math.PI) / 180;

let projectileCounter = 0;

function nextProjectileId(owner: ProjectileEntity["owner"]): string {
  projectileCounter += 1;
  return `proj-${owner}-${projectileCounter}`;
}

function findPlayer(state: RunState): PlayerEntity | undefined {
  return state.entities.find((e): e is PlayerEntity => e.kind === "player");
}

function spawnPlayerShots(
  state: RunState,
  player: PlayerEntity,
  dirX: number,
  dirY: number,
): void {
  const { damage, range, tripleShot } = state.playerStats;
  const baseAngle = Math.atan2(dirY, dirX);
  const offsets = tripleShot
    ? [-TRIPLE_SHOT_SPREAD, 0, TRIPLE_SHOT_SPREAD]
    : [0];

  for (const offset of offsets) {
    const angle = baseAngle + offset;
    state.entities.push({
      kind: "projectile",
      id: nextProjectileId("player"),
      owner: "player",
      x: player.x,
      y: player.y,
      vx: Math.cos(angle) * PLAYER_PROJECTILE_SPEED,
      vy: Math.sin(angle) * PLAYER_PROJECTILE_SPEED,
      radius: PROJECTILE_RADIUS,
      damage,
      life: range / PLAYER_PROJECTILE_SPEED,
    });
  }
}

/** Disparo inimigo em direção a (towardX, towardY). Usado pela IA. */
export function spawnEnemyProjectile(
  state: RunState,
  from: { x: number; y: number; enemy?: EnemyEntity["enemy"] },
  towardX: number,
  towardY: number,
  damage: number,
): void {
  const dx = towardX - from.x;
  const dy = towardY - from.y;
  const length = Math.hypot(dx, dy);
  if (length === 0) return;

  const speed = ENEMIES[from.enemy ?? "shooter"].projectileSpeed;
  if (speed <= 0) return;

  state.entities.push({
    kind: "projectile",
    id: nextProjectileId("enemy"),
    owner: "enemy",
    x: from.x,
    y: from.y,
    vx: (dx / length) * speed,
    vy: (dy / length) * speed,
    radius: PROJECTILE_RADIUS,
    damage,
    life: ENEMY_PROJECTILE_RANGE / speed,
  });
}

function updatePlayerShooting(
  state: RunState,
  player: PlayerEntity,
  input: InputState,
  dt: number,
): void {
  state.shootCooldown = Math.max(0, state.shootCooldown - dt);
  if (state.shootCooldown > 0) return;
  if (input.shootX === 0 && input.shootY === 0) return;

  spawnPlayerShots(state, player, input.shootX, input.shootY);
  state.shootCooldown = 1 / state.playerStats.fireRate;
}

function damagePlayer(state: RunState, amount: number): void {
  if (state.timeMs < state.playerStats.iframeUntil) return;
  state.playerStats.hp = Math.max(0, state.playerStats.hp - amount);
  state.playerStats.iframeUntil = state.timeMs + IFRAME_MS;
}

function dropCoin(
  state: RunState,
  enemy: EnemyEntity,
  rng: () => number,
): void {
  if (rng() >= COIN_DROP_CHANCE) return;
  const coin: PickupEntity = {
    kind: "pickup",
    id: `coin-${enemy.id}-${Math.floor(state.timeMs)}`,
    pickup: "coin",
    amount: state.playerStats.coinMultiplier,
    x: enemy.x,
    y: enemy.y,
    vx: 0,
    vy: 0,
    radius: COIN_RADIUS,
  };
  state.entities.push(coin);
}

function updateProjectiles(
  state: RunState,
  player: PlayerEntity,
  dt: number,
  rng: () => number,
): void {
  const removed = new Set<Entity>();
  const projectiles = state.entities.filter(
    (e): e is ProjectileEntity => e.kind === "projectile",
  );
  const enemies = state.entities.filter(
    (e): e is EnemyEntity => e.kind === "enemy" && e.hp > 0,
  );

  for (const projectile of projectiles) {
    projectile.x += projectile.vx * dt;
    projectile.y += projectile.vy * dt;
    projectile.life -= dt;

    if (
      projectile.life <= 0 ||
      circleHitsSolid(
        projectile.x,
        projectile.y,
        projectile.radius,
        state.room.tiles,
      )
    ) {
      removed.add(projectile);
      continue;
    }

    if (projectile.owner === "player") {
      const target = enemies.find(
        (enemy) =>
          enemy.hp > 0 &&
          circlesOverlap(
            projectile.x,
            projectile.y,
            projectile.radius,
            enemy.x,
            enemy.y,
            enemy.radius,
          ),
      );
      if (!target) continue;
      target.hp -= projectile.damage;
      removed.add(projectile);
      if (target.hp <= 0) {
        removed.add(target);
        dropCoin(state, target, rng);
      }
    } else if (
      circlesOverlap(
        projectile.x,
        projectile.y,
        projectile.radius,
        player.x,
        player.y,
        player.radius,
      )
    ) {
      damagePlayer(state, projectile.damage);
      removed.add(projectile);
    }
  }

  if (removed.size > 0) {
    state.entities = state.entities.filter((e) => !removed.has(e));
  }
}

function updateContactDamage(state: RunState, player: PlayerEntity): void {
  for (const entity of state.entities) {
    if (entity.kind !== "enemy" || entity.hp <= 0) continue;
    if (
      circlesOverlap(
        player.x,
        player.y,
        player.radius,
        entity.x,
        entity.y,
        entity.radius,
      )
    ) {
      damagePlayer(state, ENEMIES[entity.enemy].damage);
    }
  }
}

/** `dt` em segundos. Avança `state.timeMs`, base dos i-frames. */
export function updateCombat(
  state: RunState,
  input: InputState,
  dt: number,
  rng: () => number = Math.random,
): void {
  if (state.status !== "playing") return;
  const player = findPlayer(state);
  if (!player) return;

  state.timeMs += dt * 1000;

  updatePlayerShooting(state, player, input, dt);
  updateProjectiles(state, player, dt, rng);
  updateContactDamage(state, player);

  player.hp = state.playerStats.hp;
  if (state.playerStats.hp <= 0) state.status = "dead";
}
