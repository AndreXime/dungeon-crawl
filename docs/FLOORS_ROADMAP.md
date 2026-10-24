# Roadmap de andares

Cada andar precisa ter **identidade própria**: bioma, inimigos, boss, tiles, música e ritmo de combate. Não basta recolorir o mesmo mapa.

Hoje: depths 1–5 via portal após o boss (vitória só no depth 5). Andar 2 (Catacumbas) tem tiles e Espectro.

## Princípios

1. **Um bioma por depth** (tile + cor dominante + música de exploração).
2. **Pool de inimigos muda** (1–2 tipos novos + remix dos anteriores).
3. **Boss único** com padrão próprio (não só mais HP no Ogre).
4. **Obstáculos/armadilhas** do bioma (não só crate/barrel/spike genéricos).
5. **Loja/item** continuam, mas o peso de raridade pode subir com o depth.
6. **Tema Strudel** próprio (`NthFloorTheme.js` + WAV) + stinger de boss opcional.

---

## Andar 1 — Masmorra de Pedra (atual / MVP)

| | |
|---|---|
| **Clima** | Pedra úmida, exploração tensa |
| **Tiles** | `floor-1` / `wall-1` / `door-1` (fechada). Aberta: `wall` + `door-open` |
| **Inimigos** | Morcego, Esqueleto, Slime |
| **Boss** | Ogre (charge + projétil) |
| **Música** | `FirstFloorTheme` |
| **Status** | Feito |

---

## Andar 2 — Catacumbas

| | |
|---|---|
| **Clima** | Ossos, cinza frio, corredores estreitos |
| **Tiles** | Floor cinza-osso, wall com ossários, spike → `spike-bone` |
| **Inimigos** | Esqueleto (mais comum), Morcego; **Espectro** (pisca/teleporta, frágil, atravessa paredes) |
| **Boss** | **Necromante** — mantém distância, orbes roxos, invoca esqueletos fracos (cap 3; enrage em 50% HP) |
| **Obstáculos** | crate/barrel + spike ósseo (`spike-bone`) |
| **Tiles** | `floor-2` / `wall-2` / `door-2`. Aberta: `wall` + `door-open` |
| **Música** | `CatacombsTheme` (fonte Strudel pronta; exportar WAV) |
| **Diff** | +1 inimigo médio nas salas normais |
| **Status** | Feito (tiles + Espectro + Necromante + tema Strudel + spike-bone) |

---

## Andar 3 — Esgotos

| | |
|---|---|
| **Clima** | Verde-limo, água rasa, escorregadio |
| **Tiles** | `floor-3` / `wall-3` / `door-3`. Aberta: `wall` + `door-open` |
| **Inimigos** | Slime (dominante); **Rato** (rápido, melee); Morcego |
| **Boss** | **Rei Lodo** — spit + divide em 2 slimes médios ao morrer (1x) |
| **Obstáculos** | Poças (`TilePuddle`) — 50% speed; Slime/Rei Lodo imunes |
| **Música** | `SewersTheme` (fonte Strudel pronta; exportar WAV) |
| **Diff** | +1 inimigo extra vs depth 2 |
| **Status** | Feito (tiles + Rato + Rei Lodo + slow zones + tema Strudel) |

---

## Andar 4 — Arsenal

| | |
|---|---|
| **Clima** | Ferro, tochas, disciplina militar |
| **Tiles** | `floor-4` / `wall-4` / `door-4`. Aberta: `wall` + `door-open` |
| **Inimigos** | **Guarda** (lento, tank, melee); Esqueleto; Slime raro |
| **Boss** | **Golem de Ferro** — projéteis → charge → vulnerável (2x dano, tint laranja) |
| **Obstáculos** | `barricade` / `anvil` / spike (sem crate/barrel) |
| **Música** | `ArmoryTheme` (fonte Strudel pronta; exportar WAV) |
| **Diff** | +1 inimigo extra vs depth 3 |
| **Status** | Feito (tiles + Guarda + Golem + tema Strudel + obstáculos) |

---

## Andar 5 — Santuário Sombrio (final)

| | |
|---|---|
| **Clima** | Roxo/carmesim, runas, ritual |
| **Tiles** | `floor-5` / `wall-5` / `door-5`. Aberta: `wall` + `door-open` |
| **Inimigos** | Espectro, Guarda, **Acólito** (orbita e atira magia) |
| **Boss** | **Senhor das Runas** — orbes, invoca Acólitos, burst em ≤50% HP |
| **Obstáculos** | `TileRune` (dano 1 / iframe), `TilePedestal` (sólido) |
| **Música** | `SanctumTheme` (fonte Strudel pronta; exportar WAV). Boss: `BossTheme` |
| **Diff** | Matar o boss = vitória da run |
| **Status** | Feito (tiles + Acólito + Senhor das Runas + runas/pedestais) |

---

## Ordem de implementação sugerida

1. **Infra multi-andar** — feito: portal após boss depth &lt; 5; `RunFactory.AdvanceFloor`; `Won` só no depth 5.
2. **Andar 2** — tileset + 1 inimigo + boss Necromante + tema musical.
3. **Andar 3** — tileset + Rato + Rei Lodo + hazards de slow.
4. **Andar 4** — Guarda + Golem + armadilhas.
5. **Andar 5** — bioma final + Senhor das Runas + tema de boss exclusivo.
6. **Polimento** — HP/dano por depth e pesos de raridade (feito). Falta stingers de boss por bioma.

## Fora de escopo (por enquanto)

- Depth infinito / endless
- Andares secretos / side floors
- Multiplayer / co-op
