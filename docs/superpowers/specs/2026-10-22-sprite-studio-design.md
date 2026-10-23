# Sprite Studio local — Design

Data: 2026-10-22  
Status: aprovado (conversa)  
Escopo v1: inimigos em `ChamberCrawl/Assets/Textures/enemies/`

## Problema

Queremos redesenhar os sprites do Diskette (começando pelos inimigos) com um editor de pixels no browser, sem pipeline externo. O AI esboça melhor em **código Canvas** do que em matriz JSON. Depois da edição manual, a fonte da verdade passa a ser a grade de pixels + PNG no disco do jogo.

## Decisões

| Tema | Escolha |
|------|---------|
| Fonte editável | Pasta espelhada `Assets/SpriteSources/` (opção B) |
| Esboço inicial | Campo `canvasCode` no JSON |
| Após save manual | Descarta `canvasCode`; fica só `pixels` + PNG (opção B) |
| Server | Um único `server.js` com `node:http` + `node:fs` (sem npm) |
| HTML | CDN permitido (Tailwind / Font Awesome ok) |
| Abordagem | Studio mínimo em `tools/sprite-studio/` |

## Layout no disco

```
tools/sprite-studio/
  server.js
  index.html

ChamberCrawl/Assets/Textures/enemies/<name>.png
ChamberCrawl/Assets/SpriteSources/enemies/<name>.json
```

Espelho 1:1 pelo caminho relativo sob `enemies/` (e, em versões futuras, outras pastas de `Textures/`).

### Inventário v1 (enemies)

| PNG | Size nativo alvo |
|-----|------------------|
| `enemy-pspam.png` | 32 |
| `enemy-trojan.png` | 32 |
| `enemy-trojan-active.png` | 32 |
| `enemy-adware-king.png` | 64 |
| `projectile.png` | 64 |

Tamanhos seguem `docs/SPRITE_STYLE.md` (inimigos 32; boss/projétil 64).

## Schema JSON

### Modo canvas (esboço)

```json
{
  "size": 32,
  "mode": "canvas",
  "canvasCode": "function draw(ctx, size) {\n  // fillRect pixel-aligned; imageSmoothingEnabled = false\n}\n"
}
```

Contrato de `canvasCode`:

- Deve definir `function draw(ctx, size)` (ou atribuir `draw` no escopo avaliado).
- `ctx` é 2D de um canvas offscreen `size×size`.
- Desenho em coordenadas nativas (`0..size-1`); sem anti-alias (`imageSmoothingEnabled = false`).
- Fundo transparente (não preencher o canvas inteiro com opaco).

### Modo pixels (pós-edição)

```json
{
  "size": 32,
  "mode": "pixels",
  "pixels": [
    [null, "#335d92", "..."],
    ["..."]
  ]
}
```

- `pixels` é matriz `size×size`.
- Célula: `null` (transparente) ou string `#rrggbb` (opaco).
- No save a partir do editor de pixels: **remover** `canvasCode` se existir.

## Server (`tools/sprite-studio/server.js`)

- Bind: `127.0.0.1:3847` (localhost only).
- Deps: somente `node:http`, `node:fs`, `node:path` (built-ins).
- Serve estático: `GET /` → `index.html`, e assets estáticos do studio se necessário.
- Raiz de assets configurável por constante relativa ao repo:
  - `TEXTURES = ChamberCrawl/Assets/Textures`
  - `SOURCES = ChamberCrawl/Assets/SpriteSources`

### API

#### `GET /api/sprites`

Lista sprites sob `Textures/enemies/` (recursivo raso: só essa pasta na v1).

Resposta exemplo:

```json
{
  "sprites": [
    {
      "id": "enemies/enemy-pspam",
      "pngPath": "enemies/enemy-pspam.png",
      "jsonPath": "enemies/enemy-pspam.json",
      "size": 32,
      "mode": "canvas",
      "hasSource": true
    }
  ]
}
```

Se o PNG existe e o JSON não: `hasSource: false`, `mode`/`size` inferidos por tabela de defaults (acima) ou lidos do PNG (width) quando possível. Na v1, defaults por nome + fallback 32.

#### `GET /api/sprites/:id`

