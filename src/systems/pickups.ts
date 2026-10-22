import { applyItem } from "../data/items";
import type { Entity, PlayerEntity, RunState } from "../game/types";
import { circlesOverlap } from "./collision";

export function updatePickups(state: RunState): void {
  if (state.status !== "playing") return;
  const player = state.entities.find(
    (e): e is PlayerEntity => e.kind === "player",
  );
  if (!player) return;

  const node = state.floor.rooms[state.currentRoomId];
  const removed = new Set<Entity>();

  for (const entity of state.entities) {
    if (entity.kind !== "pickup" || entity.pickup === "portal") continue;
    if (
      !circlesOverlap(
        player.x,
        player.y,
        player.radius,
        entity.x,
        entity.y,
        entity.radius,
      )
    ) {
      continue;
    }

    if (entity.pickup === "coin") {
      state.coins += entity.amount ?? 1;
      removed.add(entity);
      continue;
    }

    if (entity.pickup === "item" && entity.itemId) {
      const price = entity.price ?? 0;
      if (price > 0 && state.coins < price) continue;
      if (price > 0) state.coins -= price;
      state.playerStats = applyItem(state.playerStats, entity.itemId);
      player.hp = state.playerStats.hp;
      state.inventory.push(entity.itemId);
      if (node && entity.spawnIndex !== undefined) {
        node.claimedSpawns.push(entity.spawnIndex);
      }
      removed.add(entity);
    }
  }

  if (removed.size > 0) {
    state.entities = state.entities.filter((e) => !removed.has(e));
  }
}
