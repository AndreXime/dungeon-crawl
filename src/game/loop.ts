import { createRun } from "./run";
import { createInput } from "./input";
import { createTilemapRenderer } from "../render/tilemap";
import { createOverlay } from "../render/overlay";
import { createHud } from "../render/hud";
import { updateMovement } from "../systems/movement";
import { updateAi } from "../systems/ai";
import { updateCombat } from "../systems/combat";
import { updatePickups } from "../systems/pickups";
import { updateDoors } from "../systems/doors";
import { updateProgression } from "../systems/progression";

const MAX_FRAME_DT_S = 0.05;

function newSeed(): number {
  return Date.now() % 1_000_000;
}

export function startGame(root: {
  tilemap: HTMLElement;
  overlay: HTMLElement;
  hud: HTMLElement;
}): () => void {
  let state = createRun(newSeed());
  const input = createInput();
  const tilemap = createTilemapRenderer(root.tilemap);
  const overlay = createOverlay(root.overlay);
  const hud = createHud(root.hud);

  let last = performance.now();
  let frameId = 0;

  function frame(now: number) {
    const dt = Math.min(MAX_FRAME_DT_S, (now - last) / 1000);
    last = now;
    const playerInput = input.get();

    if (state.status !== "playing") {
      if (playerInput.restart) state = createRun(newSeed());
    } else {
      // state.timeMs é avançado dentro de updateCombat (base dos i-frames), não aqui.
      updateMovement(state, playerInput, dt);
      updateAi(state, dt);
      updateCombat(state, playerInput, dt);
      updatePickups(state);
      updateDoors(state);
      updateProgression(state);
    }

    tilemap.draw(state);
    overlay.draw(state, tilemap.cellSize());
    hud.draw(state);
    frameId = requestAnimationFrame(frame);
  }
  frameId = requestAnimationFrame(frame);

  return () => {
    cancelAnimationFrame(frameId);
    input.dispose();
  };
}