`id` = caminho relativo sem extensão, ex. `enemies/enemy-pspam` (URL-encoded).

- Se JSON existe: retorna o objeto fonte.
- Se não existe: **não cria no disco ainda**; retorna stub em memória:

```json
{
  "size": 32,
  "mode": "canvas",
  "canvasCode": "function draw(ctx, size) {\n  ctx.imageSmoothingEnabled = false;\n}\n",
  "stub": true
}
```

O stub real dos inimigos será preenchido na implementação (esboços Canvas por inimigo) ou gerado vazio até o AI/commit de fontes.

#### `PUT /api/sprites/:id`

Body JSON:

```json
{
  "size": 32,
  "mode": "pixels",
  "pixels": [[null, "#ff0000"]],
  "pngBase64": "<base64 sem prefixo data-url, PNG>"
}
```

ou, enquanto ainda em esboço (raro via UI, útil para seed):

```json
{
  "size": 32,
  "mode": "canvas",
  "canvasCode": "function draw(ctx, size) { ... }"
}
```

Comportamento:

1. Valida `id` (não pode sair de `enemies/` na v1; rejeitar `..`).
2. Garante diretórios `SpriteSources/enemies` e `Textures/enemies`.
3. Se `mode === "pixels"`: grava JSON **sem** `canvasCode`; exige `pixels` e `pngBase64`.
4. Se `mode === "canvas"`: grava JSON com `canvasCode`; se `pngBase64` vier, também grava PNG (seed visual).
5. Escreve PNG via `Buffer.from(pngBase64, 'base64')` em `Textures/...`.
6. Resposta `{ ok: true }`.

Erros: `400` validação, `404` id desconhecido (PNG não listável), `500` FS.

## Cliente (`index.html`)

Base no editor atual (`sprite_generator_sheet_exporter.html`):

- Ferramentas: paint, erase, pick, undo, grade, paleta Diskette.
- Sidebar: lista de sprites da API; ao clicar, carrega.
- Badge `canvas` / `pixels`.
- Load `canvas`: `eval`/Function sandboxed o suficiente para rodar `draw` em offscreen; amostrar RGBA → matriz `pixels` **em memória** (ainda `mode` canvas até o primeiro autosave de pixels? Ver regra abaixo).
- Autosave debounce ~400ms após mudança na grade: sempre envia `mode: "pixels"` + `pixels` + `pngBase64` → isso **promove** o sprite para pixels e descarta canvas no disco.
- Status “Salvando…” / “Salvo” / erro.
- Sem `localStorage` como fonte da verdade (opcional: cache de UI only). CDN ok.

### Regra de promoção canvas → pixels

- Abrir sprite `mode: canvas`: rasteriza para a grade editável; UI mostra badge canvas.
- Qualquer edição (ou botão “Salvar agora”) dispara PUT em `pixels` → JSON no disco perde `canvasCode`.
- Não há “Reset ao esboço Canvas” na v1 (decisão B).

## Esboços iniciais dos inimigos

Na implementação, criar JSONs `mode: canvas` com `draw` temáticos (estilo `SPRITE_STYLE.md`, paleta curta, bordas duras) para os 5 arquivos da tabela. O humano refine no editor e autosave gera PNG + pixels.

## Fora de escopo (v1)

- Editar `player/`, `items/`, `tiles/` (só estrutura pronta para estender a lista de pastas).
- Importar PNG existente para pixels (não necessário se todos os enemies forem reescritos via canvas).
- Autenticação, bind em 0.0.0.0, HTTPS.
- Empacotar no build MonoGame além de sobrescrever os PNGs já referenciados.
- Commit automático git.

## Como rodar

```bash
node tools/sprite-studio/server.js
# abrir http://127.0.0.1:3847
```

## Critérios de sucesso

1. Server sobe só com Node built-in.
2. Lista os 5 enemies; abre cada um.
3. Esboço canvas aparece na grade; editar e esperar autosave atualiza `SpriteSources/enemies/*.json` (pixels) e `Textures/enemies/*.png`.
4. Reload da página recarrega do JSON pixels (não do canvas).
5. Jogo continua a carregar os mesmos paths de textura.
