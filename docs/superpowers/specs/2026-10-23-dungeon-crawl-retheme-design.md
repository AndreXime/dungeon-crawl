# Design: Dungeon Crawl (retema medieval completo)

Data: 2026-10-23  
Status: aprovado em brainstorming  
Projeto técnico: `ChamberCrawl` (MonoGame DesktopGL)  
Nome de exibição: **Dungeon Crawl**  
Substitui a fatia jogável de `docs/superpowers/specs/2026-10-22-diskette-theme-design.md` (Partition C / Diskette).

## Objetivo

Trocar a temática PC/Diskette por um dungeon crawler medieval clássico, com rename de domínio no código, novas AIs (chase / shoot / multiply + boss em 2 fases) e sprites via Sprite Studio. O loop de jogo permanece: um andar, salas procedurais, loja, itens, vitória no boss sem portal.

## Decisões de produto

| Tema | Escolha |
|------|---------|
| Escopo | Retheme completo no domínio (abordagem 2): cosmético + mecânicas de AI |
| Nome | UI / janela / README = **Dungeon Crawl**; pasta e namespaces = `ChamberCrawl` |
| Andar | Um andar jogável (sem portal / sem `AdvanceFloor`) |
| Inimigos | Bat (chase), Skeleton (shoot), Slime (multiply) |
| Boss | Ogre: fase 1 chase, fase 2 shoot |
| Obstáculos | crate (destruível), barrel (sólido), spike (sólido + bloqueia projéteis) |
| Projétil do jogador | bolt mágico (substitui Bits `0`/`1`) |
| Player | mage |
| Itens (nomes clássicos) | potion, boots, scroll, coin, heart (+ itens de loja com nomes medievais) |
| Arte | Canvas no Sprite Studio → bake PNG; guideline em `docs/SPRITE_STYLE.md` atualizada para o tema |

## Mapeamento domínio

| Papel | Antes (Diskette) | Depois (Dungeon Crawl) |
|-------|------------------|------------------------|
| Branding | Diskette | Dungeon Crawl |
| Andar | Partition C | Dungeon (depth 1) |
| Player | Diskette | Mage |
| Projétil jogador | Bits (`bit-0` / `bit-1`) | Bolt (`bolt`) |
| Multiply | Pspam | Slime |
| Chase | Trojan (idle/charge/chase) | Bat (só persegue) |
| Shoot | (não havia no pool) | Skeleton |
| Boss | Adware King (3 padrões) | Ogre (2 fases: chase → shoot) |
| Tile destruível | Folder | Crate |
| Tile sólido | Popup | Barrel |
| Tile sólido + para tiro | BadSector | Spike |
| Pickup HP visual | heart | heart |
| Pickup moeda | coin | coin |

Constantes de tile: renomear `TileFolder` → `TileCrate`, `TilePopup` → `TileBarrel`, `TileBadSector` → `TileSpike` (valores numéricos 4/5/6 podem permanecer).

`EnemyKind`: `{ Bat, Skeleton, Slime, Ogre }`. Remover `Pspam`, `Trojan`, `AdwareKing`.

## Comportamento (baseline)

Valores iniciais, ajustáveis em playtest. Preferir reutilizar `Phase` / `Cooldown` / `CanSplit` / `IframeUntil` já existentes.

### Bat (chase)

- HP 3, speed ~70, raio ~10, dano contato 1.
- Sem tiro. Sempre persegue o jogador (sem idle / sem charge).
- Spawn: qualquer tile livre na sala normal (sem regra de canto).

### Skeleton (shoot)

- HP 3, speed ~40, raio ~10, dano contato 1.
- Mantém distância preferida (~90 px): aproxima se longe, recua se perto demais.
- Atira no jogador a cada ~1200 ms, projétil speed ~160, dano 1.
- Textura de projétil inimigo: `projectile` (pode tintar).

### Slime (multiply)

- HP 2 (filho: 1), speed ~50, raio ~9 (filho ~70% do pai).
- Sem tiro. Contato: 1 dano.
- A cada **4 s** com HP > 0 e `CanSplit`: remove-se e spawna **2** filhos.
- Filhos `CanSplit = false` (no máximo uma geração).
- Filhos contam como inimigos vivos para limpeza de sala.

### Ogre (boss, 2 fases)

- HP ~40, speed chase ~90, raio ~24, dano contato 2.
- **Fase 1 (Phase 0):** persegue o jogador. Ao chegar a ≤50% HP (ou após ~8 s na fase 1, o que ocorrer primeiro), entra na fase 2.
- **Fase 2 (Phase 1):** para de chase contínuo; atira rajadas no jogador a cada ~1000–1200 ms (1–3 projéteis em leque leve ou tiro único; preferir tiro único mirado para simplicidade). Pode andar lento (~45) entre tiros.
- Sem spawn de adds, sem resize, sem onda de choque (remover padrões do Adware King).
- Vitória: sala do boss limpa → `RunStatus.Won` (sem portal).

### Obstáculos

| Tile | Sólido corpo | Projétil jogador | Projétil inimigo | Destruível |
|------|--------------|------------------|------------------|------------|
| Crate | Sim | Para e vira floor | Para, crate permanece | Só tiro do jogador |
| Barrel | Sim | Para | Para | Não |
| Spike | Sim | Para | Para | Não |

`TileObstacle` genérico pode permanecer para compatibilidade; RoomGen do dungeon usa só crate/barrel/spike.

### Itens (loja / drops)

Manter os 6 IDs mecânicos. Atualizar `Name` + texturas para tema medieval:

| Id | Nome exibido | Textura (arquivo) |
|----|--------------|-------------------|
| `damage_up` | Lâmina | `item-damage` |
| `speed_up` | Botas | `item-speed` |
| `hp_up` | Poção | `item-hp` |
| `fire_rate` | Pergaminho | `item-firerate` |
| `triple` | Tridente | `item-triple` |
| `greedy` | Ímã | `item-greedy` |

Pickups soltos: `coin`, `heart` (mesmos ids de textura). HUD/prompts usam os novos nomes.

## Arquitetura (mudanças)

Camadas atuais preservadas. Substituições pontuais:

| Área | Mudança |
|------|---------|
| `Models/GameTypes.cs` | `EnemyKind` Bat/Skeleton/Slime/Ogre; `BitGlyph` no projétil pode virar flag única ou sumir (sempre `bolt`) |
| `Models/Constants.cs` | `TileCrate` / `TileBarrel` / `TileSpike` |
| `Data/Enemies.cs` | Defs + nomes de textura novos |
| `Data/Items.cs` | Nomes medievais |
| `Data/FloorThemes.cs` | Lookup crate/barrel/spike |
| `World/RoomGen.cs` | Pool Bat/Skeleton/Slime; boss Ogre; obstáculos dungeon |
| `Systems/Ai.cs` | `UpdateBat`, `UpdateSkeleton`, `UpdateSlime`, `UpdateOgre` (remover Pspam/Trojan/AdwareKing) |
| `Systems/Combat.cs` | Destruir crate (ex-folder) |
| `Systems/Collision.cs` | Sólidos nos novos nomes de tile |
| `Systems/RunFactory.cs` | `CanSplit` só Slime; cooldowns iniciais por kind |
| `Systems/Progression.cs` | Mantém vitória depth 1 sem portal |
| `Rendering/*` | Texturas mage/bolt/inimigos/tiles; título |
| `Game1.cs` | `Window.Title = "Dungeon Crawl"` |
| `README.md`, `docs/SPRITE_STYLE.md` | Copy e guideline medievais |
| `Assets/SpriteSources` + `Textures` | Renomear/substituir fontes e PNGs |
| `tools/sprite-studio` | Paths/títulos alinhados aos novos ids de sprite |

Estado extra do Ogre: `Phase` 0 = chase, 1 = shoot; HP threshold e/ou timer em `Cooldown`.

## Arte

Pipeline: editar/gerar Canvas no Sprite Studio → autosave `SpriteSources/**/*.json` → bake `Textures/**/*.png`.

Tamanhos (`SPRITE_STYLE`): tiles/player/bolt 32×32; itens/coin/heart/projétil inimigo/boss 64×64. Pixel art nativo, fundo transparente, paleta curta.

### Lista de texturas (ids finais)

**Player:** `player`, `bolt` (remove `bit-0`, `bit-1`)  
**Tiles:** `floor`, `wall`, `door`, `door-tile`, `crate`, `barrel`, `spike`, `obstacle` (opcional), `portal` (pode ficar unused)  
**Enemies:** `enemy-bat`, `enemy-skeleton`, `enemy-slime`, `enemy-ogre`, `projectile`  
**Items:** `coin`, `heart`, `item-damage`, `item-speed`, `item-hp`, `item-firerate`, `item-triple`, `item-greedy`

Arquivos antigos (`folder`, `popup`, `bad-sector`, `enemy-pspam`, …, `bit-0`, `bit-1`) devem ser removidos ou renomeados no mesmo PR de arte para não deixar ids órfãos no atlas.

Paleta alvo: pedra cinza-quente, madeira, musgo, tochas âmbar, sangue/roxo suave no slime. Evitar estética Win95 / neon PC.

## Testes

Atualizar / reescrever testes xUnit existentes:

- `RoomGen`: pool só Bat/Skeleton/Slime; boss = Ogre; tiles crate/barrel/spike em salas normais.
- `Combat`: tiro do jogador em crate → floor; em spike → remove projétil, tile permanece; tiro inimigo não destrói crate.
- `Ai`: Bat persegue; Skeleton dispara após cooldown; Slime split após 4 s e filhos não splitam; Ogre troca para fase shoot em ≤50% HP.
- `Progression`: boss limpo → `Won` sem portal.
- `FloorThemes` / `Collision`: ids crate/barrel/spike.
- Regressão: loja, itens claimed, i-frames, portas, menu.

## Fora de escopo

- Depths 2+ e portal entre andares.
- Rename de pasta/namespaces `ChamberCrawl`.
- Inventário novo, bombas, itens ativos.
- Áudio.
- 3ª fase de boss / adds do Ogre.
- Redesign de tamanho de sala ou algoritmo de `FloorGen`.
- Multiplayer.

## Critérios de sucesso

1. Título e copy visíveis dizem **Dungeon Crawl** (sem Diskette / Partition C / Pspam / Adware King).
2. Run completa no dungeon até Ogre → vitória.
3. Bat chase, Skeleton shoot, Slime multiply, Ogre 2 fases jogáveis conforme baseline.
4. Obstáculos crate/barrel/spike com a tabela de colisão acima.
5. Sprites medievais baked via Sprite Studio; `dotnet test` verde; `dotnet run` jogável no Linux.

## Referência

- Spec Diskette (substituída na fatia jogável): `docs/superpowers/specs/2026-10-22-diskette-theme-design.md`
- Sprite Studio: `docs/superpowers/specs/2026-10-22-sprite-studio-design.md`
- Guideline: `docs/SPRITE_STYLE.md`
- Brief: conversa 2026-10-23 (opções C / C / B / C / A / A / B + aprovação abordagem 2)
