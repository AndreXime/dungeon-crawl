import { PLAYER_RADIUS } from "../game/constants";
import type { InputState } from "../game/input";
import type { PlayerEntity, RunState } from "../game/types";
import { circleHitsSolid } from "./collision";

/** `dt` em segundos. */
export function updateMovement(
  state: RunState,
  input: InputState,
  dt: number,
): void {
  const player = state.entities.find(
    (e): e is PlayerEntity => e.kind === "player",
  );
  if (!player) return;

  const { speed } = state.playerStats;
  player.vx = input.moveX * speed;
  player.vy = input.moveY * speed;

  const nextX = player.x + player.vx * dt;
  const nextY = player.y + player.vy * dt;
  const { tiles } = state.room;

  if (!circleHitsSolid(nextX, nextY, PLAYER_RADIUS, tiles)) {
    player.x = nextX;
    player.y = nextY;
    return;
  }

  if (!circleHitsSolid(nextX, player.y, PLAYER_RADIUS, tiles)) {
    player.x = nextX;
  } else {
    player.vx = 0;
  }
  if (!circleHitsSolid(player.x, nextY, PLAYER_RADIUS, tiles)) {
    player.y = nextY;
  } else {
    player.vy = 0;
  }
}
