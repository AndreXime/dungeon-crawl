import type { RunState } from "../game/types";

export interface Hud {
  draw(state: RunState): void;
}

const heartUrls = import.meta.glob<string>("../assets/svg/heart.svg", {
  eager: true,
  query: "?url",
  import: "default",
});
const coinUrls = import.meta.glob<string>("../assets/svg/coin.svg", {
  eager: true,
  query: "?url",
  import: "default",
});
const heartUrl = Object.values(heartUrls)[0] ?? "";
const coinUrl = Object.values(coinUrls)[0] ?? "";

const MINIMAP_CELL = 10;

function parseRoomCoords(id: string): { x: number; y: number } | null {
  const match = /^(-?\d+)[,_:](-?\d+)$/.exec(id);
  return match ? { x: Number(match[1]), y: Number(match[2]) } : null;
}

function heartsMarkup(hp: number, maxHp: number): string {
  const total = Math.ceil(maxHp / 2);
  let html = "";
  for (let i = 0; i < total; i++) {
    const filled = hp - i * 2;
    const cls = filled >= 2 ? "full" : filled === 1 ? "half" : "empty";
    html += `<img class="heart ${cls}" src="${heartUrl}" alt="" />`;
  }
  return html;
}

function minimapMarkup(state: RunState): string {
  const cells = Object.values(state.floor.rooms)
    .filter((room) => room.visited || room.id === state.currentRoomId)
    .map((room) => ({ room, pos: parseRoomCoords(room.id) }))
    .filter((c): c is { room: typeof c.room; pos: { x: number; y: number } } => c.pos !== null);
  if (cells.length === 0) return "";

  const minX = Math.min(...cells.map((c) => c.pos.x));
  const minY = Math.min(...cells.map((c) => c.pos.y));
  const maxX = Math.max(...cells.map((c) => c.pos.x));
  const maxY = Math.max(...cells.map((c) => c.pos.y));
  const w = (maxX - minX + 1) * MINIMAP_CELL;
  const h = (maxY - minY + 1) * MINIMAP_CELL;

  const rects = cells
    .map(({ room, pos }) => {
      const current = room.id === state.currentRoomId;
      const cls = `mini-room ${room.type}${current ? " current" : ""}`;
      const x = (pos.x - minX) * MINIMAP_CELL;
      const y = (pos.y - minY) * MINIMAP_CELL;
      return `<rect class="${cls}" x="${x + 1}" y="${y + 1}" width="${MINIMAP_CELL - 2}" height="${MINIMAP_CELL - 2}" />`;
    })
    .join("");
  return `<svg class="minimap" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}">${rects}</svg>`;
}

export function createHud(parent: HTMLElement): Hud {
  const banner = parent.ownerDocument.getElementById("banner");
  let lastHud = "";
  let lastBanner = "";

  return {
    draw(state) {
      const { hp, maxHp } = state.playerStats;
      const html = `
        <div class="hud-left">
          <div class="hearts">${heartsMarkup(hp, maxHp)}</div>
          <div class="coins"><img src="${coinUrl}" alt="" /><span>${state.coins}</span></div>
        </div>
        <div class="hud-right">
          <div class="depth">Andar ${state.floor.depth}</div>
          ${minimapMarkup(state)}
        </div>`;
      if (html !== lastHud) {
        parent.innerHTML = html;
        lastHud = html;
      }

      if (!banner) return;
      const text =
        state.status === "dead"
          ? "Você morreu"
          : state.status === "won"
            ? "Vitória!"
            : "";
      if (text === lastBanner) return;
      lastBanner = text;
      banner.hidden = text === "";
      banner.className = state.status;
      banner.innerHTML = text ? `<strong>${text}</strong><span>R para reiniciar</span>` : "";
    },
  };
}
