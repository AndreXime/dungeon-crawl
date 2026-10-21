# Isaac-style Roguelike (rot.js) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Roguelike em tempo real no browser, loop Binding of Isaac (salas, limpar, itens, loja, boss), com rot.js no tilemap/lógica e SVGs no overlay.

**Architecture:** Vite + TypeScript. `RunState` central. Sistemas puros atualizam o estado a cada `dt` (input → movimento → IA → projéteis → colisão → portas → render). rot.js `Display` desenha o grid; overlay DOM/canvas posiciona SVGs nas entidades.

**Tech Stack:** Vite, TypeScript, rot-js ^2.2.1, Vitest, SVG assets locais

## Global Constraints

- Idioma da UI e commits: português brasileiro
- Sem `any` no TypeScript
- Controles fixos: WASD move, setas atiram
- 2 andares, ~6–8 salas, 4 inimigos (1 boss), 6–8 itens
- Arte SVG no repo (`src/assets/svg/`), sem CDN
- Spec: `docs/superpowers/specs/2026-10-21-isaac-roguelike-design.md`

## File Structure

```
package.json
vite.config.ts
tsconfig.json
index.html
src/main.ts
src/style.css
src/game/types.ts
src/game/constants.ts
src/game/input.ts
src/game/run.ts
src/game/loop.ts
src/world/floorGen.ts
src/world/roomGen.ts
src/data/items.ts
src/data/enemies.ts
src/systems/collision.ts
src/systems/movement.ts
src/systems/combat.ts
src/systems/ai.ts
src/systems/doors.ts
src/systems/pickups.ts
src/systems/progression.ts
src/render/tilemap.ts
src/render/overlay.ts
src/render/hud.ts
src/assets/svg/*.svg
src/vite-env.d.ts
tests/floorGen.test.ts
tests/items.test.ts
tests/collision.test.ts
tests/doors.test.ts
```

---

### Task 1: Scaffold Vite + TypeScript + Vitest + rot-js

**Files:**
- Create: `package.json`, `vite.config.ts`, `tsconfig.json`, `index.html`, `src/main.ts`, `src/style.css`, `src/vite-env.d.ts`
- Test: smoke via `npm run build`

**Interfaces:**
- Produz: app Vite rodando em `npm run dev`, testes em `npm test`

- [ ] **Step 1: Criar `package.json`**

```json
{
  "name": "isaac-roguelike",
  "private": true,
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "test": "vitest run"
  },
  "dependencies": {
    "rot-js": "^2.2.1"
  },
  "devDependencies": {
    "typescript": "^5.6.0",
    "vite": "^5.4.0",
    "vitest": "^2.1.0"
  }
}
```

- [ ] **Step 2: Criar `tsconfig.json`, `vite.config.ts`, `index.html`, stubs**

`tsconfig.json`:
```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "ESNext",
    "moduleResolution": "bundler",
    "strict": true,
    "noEmit": true,
    "skipLibCheck": true,
    "isolatedModules": true,
    "lib": ["ES2022", "DOM"]
  },
  "include": ["src", "tests"]
}
```

`vite.config.ts`:
```ts
import { defineConfig } from "vite";

export default defineConfig({
  test: {
    environment: "node",
  },
});
```

`index.html`:
```html
<!doctype html>
<html lang="pt-BR">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Chamber Crawl</title>
  </head>
  <body>
    <div id="app">
      <div id="game-root">
        <div id="tilemap"></div>
        <div id="overlay"></div>
        <div id="hud"></div>
        <div id="banner" hidden></div>
      </div>
    </div>
    <script type="module" src="/src/main.ts"></script>
  </body>
</html>
```

`src/main.ts`:
```ts
import "./style.css";

const root = document.querySelector("#app");
if (root) {
  const note = document.createElement("p");
  note.textContent = "Chamber Crawl boot OK";
  root.appendChild(note);
}
```

`src/style.css`: CSS mínimo (fundo escuro, `#game-root` relative, camadas absolute).

`src/vite-env.d.ts`:
```ts
/// <reference types="vite/client" />
```

- [ ] **Step 3: Instalar e verificar**

