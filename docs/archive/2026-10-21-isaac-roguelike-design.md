# Design: Roguelike estilo Binding of Isaac com rot.js

Data: 2026-10-21  
Status: aprovado pelo usuário (brainstorming)

## Objetivo

Criar um roguelike em tempo real no browser, inspirado no loop de Binding of Isaac (salas conectadas, limpar inimigos, itens, loja, boss), usando rot.js na lógica/mapa e SVGs no overlay visual.

## Escopo do MVP

- 2 andares
- ~6–8 salas por andar (normal, item, loja, boss)
- Controles: WASD move, setas atiram (hold = fire rate)
- 4 tipos de inimigo (incluindo 1 boss com padrões simples)
- 6–8 itens que alteram stats / padrão de tiro
- Moedas + loja
- Morte = restart do run
- Vitória = derrotar o boss do 2º andar

### Fora do MVP

- Saves persistentes, multiplayer, bosses complexos, sinergias profundas, áudio polido, sprites animados avançados

## Abordagem técnica

**Híbrido (escolhida):**

1. rot.js: geração de mapas/salas, RNG, pathfinding, Display do tilemap
2. Overlay (canvas ou DOM): entidades com SVGs (player, inimigos, projéteis, itens, HUD)

**Stack:** Vite + TypeScript + rot.js. Node.js apenas como tooling (npm/Vite). Jogo roda no browser.

## Arquitetura

| Camada | Responsabilidade |
|--------|------------------|
| Floor | Grafo de salas, tipos, conexões N/S/L/O, seed do andar |
| Room | Tilemap da sala, obstáculos, spawns |
| Entities | Player, inimigos, projéteis, pickups (posição contínua em px sobre o grid) |
| Systems | Input, movimento, tiro, IA, colisão, portas, progressão |
| Render | rot.js Display (chão/paredes) + overlay SVG |
| HUD | HP, moedas, minimapa, andar |

### Loop principal

`requestAnimationFrame` com `dt` em segundos. Sistemas rodam na ordem: input → movimento → IA → projéteis → colisão → portas/transição → render.

## Gameplay

### Player

- Stats: HP, velocidade, dano, fire rate, alcance
- Tiro inicial ortogonal (4 direções)
- I-frames curtos após dano

### Inimigos

1. **Chaser** — persegue o player
2. **Shooter** — mantém distância e atira
3. **Wander** — movimento errático, dano no contato
4. **Boss** — HP alto, charge + rajada de tiros

### Salas

- **Normal:** inimigos
- **Item:** um pedestal com item gratuito
- **Loja:** 2–3 itens compráveis com moedas
- **Boss:** boss do andar + recompensa

Portas ficam trancadas enquanto houver inimigos vivos. Cruzar a porta troca instantaneamente para a sala vizinha.

### Itens (exemplos)

- Mais dano
- Mais velocidade
- Mais HP / cura
- Fire rate maior
- Tiro triplo
- Mais moedas ao matar

### Economia

Inimigos dropam moedas. Loja consome moedas. Sem refund.

### Fim de run

- HP ≤ 0 → tela de derrota + restart
- Boss do andar 2 morto → tela de vitória + restart

## Modelo de dados

```ts
type RoomType = "normal" | "item" | "shop" | "boss" | "start";

interface FloorRoomNode {
  id: string;
  type: RoomType;
  neighbors: Partial<Record<"N" | "S" | "E" | "W", string>>;
  cleared: boolean;
  visited: boolean;
  roomSeed: number;
}

interface Floor {
  depth: 1 | 2;
  rooms: Record<string, FloorRoomNode>;
  startRoomId: string;
  bossRoomId: string;
}

interface Entity {
  id: string;
  kind: "player" | "enemy" | "projectile" | "pickup" | "door";
  x: number;
  y: number;
  vx: number;
  vy: number;
  radius: number;
  hp?: number;
  // campos específicos por kind
}

interface RunState {
  seed: number;
  floor: Floor;
  currentRoomId: string;
  entities: Entity[];
  playerStats: PlayerStats;
  coins: number;
  inventory: string[];
  status: "playing" | "dead" | "won";
}
```

## Colisão

- Entidades móveis: círculo
- Paredes: AABB do tile sólido
- Projéteis: círculo pequeno vs paredes e hitboxes inimigas/player
- Porta: trigger na célula da porta se `cleared === true`

## Arte (SVG)

Arquivos em `src/assets/svg/` (ou `public/svg/`):

- `player.svg`
- `enemy-chaser.svg`, `enemy-shooter.svg`, `enemy-wander.svg`, `enemy-boss.svg`
- `projectile.svg`
- `heart.svg`, `coin.svg`, `door.svg`
- Itens genéricos (`item-*.svg`)

Estilo: flat, silhueta clara em ~32–48px. Criados no repositório (não dependem de CDN).

## Estrutura de pastas (proposta)

```
src/
  main.ts
  game/
    loop.ts
    run.ts
    input.ts
  world/
    floorGen.ts
    roomGen.ts
  entities/
    player.ts
    enemies.ts
    projectiles.ts
    pickups.ts
  systems/
    movement.ts
    combat.ts
    collision.ts
    doors.ts
    ai.ts
  render/
    tilemap.ts
    overlay.ts
    hud.ts
  data/
    items.ts
    enemies.ts
  assets/svg/
```

## Testes

- Unitários leves em funções puras: geração de floor (conexividade, presença de boss/loja), aplicação de item em stats, `isRoomCleared`
- Smoke manual no browser para combate, portas e progressão de andares

## Critérios de sucesso

1. Dá para completar um run (dois andares) do início ao boss final
2. Loop de sala (entrar → limpar → portas → item/loja) está claro
3. Controles WASD + setas respondem em tempo real
4. Visual híbrido (tilemap rot.js + SVG) é legível
5. Morte e vitória reiniciam o run sem reload manual da página
