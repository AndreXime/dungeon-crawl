# Sprite Cohesion (Isaac Style) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Regenerar os 24 sprites do Dungeon Crawl num kit visual único estilo Binding of Isaac / dungeon clássico, com tiles/itens em Canvas compartilhado e personagens via GenerateImage.

**Architecture:** Extrair paleta + helpers Canvas num módulo seed reutilizável. Reescrever `SpriteSources/**/*.json` (mode canvas) para tiles e itens. Gerar personagens com GenerateImage (prompt + paleta fixos), limpar fundo, nearest-neighbor para 32/64, gravar PNG + fonte no studio. Bake final e atualizar `SPRITE_STYLE.md`.

**Tech Stack:** Node Sprite Studio (`tools/sprite-studio`), Canvas seeds JSON, Cursor `GenerateImage`, ImageMagick/`magick` opcional para resize point, PNG em `ChamberCrawl/Assets/Textures/`.

## Global Constraints

- Spec: `docs/superpowers/specs/2026-10-23-sprite-cohesion-isaac-design.md`
- Outline `#0d0a08`, luz TL, sem AA/halo, sem cyan Diskette
- Paleta só da tabela da spec (≤16 cores/sprite)
- Tamanhos: tiles/player/bolt/bat/skeleton/slime = 32; items/coin/heart/ogre/projectile/portal = 64
- Bake: `node tools/sprite-studio/server.js --bake` → 24/24
- Sem mudanças de mecânica C# (só arte + guideline)
- Commits Conventional Commits em português

## File map

| File | Role |
|------|------|
| `tools/sprite-studio/palette.js` | Hex nomeados + helpers documentados (ou embutidos nos JSON) |
| `ChamberCrawl/Assets/SpriteSources/tiles/*.json` | Canvas tiles |
| `ChamberCrawl/Assets/SpriteSources/items/*.json` | Canvas itens/HUD |
| `ChamberCrawl/Assets/SpriteSources/player/*.json` | player + bolt (pós GenerateImage ou canvas amostrado) |
| `ChamberCrawl/Assets/SpriteSources/enemies/*.json` | inimigos + projectile |
| `ChamberCrawl/Assets/Textures/**/*.png` | PNGs baked |
| `docs/SPRITE_STYLE.md` | Kit Isaac + paleta |

---

### Task 1: Paleta + guideline

**Files:**
- Create: `tools/sprite-studio/palette.md` (tabela hex + regras; referência humana/agente)
- Modify: `docs/SPRITE_STYLE.md`

**Interfaces:**
- Produces: documento com todos os hex da spec copiados verbatim
- Produces: `SPRITE_STYLE.md` atualizado (temática Isaac, paleta, checklist)

- [ ] **Step 1: Escrever `tools/sprite-studio/palette.md`**

Copiar a tabela de paleta da spec + regras de outline/luz/fundo. Incluir bloco copy-paste para prompts GenerateImage:

```text
native NNxNN pixel art, Binding of Isaac inspired dungeon crawler style, hard pixel edges, no anti-aliasing, no blur, selective black outline (#0d0a08), top-left lighting, limited palette from dungeon kit, transparent background
```

- [ ] **Step 2: Atualizar `docs/SPRITE_STYLE.md`**

Substituir seção temática Diskette residual se houver. Referência positiva deixa de ser só coin antigo: “após Task 2, coin regenerado é a âncora”. Checklist: mesma família outline/luz/paleta.

- [ ] **Step 3: Commit**

```bash
git add tools/sprite-studio/palette.md docs/SPRITE_STYLE.md
git commit -m "$(cat <<'EOF'
docs: paleta e guideline estilo Isaac

EOF
)"
```

---

### Task 2: Canvas tiles (9)

**Files:**
- Modify: `ChamberCrawl/Assets/SpriteSources/tiles/{floor,wall,door,door-tile,crate,barrel,spike,obstacle,portal}.json`
- Modify: baked PNGs via bake

**Interfaces:**
- Consumes: paleta de `palette.md`
- Produces: 9 fontes canvas + PNGs após bake parcial

Cada JSON: `mode: "canvas"`, `size` 32 (portal 64), `canvasCode` com:

```javascript
function draw(ctx, size) {
  ctx.imageSmoothingEnabled = false;
  const O = '#0d0a08', P1 = '#2a2620', P2 = '#4a443c', P3 = '#6a6358';
  const W1 = '#3d2814', W2 = '#8b5a2b', W3 = '#c4a06a', Moss = '#3d5a3d';
  const px = (x, y, w, h, c) => { ctx.fillStyle = c; ctx.fillRect(x|0, y|0, w|0, h|0); };
  // ... silhueta Isaac do tile ...
}
```

Requisitos por tile:
- `floor`: pedra quadriculada suave + 2–3 pixels musgo
- `wall`: pedra mais escura, tijolos/fendas legíveis
- `door` / `door-tile`: madeira + ferro, tranca amarela opcional quando locked (sprite door)
- `crate`: caixa madeira com ripas (não baú genérico)
- `barrel`: cilindro madeira + aro metal
- `spike`: pontas metálicas no chão
- `obstacle`: bloco pedra genérico
- `portal`: arco pedra + glow magia `#c45cff` (64)

- [ ] **Step 1: Reescrever os 9 JSON canvas**

- [ ] **Step 2: Bake só tiles**

```bash
node tools/sprite-studio/server.js --bake
```