```bash
npm install
npm run build
npm test
```

Expected: build OK. `npm test` passa sem testes (ou “no test files” — aceitável até Task 2).

- [ ] **Step 4: Commit**

```bash
git add package.json package-lock.json vite.config.ts tsconfig.json index.html src/
git commit -m "$(cat <<'EOF'
chore: scaffold Vite TypeScript com rot-js e Vitest

EOF
)"
```

---

### Task 2: Tipos e constantes do jogo

**Files:**
- Create: `src/game/types.ts`, `src/game/constants.ts`

**Interfaces:**
- Produz: `RoomType`, `Floor`, `Entity`, `RunState`, `PlayerStats`, `TILE`, `ROOM_W`, `ROOM_H`, `CELL`

- [ ] **Step 1: Escrever `src/game/constants.ts`**

```ts
export const ROOM_W = 15;
export const ROOM_H = 11;
export const CELL = 32;
export const PLAYER_RADIUS = 10;
export const PROJECTILE_RADIUS = 4;
export const IFRAME_MS = 600;

export const TILE = {
  floor: 0,
  wall: 1,
  door: 2,
  obstacle: 3,
} as const;

export type TileId = (typeof TILE)[keyof typeof TILE];
```

- [ ] **Step 2: Escrever `src/game/types.ts`**

```ts
import type { TileId } from "./constants";

export type Dir = "N" | "S" | "E" | "W";
export type RoomType = "normal" | "item" | "shop" | "boss" | "start";
export type RunStatus = "playing" | "dead" | "won";
export type EnemyKind = "chaser" | "shooter" | "wander" | "boss";

export interface PlayerStats {
  maxHp: number;
  hp: number;
  speed: number;
  damage: number;
  fireRate: number;
  range: number;
  tripleShot: boolean;
  coinMultiplier: number;
  iframeUntil: number;
}

export interface FloorRoomNode {
  id: string;
  type: RoomType;
  neighbors: Partial<Record<Dir, string>>;
  cleared: boolean;
  visited: boolean;
  roomSeed: number;
}

export interface Floor {
  depth: 1 | 2;
  rooms: Record<string, FloorRoomNode>;
  startRoomId: string;
  bossRoomId: string;
}

export interface RoomData {
  tiles: TileId[][];
  spawns: SpawnSpec[];
}

export type SpawnSpec =
  | { kind: "enemy"; enemy: EnemyKind; tx: number; ty: number }
  | { kind: "item"; itemId: string; tx: number; ty: number; price?: number }
  | { kind: "door"; dir: Dir; tx: number; ty: number };

export interface EntityBase {
  id: string;
  x: number;
  y: number;
  vx: number;
  vy: number;
  radius: number;
}

export interface PlayerEntity extends EntityBase {
  kind: "player";
  hp: number;
}

export interface EnemyEntity extends EntityBase {
  kind: "enemy";
  enemy: EnemyKind;
  hp: number;
  cooldown: number;
  phase?: number;
}

export interface ProjectileEntity extends EntityBase {
  kind: "projectile";
  owner: "player" | "enemy";
  damage: number;
  life: number;
}

export interface PickupEntity extends EntityBase {
  kind: "pickup";
  pickup: "coin" | "item";
  itemId?: string;
  price?: number;
  amount?: number;
}

export interface DoorEntity extends EntityBase {
  kind: "door";
  dir: Dir;
  locked: boolean;
}

export type Entity =
  | PlayerEntity
  | EnemyEntity
  | ProjectileEntity
  | PickupEntity
  | DoorEntity;

export interface RunState {
  seed: number;
  floor: Floor;
  currentRoomId: string;
  room: RoomData;
  entities: Entity[];
  playerStats: PlayerStats;
  coins: number;
  inventory: string[];
  status: RunStatus;
  shootCooldown: number;
  timeMs: number;
}
```

- [ ] **Step 3: Commit**

```bash
git add src/game/types.ts src/game/constants.ts
git commit -m "$(cat <<'EOF'
feat: tipos e constantes do run

EOF
)"
```

---

### Task 3: Geração de andar (grafo de salas)

