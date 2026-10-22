import type { Entity } from "../game/types";

export function isRoomCleared(entities: Entity[]): boolean {
  return !entities.some((e) => e.kind === "enemy" && e.hp > 0);
}
