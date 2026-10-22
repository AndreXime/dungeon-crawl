export interface InputState {
  moveX: number;
  moveY: number;
  shootX: number;
  shootY: number;
  restart: boolean;
}

const GAME_KEYS = new Set([
  "KeyW",
  "KeyA",
  "KeyS",
  "KeyD",
  "ArrowUp",
  "ArrowDown",
  "ArrowLeft",
  "ArrowRight",
  "KeyR",
]);

function axis(pressed: Set<string>, negative: string, positive: string): number {
  return (pressed.has(positive) ? 1 : 0) - (pressed.has(negative) ? 1 : 0);
}

function normalize(x: number, y: number): [number, number] {
  const length = Math.hypot(x, y);
  return length === 0 ? [0, 0] : [x / length, y / length];
}

export function createInput(target: Window = window): {
  get(): InputState;
  dispose(): void;
} {
  const pressed = new Set<string>();

  const onKeyDown = (e: KeyboardEvent) => {
    if (!GAME_KEYS.has(e.code)) return;
    e.preventDefault();
    pressed.add(e.code);
  };
  const onKeyUp = (e: KeyboardEvent) => {
    pressed.delete(e.code);
  };
  const onBlur = () => pressed.clear();

  target.addEventListener("keydown", onKeyDown);
  target.addEventListener("keyup", onKeyUp);
  target.addEventListener("blur", onBlur);

  return {
    get() {
      const [moveX, moveY] = normalize(
        axis(pressed, "KeyA", "KeyD"),
        axis(pressed, "KeyW", "KeyS"),
      );
      const [shootX, shootY] = normalize(
        axis(pressed, "ArrowLeft", "ArrowRight"),
        axis(pressed, "ArrowUp", "ArrowDown"),
      );
      return { moveX, moveY, shootX, shootY, restart: pressed.has("KeyR") };
    },
    dispose() {
      target.removeEventListener("keydown", onKeyDown);
      target.removeEventListener("keyup", onKeyUp);
      target.removeEventListener("blur", onBlur);
      pressed.clear();
    },
  };
}