**Files:**
- Create: `src/world/floorGen.ts`, `tests/floorGen.test.ts`

**Interfaces:**
- Produz: `generateFloor(seed: number, depth: 1 | 2): Floor`
- Consome: tipos de `types.ts`

- [ ] **Step 1: Teste falhando**

```ts
import { describe, it, expect } from "vitest";
import { generateFloor } from "../src/world/floorGen";

describe("generateFloor", () => {
  it("gera 6 a 8 salas conectadas com start, boss e loja", () => {
    const floor = generateFloor(42, 1);
    const rooms = Object.values(floor.rooms);
    expect(rooms.length).toBeGreaterThanOrEqual(6);
    expect(rooms.length).toBeLessThanOrEqual(8);
    expect(floor.rooms[floor.startRoomId].type).toBe("start");
    expect(floor.rooms[floor.bossRoomId].type).toBe("boss");
    expect(rooms.some((r) => r.type === "shop")).toBe(true);
    expect(rooms.some((r) => r.type === "item")).toBe(true);

    const seen = new Set<string>();
    const stack = [floor.startRoomId];
    while (stack.length) {
      const id = stack.pop()!;
      if (seen.has(id)) continue;
      seen.add(id);
      const node = floor.rooms[id];
      for (const n of Object.values(node.neighbors)) {
        if (n) stack.push(n);
      }
    }
    expect(seen.size).toBe(rooms.length);
  });

  it("é determinístico para a mesma seed", () => {
    expect(generateFloor(7, 2)).toEqual(generateFloor(7, 2));
  });
});
```

- [ ] **Step 2: Rodar teste (deve falhar)**

```bash
npm test -- tests/floorGen.test.ts
```

Expected: FAIL (módulo/função ausente)

- [ ] **Step 3: Implementar `generateFloor`**

Usar `RNG.setSeed(seed)` do rot-js. Algoritmo:

1. Criar nó start em (0,0)
2. Expandir até 6–8 salas via BFS/random walk em grid inteiro (posições `gx,gy`)
3. Atribuir tipos: start fixo, boss = sala mais distante do start, 1 shop, 1 item, resto normal
4. Preencher `neighbors` bidirecionais N/S/E/W
5. `roomSeed = hash(seed, roomId)`

```ts
import { RNG } from "rot-js";
import type { Dir, Floor, FloorRoomNode, RoomType } from "../game/types";

const OPP: Record<Dir, Dir> = { N: "S", S: "N", E: "W", W: "E" };
const DELTA: Record<Dir, [number, number]> = {
  N: [0, -1],
  S: [0, 1],
  E: [1, 0],
  W: [-1, 0],
};

function key(x: number, y: number): string {
  return `${x},${y}`;
}

export function generateFloor(seed: number, depth: 1 | 2): Floor {
  RNG.setSeed(seed + depth * 10007);
  const target = 6 + Math.floor(RNG.getUniform() * 3); // 6..8
  const positions = new Map<string, { x: number; y: number }>();
  positions.set(key(0, 0), { x: 0, y: 0 });
  const frontier = [{ x: 0, y: 0 }];

  while (positions.size < target && frontier.length > 0) {
    const i = Math.floor(RNG.getUniform() * frontier.length);
    const cur = frontier[i]!;
    const dirs = (["N", "S", "E", "W"] as Dir[]).sort(
      () => RNG.getUniform() - 0.5,
    );
    let grew = false;
    for (const d of dirs) {
      const [dx, dy] = DELTA[d];
      const nx = cur.x + dx;
      const ny = cur.y + dy;
      const k = key(nx, ny);
      if (positions.has(k)) continue;
      positions.set(k, { x: nx, y: ny });
      frontier.push({ x: nx, y: ny });
      grew = true;
      break;
    }
    if (!grew) frontier.splice(i, 1);
  }

  const rooms: Record<string, FloorRoomNode> = {};
  for (const [id, pos] of positions) {
    rooms[id] = {
      id,
      type: "normal",
      neighbors: {},
      cleared: false,
      visited: false,
      roomSeed: (seed ^ (pos.x * 73856093) ^ (pos.y * 19349663) ^ depth) >>> 0,
    };
  }

  for (const [id, pos] of positions) {
    for (const d of ["N", "S", "E", "W"] as Dir[]) {
      const [dx, dy] = DELTA[d];
      const nid = key(pos.x + dx, pos.y + dy);
      if (!positions.has(nid)) continue;
      rooms[id]!.neighbors[d] = nid;
      rooms[nid]!.neighbors[OPP[d]] = id;
    }
  }

  const startRoomId = key(0, 0);
  let bossRoomId = startRoomId;
  let best = -1;
  for (const [id, pos] of positions) {
    const dist = Math.abs(pos.x) + Math.abs(pos.y);
    if (dist > best) {
      best = dist;
      bossRoomId = id;
    }
  }
  rooms[startRoomId]!.type = "start";
  rooms[startRoomId]!.cleared = true;
  rooms[bossRoomId]!.type = "boss";

  const others = Object.keys(rooms).filter(
    (id) => id !== startRoomId && id !== bossRoomId,
  );
  const shuffle = [...others].sort(() => RNG.getUniform() - 0.5);
  if (shuffle[0]) rooms[shuffle[0]]!.type = "shop";
  if (shuffle[1]) rooms[shuffle[1]]!.type = "item";

  return { depth, rooms, startRoomId, bossRoomId };
}
```

