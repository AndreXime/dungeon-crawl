import { CELL, ROOM_H, ROOM_W } from "../game/constants";
import { ITEMS } from "../data/items";
import type { Entity, RunState } from "../game/types";

const svgUrls = import.meta.glob<string>("../assets/svg/*.svg", {
  eager: true,
  query: "?url",
  import: "default",
});

function svgUrl(file: string): string {
  return svgUrls[`../assets/svg/${file}`] ?? "";
}

interface Sprite {
  file: string;
  filter?: string;
  opacity?: number;
}

function spriteFor(entity: Entity, state: RunState): Sprite | null {
  switch (entity.kind) {
    case "player": {
      const invulnerable = state.timeMs < state.playerStats.iframeUntil;
      return { file: "player.svg", opacity: invulnerable && Math.floor(state.timeMs / 80) % 2 === 0 ? 0.35 : 1 };
    }
    case "enemy":
      return { file: `enemy-${entity.enemy}.svg` };
    case "projectile":
      return {
        file: "projectile.svg",
        filter: entity.owner === "enemy" ? "hue-rotate(140deg)" : undefined,
      };
    case "pickup":
      if (entity.pickup === "coin") return { file: "coin.svg" };
      if (entity.pickup === "portal") return { file: "coin.svg", filter: "hue-rotate(200deg) saturate(2) brightness(1.3)" };
      return { file: ITEMS[entity.itemId ?? ""]?.svg ?? "coin.svg" };
    case "door":
      return { file: "door.svg", opacity: entity.locked ? 1 : 0.4 };
  }
}

export interface Overlay {
  draw(state: RunState, cellSize?: number): void;
}

export function createOverlay(parent: HTMLElement): Overlay {
  const sprites = new Map<string, HTMLImageElement>();

  return {
    draw(state, cellSize = parent.clientWidth / ROOM_W || CELL) {
      parent.style.width = `${cellSize * ROOM_W}px`;
      parent.style.height = `${cellSize * ROOM_H}px`;
      const scale = cellSize / CELL;
      const alive = new Set<string>();

      for (const entity of state.entities) {
        const sprite = spriteFor(entity, state);
        if (!sprite) continue;
        alive.add(entity.id);

        let img = sprites.get(entity.id);
        if (!img) {
          img = document.createElement("img");
          img.className = `sprite sprite-${entity.kind}`;
          img.draggable = false;
          img.alt = "";
          parent.appendChild(img);
          sprites.set(entity.id, img);
        }

        const url = svgUrl(sprite.file);
        if (img.dataset.file !== sprite.file) {
          img.dataset.file = sprite.file;
          img.src = url;
        }
        const size = entity.radius * 2 * scale;
        img.style.width = `${size}px`;
        img.style.height = `${size}px`;
        img.style.transform = `translate(${entity.x * scale - size / 2}px, ${entity.y * scale - size / 2}px)`;
        img.style.filter = sprite.filter ?? "";
        img.style.opacity = String(sprite.opacity ?? 1);
      }

      for (const [id, img] of sprites) {
        if (alive.has(id)) continue;
        img.remove();
        sprites.delete(id);
      }
    },
  };
}
