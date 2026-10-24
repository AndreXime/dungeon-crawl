# Roadmap de andares

Cada andar precisa ter **identidade própria**: bioma, inimigos, boss, tiles, música e ritmo de combate. Não basta recolorir o mesmo mapa.

Hoje: **só depth 1**. Vitória no boss encerra a run. Próximo passo estrutural: após o boss, portal/saída → depth+1 (manter stats/inventário/moedas).

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
| **Tiles** | floor / wall atuais |
| **Inimigos** | Morcego, Esqueleto, Slime |
| **Boss** | Ogre (charge + projétil) |
| **Música** | `FirstFloorTheme` |
| **Status** | Feito |

---

## Andar 2 — Catacumbas

| | |
|---|---|
| **Clima** | Ossos, cinza frio, corredores estreitos |
| **Tiles** | Floor cinza-osso, wall com ossários, spike → “ossos pontudos” |
| **Inimigos** | Esqueleto (mais comum), Morcego; **novo: Espectro** (atravessa/pisca, frágil) |
| **Boss** | **Necromante** — invoca esqueletos fracos + orbes |
| **Obstáculos** | Pilhas de ossos (crate-like), urnas (barrel) |
| **Música** | `CatacombsTheme` (mais lento, coral/drone) |
| **Diff** | Salas um pouco maiores / +1 inimigo médio; raridade Rare um pouco mais frequente |

---

## Andar 3 — Esgotos

| | |
|---|---|
| **Clima** | Verde-limo, água rasa, escorregadio |
| **Tiles** | Floor musgo/água, wall tijolo úmido |
| **Inimigos** | Slime (dominante); **novo: Rato** (rápido, melee); Morcego |
| **Boss** | **Rei Lodo** — divide em 2 slimes médios ao “morrer” (1x), spit |
| **Obstáculos** | Poças (slow zone), grades, canos |
| **Música** | `SewersTheme` (baixo gordo, percussão molhada) |
| **Diff** | Slow zones; loot um pouco melhor na loja |

---

## Andar 4 — Arsenal

| | |
|---|---|
| **Clima** | Ferro, tochas, disciplina militar |
| **Tiles** | Floor laje escura, wall pedra+ferro |
| **Inimigos** | **novo: Guarda** (escudo / lento / tank); Esqueleto arqueiro; Slime raro |
| **Boss** | **Golem de Ferro** — fases: projéteis → charge → vulnerável |
| **Obstáculos** | Barricadas, armadilhas de lança (spike up), bigornas |
| **Música** | `ArmoryTheme` (tambores + metal) |
| **Diff** | Dano inimigo sobe; Common cai, Rare/Legendary sobem um pouco |

---

## Andar 5 — Santuário Sombrio (final)

| | |
|---|---|
| **Clima** | Roxo/carmesim, runas, ritual |
| **Tiles** | Floor runado, wall pedra negra + glow `#c45cff` |
| **Inimigos** | Espectro + Guarda remix; **novo: Acólito** (orbita e atira magia) |
| **Boss** | **Senhor das Runas** — arena com runas no chão (dano), summons, burst final |
| **Obstáculos** | Runas de dano, pedestais, brasas |
| **Música** | `SanctumTheme` (drone + lead grave) + boss theme dedicado |
| **Diff** | Matar o boss = vitória da run (tela VITORIA atual) |

---

## Ordem de implementação sugerida

1. **Infra multi-andar** — feito: portal após boss depth &lt; 5; `RunFactory.AdvanceFloor`; `Won` só no depth 5.
2. **Andar 2** — tileset + 1 inimigo + boss Necromante + tema musical.
3. **Andar 3** — tileset + Rato + Rei Lodo + hazards de slow.
4. **Andar 4** — Guarda + Golem + armadilhas.
5. **Andar 5** — bioma final + Senhor das Runas + tema de boss exclusivo.
6. **Polimento** — balanceamento de HP/dano por depth, pesos de raridade, stingers por bioma.

## Fora de escopo (por enquanto)

- Depth infinito / endless
- Andares secretos / side floors
- Multiplayer / co-op