- [ ] **Step 4: Rodar testes**

```bash
npm test -- tests/floorGen.test.ts
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/world/floorGen.ts tests/floorGen.test.ts
git commit -m "$(cat <<'EOF'
feat: geração procedural do grafo de salas

EOF
)"
```

---

### Task 4: Geração de sala (tilemap + spawns)

**Files:**
- Create: `src/world/roomGen.ts`
- Test: estender `tests/floorGen.test.ts` ou criar `tests/roomGen.test.ts`

**Interfaces:**
- Produz: `generateRoom(node: FloorRoomNode, neighbors: Dir[]): RoomData`
- Consome: `ROOM_W`, `ROOM_H`, `TILE`, `FloorRoomNode`

- [ ] **Step 1: Teste**

```ts
import { describe, it, expect } from "vitest";
import { generateRoom } from "../src/world/roomGen";
import { ROOM_W, ROOM_H, TILE } from "../src/game/constants";
import type { FloorRoomNode } from "../src/game/types";

const base: FloorRoomNode = {
  id: "0,0",
  type: "normal",
  neighbors: { N: "0,-1", E: "1,0" },
  cleared: false,
  visited: false,
  roomSeed: 99,
};

describe("generateRoom", () => {
  it("cria borda de paredes e portas nos vizinhos", () => {
    const room = generateRoom(base, ["N", "E"]);
    expect(room.tiles.length).toBe(ROOM_H);
    expect(room.tiles[0]!.length).toBe(ROOM_W);
    expect(room.tiles[0]![Math.floor(ROOM_W / 2)]).toBe(TILE.door);
    expect(room.tiles[Math.floor(ROOM_H / 2)]![ROOM_W - 1]).toBe(TILE.door);
    expect(room.spawns.some((s) => s.kind === "enemy")).toBe(true);
  });
});
```

- [ ] **Step 2: Implementar `generateRoom`**

1. Preencher floor, paredes na borda
2. Abrir `TILE.door` no meio de cada lado com vizinho
3. Obstáculos aleatórios no miolo (seed via `RNG.setSeed(node.roomSeed)`)
4. Spawns por tipo:
   - start: sem inimigos
   - normal: 3–5 inimigos mistos (chaser/wander/shooter)
   - item: 1 spawn item gratuito
   - shop: 2–3 itens com `price`
   - boss: 1 boss no centro
5. Spawns `door` para cada direção vizinha

- [ ] **Step 3: Testes PASS + commit**

```bash
npm test -- tests/roomGen.test.ts
git add src/world/roomGen.ts tests/roomGen.test.ts
git commit -m "$(cat <<'EOF'
feat: geração de tilemap e spawns por tipo de sala

EOF
)"
```

---

### Task 5: Colisão e itens (funções puras)

