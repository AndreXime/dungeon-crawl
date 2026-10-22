import { CELL, ROOM_H, ROOM_W } from "../game/constants";
import { enterRoom } from "../game/run";
import type { PlayerEntity, RunState } from "../game/types";
import { generateFloor } from "../world/floorGen";
import { circlesOverlap } from "./collision";
import { isRoomCleared } from "./doors";

const PORTAL_ID = "portal";
const PORTAL_RADIUS = 14;

export function advanceFloor(state: RunState): void {
  state.floor = generateFloor(state.seed, 2);
  enterRoom(state, state.floor.startRoomId);
}

export function updateProgression(state: RunState): void {
  if (state.status !== "playing") return;
  const node = state.floor.rooms[state.currentRoomId];
  if (!node || node.type !== "boss" || !isRoomCleared(state.entities)) return;

  if (state.floor.depth === 2) {
    state.status = "won";
    return;
  }

  let portal = state.entities.find((e) => e.id === PORTAL_ID);
  if (!portal) {
    portal = {
      kind: "pickup",
      id: PORTAL_ID,
      pickup: "portal",
      x: (Math.floor(ROOM_W / 2) + 0.5) * CELL,
      y: (Math.floor(ROOM_H / 2) + 0.5) * CELL,
      vx: 0,
      vy: 0,
      radius: PORTAL_RADIUS,
    };
    state.entities.push(portal);
  }

  const player = state.entities.find(
    (e): e is PlayerEntity => e.kind === "player",
  );
  if (
    player &&
    circlesOverlap(player.x, player.y, player.radius, portal.x, portal.y, portal.radius)
  ) {
    advanceFloor(state);
  }
}