Expected: tiles ok (outros ids também bakeiam; ok).

- [ ] **Step 3: Verificação visual**

Abrir `floor.png`, `wall.png`, `crate.png`, `barrel.png`, `spike.png` no editor. Checklist: outline, luz TL, sem cyan, legíveis a 32px.

- [ ] **Step 4: Commit** `feat: tiles dungeon estilo Isaac`

---

### Task 3: Canvas itens + HUD (8)

**Files:**
- Modify: `SpriteSources/items/{coin,heart,item-damage,item-speed,item-hp,item-firerate,item-triple,item-greedy}.json`
- Bake PNGs 64×64

**Interfaces:**
- Consumes: mesma paleta
- Produces: coin âncora do set; heart claramente coração

Desenhos alvo:
- `coin`: círculo ouro + cruz/estrela central (manter espírito do coin bom, alinhar outline `#0d0a08`)
- `heart`: coração vermelho clássico Isaac (não slime)
- `item-damage`: lâmina/espada curta
- `item-speed`: botas
- `item-hp`: poção vermelha
- `item-firerate`: pergaminho
- `item-triple`: tridente
- `item-greedy`: ímã dourado

- [ ] **Step 1: JSON canvas 64×64**

- [ ] **Step 2: Bake + inspeção visual** (heart vs slime antigo; coin ao lado dos itens)

- [ ] **Step 3: Commit** `feat: itens e HUD estilo Isaac`

---

### Task 4: Personagens via GenerateImage (7)

**Files:**
- Create/overwrite: `Textures/player/player.png`, `bolt.png`
- Create/overwrite: `Textures/enemies/enemy-bat.png`, `enemy-skeleton.png`, `enemy-slime.png`, `enemy-ogre.png`, `projectile.png`
- Update: `SpriteSources` correspondentes (após limpeza: mode `pixels` amostrado ou canvas simplificado)

**Interfaces:**
- Consumes: prompt fixo de `palette.md` + referência `Textures/items/coin.png` (pós Task 3)
- Produces: 7 PNGs nativos 32 ou 64

Sujetos:
| Id | Size | Prompt sujeito |
|----|------|----------------|
| player | 32 | front-facing chubby dungeon mage, purple robe, staff with purple gem, Isaac-like proportions |
| bolt | 32 | small purple magic bolt / tear projectile |
| enemy-bat | 32 | cute menacing dungeon bat, red eyes |
| enemy-skeleton | 32 | dungeon skeleton with bone cream colors |
| enemy-slime | 32 | green blob slime, highlight TL |
| enemy-ogre | 64 | large green ogre boss, red eyes, Isaac boss silhouette |
| projectile | 64 | enemy magic orb purple/red |

- [ ] **Step 1: GenerateImage** para cada id (`aspect_ratio` 1:1). Anexar `coin.png` como `reference_image_paths` quando a ferramenta permitir.

- [ ] **Step 2: Pós-processo**

Para cada PNG gerado:

```bash
# Exemplo 32x32 (ajustar path)
magick INPUT -filter point -resize 32x32! -background none -flatten OUT.png
# Remover fundo preto/branco se necessário (floodfill / script studio)
```

Se `magick` ausente: usar canvas no studio amostrando ou Node script existente do bake. Garantir alpha 0 no fundo.

- [ ] **Step 3: Copiar para `Textures/...` e atualizar `SpriteSources`**

Preferência: salvar como `mode: "pixels"` no JSON após amostrar, para edição no studio.

- [ ] **Step 4: Inspeção** — Read tool nas 7 imagens. Rejeitar e regenerar se ilegível, cyan, ou fora da família.

- [ ] **Step 5: Commit** `feat: personagens dungeon estilo Isaac`

---

### Task 5: Bake final + checklist + regressão

**Files:**
- All `Textures/**` via bake
- Optionally touch `palette.md` notes

- [ ] **Step 1: Bake completo**

```bash
node tools/sprite-studio/server.js --bake
```

Expected: `baked 24/24 (all ok)`.

- [ ] **Step 2: Checklist da spec**

Montar grade mental / abrir:
- player + bat + skeleton + slime + ogre
- floor + wall + crate + barrel
- coin + heart + 2 items

Checklist:
- [ ] Sem cyan Diskette
- [ ] Heart = coração
- [ ] Crate = caixa madeira
- [ ] Mesma outline/luz
- [ ] Tamanhos corretos (`identify` ou `file`)

- [ ] **Step 3: Testes**

```bash
dotnet test --nologo
```

Expected: 104 passed (sem mudança C#).

- [ ] **Step 4: Smoke** `dotnet run --project ChamberCrawl` (menu DUNGEON CRAWL, sprites novos na sala).

- [ ] **Step 5: Commit** se houver ajustes pós-checklist `chore: bake final sprites Isaac` ou incluir no último commit de arte.

---

## Self-review (plan vs spec)

| Spec | Task |
|------|------|
| Kit outline/luz/paleta | 1 |
| Tiles Canvas | 2 |
| Itens/HUD Canvas | 3 |
| Personagens GenerateImage | 4 |
| Bake 24/24 + critérios sucesso | 5 |
| SPRITE_STYLE | 1 |
| Sem mecânica C# | respeitado |

Sem TBD. Parallelização possível: Task 4 (GenerateImage) pode começar após Task 1 (paleta), em paralelo com Tasks 2–3 se worktrees isolados; merge só após bake conjunto na Task 5.