**Files:**
- Create: `src/systems/collision.ts`, `src/data/items.ts`, `tests/collision.test.ts`, `tests/items.test.ts`

**Interfaces:**
- Produz: `circleHitsTile`, `circlesOverlap`, `applyItem`, `ITEMS`, `isRoomCleared`

- [ ] **Step 1: Testes**

```ts
// tests/collision.test.ts
import { describe, it, expect } from "vitest";
import { circlesOverlap, circleHitsSolid } from "../src/systems/collision";
import { TILE, CELL } from "../src/game/constants";

describe("collision", () => {
  it("detecta overlap de círculos", () => {
    expect(circlesOverlap(0, 0, 5, 6, 0, 5)).toBe(true);
    expect(circlesOverlap(0, 0, 5, 20, 0, 5)).toBe(false);
  });

  it("bloqueia círculo contra tile parede", () => {
    const tiles = [
      [TILE.wall, TILE.wall],
      [TILE.wall, TILE.floor],
    ];
    expect(circleHitsSolid(CELL * 1.5, CELL * 1.5, 8, tiles)).toBe(false);
    expect(circleHitsSolid(8, 8, 8, tiles)).toBe(true);
  });
});
```

```ts
// tests/items.test.ts
import { describe, it, expect } from "vitest";
import { applyItem, ITEMS } from "../src/data/items";
import type { PlayerStats } from "../src/game/types";

const base = (): PlayerStats => ({
  maxHp: 6,
  hp: 4,
  speed: 120,
  damage: 1,
  fireRate: 4,
  range: 320,
  tripleShot: false,
  coinMultiplier: 1,
  iframeUntil: 0,
});

describe("applyItem", () => {
  it("aplica boost de dano", () => {
    const s = applyItem(base(), "damage_up");
    expect(s.damage).toBe(base().damage + ITEMS.damage_up.damage!);
  });
});
```

- [ ] **Step 2: Implementar**

`collision.ts`: amostrar 4–8 pontos na circunferência e checar tile `wall`/`obstacle` (porta não bloqueia se unlocked — tratado em doors).

`items.ts`:
```ts
export const ITEMS: Record<string, ItemDef> = {
  damage_up: { id: "damage_up", name: "Dente Afiado", damage: 1, svg: "item-damage.svg" },
  speed_up: { id: "speed_up", name: "Botas", speed: 30, svg: "item-speed.svg" },
  hp_up: { id: "hp_up", name: "Coração Extra", maxHp: 2, heal: 2, svg: "item-hp.svg" },
  fire_rate: { id: "fire_rate", name: "Cafeína", fireRate: 1.5, svg: "item-firerate.svg" },
  triple: { id: "triple", name: "Tridente", tripleShot: true, svg: "item-triple.svg" },
  greedy: { id: "greedy", name: "Ímã", coinMultiplier: 1, svg: "item-greedy.svg" },
};
```

`applyItem` clona stats e soma campos definidos. `greedy` faz `coinMultiplier += 1`.

Também em `collision.ts` ou `doors.ts`:
```ts
export function isRoomCleared(entities: Entity[]): boolean {
  return !entities.some((e) => e.kind === "enemy" && e.hp > 0);
}
```

- [ ] **Step 3: PASS + commit**

```bash
npm test
git add src/systems/collision.ts src/data/items.ts tests/
git commit -m "$(cat <<'EOF'
feat: colisão círculo-tile e aplicação de itens

EOF
)"
```

---

### Task 6: Input, bootstrap do run e movimento

**Files:**
- Create: `src/game/input.ts`, `src/game/run.ts`, `src/systems/movement.ts`, `src/data/enemies.ts`

**Interfaces:**
- Produz: `createInput()`, `createRun(seed)`, `updateMovement(state, input, dt)`, `spawnRoomEntities`

- [ ] **Step 1: `input.ts`**

Track `Set` de teclas. Mapear:
- move: W/A/S/D → vetor normalizado
- shoot: ArrowUp/Down/Left/Right → vetor (prioridade última tecla se quiser, ou soma ortogonal)

