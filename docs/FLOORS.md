# Andares

Referência dos biomas 1–5. Portal após o boss se depth &lt; 5. Vitória da run no depth 5.

Cada andar tem identidade própria: tiles, pool de inimigos, boss, obstáculos e tema musical (`*Theme.js` → WAV em `Assets/Music/`). Raridade da loja e HP/dano dos inimigos sobem com o depth.

---

## Andar 1 — Masmorra de Pedra

| | |
|---|---|
| **Clima** | Pedra úmida, exploração tensa |
| **Tiles** | `floor-1` / `wall-1` / `door-1`. Aberta: `wall` + `door-open` |
| **Inimigos** | Morcego, Esqueleto, Slime |
| **Boss** | Ogre (charge + projétil) |
| **Obstáculos** | crate / barrel / spike |
| **Música** | `FirstFloorTheme` |

---

## Andar 2 — Catacumbas

| | |
|---|---|
| **Clima** | Ossos, cinza frio, corredores estreitos |
| **Tiles** | `floor-2` / `wall-2` / `door-2`. Aberta: `wall` + `door-open` |
| **Inimigos** | Esqueleto (mais comum), Morcego; **Espectro** (pisca/teleporta, frágil, atravessa paredes) |
| **Boss** | **Necromante** — distância, orbes roxos, invoca esqueletos (cap 3; enrage em 50% HP) |
| **Obstáculos** | crate/barrel + `spike-bone` |
| **Música** | `CatacombsTheme` |
| **Diff** | +1 inimigo médio nas salas normais |

---

## Andar 3 — Esgotos

| | |
|---|---|
| **Clima** | Verde-limo, água rasa, escorregadio |
| **Tiles** | `floor-3` / `wall-3` / `door-3`. Aberta: `wall` + `door-open` |
| **Inimigos** | Slime (dominante); **Rato** (rápido, melee); Morcego |
| **Boss** | **Rei Lodo** — spit + divide em 2 slimes médios ao morrer (1x) |
| **Obstáculos** | Poças (`TilePuddle`) — 50% speed; Slime/Rei Lodo imunes |
| **Música** | `SewersTheme` |
| **Diff** | +1 inimigo extra vs depth 2 |

---

## Andar 4 — Arsenal

| | |
|---|---|
| **Clima** | Ferro, tochas, disciplina militar |
| **Tiles** | `floor-4` / `wall-4` / `door-4`. Aberta: `wall` + `door-open` |
| **Inimigos** | **Guarda** (lento, tank, melee); Esqueleto; Slime raro |
| **Boss** | **Golem de Ferro** — projéteis → charge → vulnerável (2x dano, tint laranja) |
| **Obstáculos** | `barricade` / `anvil` / spike (sem crate/barrel) |
| **Música** | `ArmoryTheme` |
| **Diff** | +1 inimigo extra vs depth 3 |

---

## Andar 5 — Santuário Sombrio (final)

| | |
|---|---|
| **Clima** | Roxo/carmesim, runas, ritual |
| **Tiles** | `floor-5` / `wall-5` / `door-5`. Aberta: `wall` + `door-open` |
| **Inimigos** | Espectro, Guarda, **Acólito** (orbita e atira magia) |
| **Boss** | **Senhor das Runas** — orbes, invoca Acólitos, burst em ≤50% HP |
| **Obstáculos** | `TileRune` (dano 1 / iframe), `TilePedestal` (sólido) |
| **Música** | `SanctumTheme`. Boss: `BossTheme` |
| **Diff** | Matar o boss = vitória da run |

---

## Polimento opcional

- Stingers de boss por bioma
- SFX (bfxr / music-studio)

## Fora de escopo

- Depth infinito / endless
- Andares secretos / side floors
- Multiplayer / co-op
