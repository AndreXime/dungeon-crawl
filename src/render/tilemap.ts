import { Display } from "rot-js";
import { ROOM_W, ROOM_H, TILE } from "../game/constants";
import type { RunState } from "../game/types";

export interface TilemapRenderer {
  draw(state: RunState): void;
  cellSize(): number;
}

export function createTilemapRenderer(parent: HTMLElement): TilemapRenderer {
  const display = new Display({
    width: ROOM_W,
    height: ROOM_H,
    fontSize: 20,
    bg: "#1a1a2e",
    forceSquareRatio: true,
  });
  const container = display.getContainer();
  if (container) parent.appendChild(container);

  return {
    draw(state) {
      for (let y = 0; y < ROOM_H; y++) {
        for (let x = 0; x < ROOM_W; x++) {
          const tile = state.room.tiles[y]?.[x];
          if (tile === TILE.wall) display.draw(x, y, "#", "#555", "#111");
          else if (tile === TILE.door) display.draw(x, y, "+", "#c9a227", "#222");
          else if (tile === TILE.obstacle) display.draw(x, y, "*", "#666", "#1a1a2e");
          else display.draw(x, y, ".", "#333", "#1a1a2e");
        }
      }
    },
    cellSize() {
      return (container?.clientWidth ?? 0) / ROOM_W;
    },
  };
}