```ts
export interface InputState {
  moveX: number;
  moveY: number;
  shootX: number;
  shootY: number;
  restart: boolean;
}

export function createInput(target: Window = window): {
  get(): InputState;
  dispose(): void;
}
```

- [ ] **Step 2: `run.ts`**

`createRun(seed)`:
1. `generateFloor(seed, 1)`
2. marca start `visited`
3. `generateRoom` da start
4. spawna player no centro + entidades da sala
5. stats iniciais: hp 6, speed 120, damage 1, fireRate 4, range 320

`enterRoom(state, roomId, entryDir?)` regenera room/entities e posiciona player na porta oposta.

- [ ] **Step 3: `movement.ts`**

Player: `vx,vy` de input * speed. Integrar posição. Se `circleHitsSolid`, tentar slide no eixo X e Y separadamente.

- [ ] **Step 4: Commit**

```bash
git add src/game/input.ts src/game/run.ts src/systems/movement.ts src/data/enemies.ts
git commit -m "$(cat <<'EOF'
feat: input, bootstrap do run e movimento com colisão

EOF
)"
```

---

### Task 7: Combate (tiros do player e inimigos)

**Files:**
- Create: `src/systems/combat.ts`
- Modify: loop ainda não, só sistema

**Interfaces:**
- Produz: `updateCombat(state, input, dt): void` (mutates state)

- [ ] **Step 1: Implementar tiro do player**

Se `shootX/shootY` e `shootCooldown <= 0`:
- spawn projectile(s) na posição do player
- se `tripleShot`, 3 projéteis com spread ±15°
- `life = range / projectileSpeed`
- reset cooldown `1 / fireRate`

- [ ] **Step 2: Integrar projéteis + hits**

Cada frame: mover projéteis, destruir em parede, se owner player vs enemy aplicar damage e remover projectile, se owner enemy vs player aplicar damage com i-frames (`iframeUntil`).

Ao matar inimigo: chance de drop coin (`amount = coinMultiplier`).

Player hp ≤ 0 → `status = "dead"`.

- [ ] **Step 3: Commit**

```bash
git add src/systems/combat.ts
git commit -m "$(cat <<'EOF'
feat: sistema de projéteis, dano e i-frames

EOF
)"
```

---

### Task 8: IA dos inimigos

**Files:**
- Create: `src/systems/ai.ts`
- Modify: `src/data/enemies.ts` (stats: hp, speed, damage)

**Interfaces:**
- Produz: `updateAi(state, dt)`

- [ ] **Step 1: Comportamentos**

- **chaser:** direção ao player, velocidade constante, colisão tile
- **wander:** a cada 0.5–1.5s escolhe dir aleatória
- **shooter:** se dist < preferida afasta, senão aproxima leve; a cada cooldown atira ao player
- **boss:** `phase` timer — 2s chase charge, 1s rajada 8 direções, repete

Dano de contato: se overlap com player e sem i-frame, aplicar damage inimigo.

- [ ] **Step 2: Commit**

```bash
git add src/systems/ai.ts src/data/enemies.ts
git commit -m "$(cat <<'EOF'
feat: IA chaser, shooter, wander e boss

EOF
)"
```

---

### Task 9: Portas, limpeza de sala e transição

**Files:**
- Create: `src/systems/doors.ts`, `tests/doors.test.ts`
- Modify: `src/game/run.ts` (`enterRoom`)

**Interfaces:**
- Produz: `updateDoors(state): void`, `isRoomCleared`

- [ ] **Step 1: Teste `isRoomCleared`**

```ts
import { describe, it, expect } from "vitest";
import { isRoomCleared } from "../src/systems/doors";

describe("isRoomCleared", () => {
  it("true sem inimigos vivos", () => {
    expect(isRoomCleared([])).toBe(true);
    expect(
      isRoomCleared([
        {
          id: "1",
          kind: "enemy",
          enemy: "chaser",
          x: 0,
          y: 0,
          vx: 0,
          vy: 0,
          radius: 8,
          hp: 0,
          cooldown: 0,
        },
      ]),
    ).toBe(true);
  });
});
```

- [ ] **Step 2: Implementar**

