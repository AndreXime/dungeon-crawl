import { enterRoom } from "../game/run";
import type { Entity, PlayerEntity, RunState } from "../game/types";
import { circlesOverlap } from "./collision";

export function isRoomCleared(entities: Entity[]): boolean {
  return !entities.some((e) => e.kind === "enemy" && e.hp > 0);
}

export function updateDoors(state: RunState): void {
  if (state.status !== "playing") return;
  const node = state.floor.rooms[state.currentRoomId];
  if (!node) return;

  if (isRoomCleared(state.entities)) {
    node.cleared = true;
    for (const entity of state.entities) {
      if (entity.kind === "door") entity.locked = false;
    }
  }

  const player = state.entities.find(
    (e): e is PlayerEntity => e.kind === "player",
  );
  if (!player) return;

  for (const entity of state.entities) {
    if (entity.kind !== "door" || entity.locked) continue;
    const neighborId = node.neighbors[entity.dir];
    if (!neighborId) continue;
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
      enterRoom(state, neighborId, entity.dir);
      return;
    }
  }
}