Quando cleared: marcar `floor.rooms[id].cleared = true`, `locked = false` em doors.

Se player overlap porta unlocked: `enterRoom(state, neighborId, dir)`.

Sala boss do depth 1 cleared + player na “saída” (ou item portal no centro após boss): `startFloor2(state)` gera floor depth 2 e reseta para start.

Boss depth 2 morto: `status = "won"`.

- [ ] **Step 3: PASS + commit**

```bash
npm test -- tests/doors.test.ts
git add src/systems/doors.ts src/systems/progression.ts src/game/run.ts tests/doors.test.ts
git commit -m "$(cat <<'EOF'
feat: portas, transição de salas e progressão de andares

EOF
)"
```

Nota: criar `src/systems/progression.ts` com `advanceFloor` e checagem de vitória.

---

### Task 10: Pickups e loja

**Files:**
- Create: `src/systems/pickups.ts`

**Interfaces:**
- Produz: `updatePickups(state)`

- [ ] **Step 1: Implementar**

Overlap player + pickup:
- coin: `coins += amount`
- item sem price / price 0: `applyItem`, push inventory, remover entity
- item com price: se `coins >= price`, debitar, aplicar, remover; senão ignorar

- [ ] **Step 2: Commit**

```bash
git add src/systems/pickups.ts
git commit -m "$(cat <<'EOF'
feat: coleta de moedas e itens da loja

EOF
)"
```

---

### Task 11: Assets SVG

**Files:**
- Create: `src/assets/svg/player.svg`, `enemy-chaser.svg`, `enemy-shooter.svg`, `enemy-wander.svg`, `enemy-boss.svg`, `projectile.svg`, `heart.svg`, `coin.svg`, `door.svg`, `item-damage.svg`, `item-speed.svg`, `item-hp.svg`, `item-firerate.svg`, `item-triple.svg`, `item-greedy.svg`

**Interfaces:**
- Produz: SVGs 32×32 flat, viewBox `0 0 32 32`, cores contrastantes

- [ ] **Step 1: Criar SVGs**

Estilo: formas geométricas simples (círculos, triângulos, olhos). Player azul. Chaser vermelho. Shooter roxo. Wander verde. Boss grande laranja. Projétil amarelo.

Exemplo player:
```svg
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32">
  <circle cx="16" cy="16" r="12" fill="#4fc3f7"/>
  <circle cx="12" cy="13" r="2" fill="#0d47a1"/>
  <circle cx="20" cy="13" r="2" fill="#0d47a1"/>
</svg>
```

- [ ] **Step 2: Commit**

```bash
git add src/assets/svg/
git commit -m "$(cat <<'EOF'
feat: SVGs flat para entidades e HUD

EOF
)"
```

---

### Task 12: Render tilemap (rot.js) + overlay SVG + HUD

**Files:**
- Create: `src/render/tilemap.ts`, `src/render/overlay.ts`, `src/render/hud.ts`
- Modify: `src/style.css`

**Interfaces:**
- Produz: `createTilemapRenderer(el)`, `createOverlay(el)`, `createHud(el)` com `.draw(state)`

- [ ] **Step 1: Tilemap**

```ts
import { Display } from "rot-js";
import { ROOM_W, ROOM_H, TILE, CELL } from "../game/constants";

export function createTilemapRenderer(parent: HTMLElement) {
  const display = new Display({
    width: ROOM_W,
    height: ROOM_H,
    fontSize: 20,
    bg: "#1a1a2e",
    forceSquareRatio: true,
  });
  parent.appendChild(display.getContainer()!);
  return {
    draw(state: RunState) {
      for (let y = 0; y < ROOM_H; y++) {
        for (let x = 0; x < ROOM_W; x++) {
          const t = state.room.tiles[y]![x]!;
          if (t === TILE.wall) display.draw(x, y, "#", "#555", "#111");
          else if (t === TILE.door) display.draw(x, y, "+", "#c9a227", "#222");
          else if (t === TILE.obstacle) display.draw(x, y, "*", "#666", "#1a1a2e");
          else display.draw(x, y, ".", "#333", "#1a1a2e");
        }
      }
    },
    cellSize(): number {
      const c = display.getContainer() as HTMLElement;
      return c.clientWidth / ROOM_W;
    },
  };
}
```

- [ ] **Step 2: Overlay**

Para cada entity (exceto door opcional), `<img>` ou inline SVG absoluto em `left/top` convertendo mundo px → CSS px (`x / CELL * cellSize`). Mapear kind → asset.

- [ ] **Step 3: HUD**

Coraçãoes (`hp/2` se quiser pares), moedas, `Andar {depth}`, minimapa  (grid de rooms visitados, atual destacado). Banner dead/won com “R para reiniciar”.

- [ ] **Step 4: Commit**

```bash
git add src/render/ src/style.css
git commit -m "$(cat <<'EOF'
feat: render híbrido rot.js + overlay SVG e HUD

EOF
)"
```

---

### Task 13: Loop principal e wiring

**Files:**
- Create: `src/game/loop.ts`
- Modify: `src/main.ts`, `index.html` se preciso

**Interfaces:**
- Produz: jogo jogável de ponta a ponta

- [ ] **Step 1: `loop.ts`**

```ts
export function startGame(root: {
  tilemap: HTMLElement;
  overlay: HTMLElement;
  hud: HTMLElement;
  banner: HTMLElement;
}) {
  let state = createRun(Date.now() % 1_000_000);
  const input = createInput();
  const tilemap = createTilemapRenderer(root.tilemap);
  const overlay = createOverlay(root.overlay);
  const hud = createHud(root.hud, root.banner);

  let last = performance.now();
  function frame(now: number) {
    const dt = Math.min(0.05, (now - last) / 1000);
    last = now;
    const inp = input.get();
    if (state.status !== "playing") {
      if (inp.restart) state = createRun(Date.now() % 1_000_000);
    } else {
      state.timeMs += dt * 1000;
      updateMovement(state, inp, dt);
      updateAi(state, dt);
      updateCombat(state, inp, dt);
      updatePickups(state);
      updateDoors(state);
      updateProgression(state);
    }
    tilemap.draw(state);
    overlay.draw(state, tilemap.cellSize());
    hud.draw(state);
    requestAnimationFrame(frame);
  }
  requestAnimationFrame(frame);
  return () => input.dispose();
}
```

Ordem dos sistemas conforme spec.

- [ ] **Step 2: `main.ts` liga DOM e chama `startGame`**

- [ ] **Step 3: Smoke manual**

```bash
npm run dev
```

Checklist: mover, atirar, matar, porta abre, pegar item, loja, boss, andar 2, morrer/vencer + R.

- [ ] **Step 4: Commit**

```bash
git add src/game/loop.ts src/main.ts
git commit -m "$(cat <<'EOF'
feat: loop do jogo e integração completa do MVP

EOF
)"
```

---

### Task 14: README e verificação final

**Files:**
- Create: `README.md`

- [ ] **Step 1: README**

Como rodar (`npm install`, `npm run dev`, `npm test`), controles, objetivo do MVP.

- [ ] **Step 2: Rodar suíte**

```bash
npm test
npm run build
```

Expected: tudo verde.

- [ ] **Step 3: Commit**

```bash
git add README.md
git commit -m "$(cat <<'EOF'
docs: README com controles e como rodar

EOF
)"
```

---

## Spec coverage (self-review)

| Requisito | Task |
|-----------|------|
| Vite + TS + rot.js | 1 |
| Grafo 6–8 salas, 2 andares | 3, 9 |
| Tipos sala normal/item/loja/boss | 3, 4 |
| WASD + setas tempo real | 6, 7, 13 |
| 4 inimigos + boss patterns | 8 |
| 6 itens | 5, 10, 11 |
| Moedas + loja | 4, 10 |
| Portas trancadas até clear | 9 |
| SVGs locais | 11, 12 |
| Render híbrido | 12 |
| Morte/vitória + restart | 7, 9, 13 |
| Testes floor/items/clear | 3, 5, 9 |

Sem placeholders. Nomes de tipos alinhados entre tasks (`RunState`, `generateFloor`, `applyItem`, `isRoomCleared`).
