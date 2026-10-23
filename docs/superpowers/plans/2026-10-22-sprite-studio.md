# Sprite Studio local Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rodar um editor local (`tools/sprite-studio`) que lista inimigos, carrega esboço Canvas ou pixels, autosalva JSON em `SpriteSources/` e PNG em `Textures/`, só com Node built-in no server.

**Architecture:** `server.js` (`node:http` + `node:fs` + `node:path`) serve `index.html` e uma API REST mínima. Fontes em `ChamberCrawl/Assets/SpriteSources/enemies/*.json` espelham `Textures/enemies/*.png`. Modo `canvas` rasteriza no browser; o primeiro autosave promove para `mode: "pixels"` e remove `canvasCode`.

**Tech Stack:** Node.js built-ins; HTML/JS no browser com Tailwind/FA via CDN; Canvas 2D.

**Spec:** `docs/superpowers/specs/2026-10-22-sprite-studio-design.md`

## Global Constraints

- Server: apenas `node:http`, `node:fs`, `node:path` (sem `npm install`).
- Bind: `127.0.0.1:3847`.
- v1: só pasta `enemies/`; rejeitar path traversal e ids fora de `enemies/`.
- Save pixels: JSON **sem** `canvasCode`.
- CDN no HTML permitido.
- Commits: só se o usuário pedir explicitamente (não commit automático neste repo).

---

## File map

| File | Responsibility |
|------|----------------|
| `tools/sprite-studio/server.js` | HTTP server, estático, API sprites |
| `tools/sprite-studio/index.html` | Editor UI + autosave |
| `ChamberCrawl/Assets/SpriteSources/enemies/*.json` | Fontes (5 enemies, seed canvas) |
| `docs/SPRITE_STYLE.md` | Referência de estilo (não editar salvo se necessário citar) |

---

### Task 1: Server — estático + `GET /api/sprites`

**Files:**
- Create: `tools/sprite-studio/server.js`
- Create: `tools/sprite-studio/index.html` (placeholder mínimo “ok” até Task 3)

**Interfaces:**
- Produces: `listEnemySprites()` → `Array<{ id, pngPath, jsonPath, size, mode, hasSource }>`
- Produces: HTTP `GET /api/sprites` JSON `{ sprites: [...] }`
- Produces: `GET /` → `index.html`

- [ ] **Step 1: Criar `server.js` com paths e defaults**

```js
'use strict';
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');

const HOST = '127.0.0.1';
const PORT = 3847;
const ROOT = path.resolve(__dirname, '../..');
const TEXTURES = path.join(ROOT, 'ChamberCrawl/Assets/Textures');
const SOURCES = path.join(ROOT, 'ChamberCrawl/Assets/SpriteSources');
const STUDIO = __dirname;

const SIZE_DEFAULTS = {
  'enemies/enemy-pspam': 32,
  'enemies/enemy-trojan': 32,
  'enemies/enemy-trojan-active': 32,
  'enemies/enemy-adware-king': 64,
  'enemies/projectile': 64,
};

function safeEnemyId(id) {
  if (typeof id !== 'string') return null;
  const decoded = decodeURIComponent(id);
  if (!decoded.startsWith('enemies/')) return null;
  if (decoded.includes('..') || path.isAbsolute(decoded)) return null;
  if (!/^[a-z0-9/_-]+$/i.test(decoded)) return null;
  return decoded;
}
```

- [ ] **Step 2: Implementar listagem e handlers HTTP**

```js
function listEnemySprites() {
  const dir = path.join(TEXTURES, 'enemies');
  const names = fs.readdirSync(dir).filter((f) => f.endsWith('.png')).sort();
  return names.map((file) => {
    const id = `enemies/${file.replace(/\.png$/, '')}`;
    const jsonPath = path.join(SOURCES, id + '.json');
    let mode = 'canvas';
    let size = SIZE_DEFAULTS[id] || 32;
    let hasSource = false;
    if (fs.existsSync(jsonPath)) {
      hasSource = true;
      try {
        const doc = JSON.parse(fs.readFileSync(jsonPath, 'utf8'));
        if (doc.mode) mode = doc.mode;
        if (doc.size) size = doc.size;
      } catch (_) { /* keep defaults */ }
    }
    return {
      id,
      pngPath: `enemies/${file}`,
      jsonPath: `enemies/${file.replace(/\.png$/, '.json')}`,
      size,
      mode,
      hasSource,
    };
  });
}

function sendJson(res, status, body) {
  const data = JSON.stringify(body);
  res.writeHead(status, {
    'Content-Type': 'application/json; charset=utf-8',
    'Content-Length': Buffer.byteLength(data),
  });
  res.end(data);
}

function readBody(req) {
  return new Promise((resolve, reject) => {
    const chunks = [];
    req.on('data', (c) => chunks.push(c));
    req.on('end', () => resolve(Buffer.concat(chunks)));
    req.on('error', reject);
  });
}

const server = http.createServer(async (req, res) => {
  const url = new URL(req.url, `http://${HOST}:${PORT}`);
  try {
    if (req.method === 'GET' && url.pathname === '/api/sprites') {
      return sendJson(res, 200, { sprites: listEnemySprites() });
    }
    if (req.method === 'GET' && (url.pathname === '/' || url.pathname === '/index.html')) {
      const html = fs.readFileSync(path.join(STUDIO, 'index.html'));
      res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
      return res.end(html);
    }
    sendJson(res, 404, { error: 'not found' });
  } catch (err) {
    sendJson(res, 500, { error: String(err && err.message ? err.message : err) });
  }
});

server.listen(PORT, HOST, () => {
  console.log(`Sprite Studio http://${HOST}:${PORT}`);
});
```

- [ ] **Step 3: Placeholder `index.html`**

```html
<!DOCTYPE html>
<html lang="pt-BR"><head><meta charset="UTF-8"><title>Sprite Studio</title></head>
<body><p>Sprite Studio — aguardando editor</p></body></html>
```

- [ ] **Step 4: Verificar listagem**

Run:
```bash
node tools/sprite-studio/server.js &
sleep 0.5
curl -s http://127.0.0.1:3847/api/sprites | head -c 400
kill %1
```
Expected: JSON com `sprites` incluindo `enemies/enemy-pspam` e os demais PNGs de enemies.

---

### Task 2: Server — `GET` e `PUT /api/sprites/:id`

**Files:**
- Modify: `tools/sprite-studio/server.js`

**Interfaces:**
- Consumes: `safeEnemyId`, `SIZE_DEFAULTS`, `SOURCES`, `TEXTURES`
- Produces: `GET /api/sprites/<id>` → documento fonte ou stub `{ size, mode, canvasCode, stub?: true }`
- Produces: `PUT /api/sprites/<id>` body → grava JSON + PNG opcional; `{ ok: true }`

- [ ] **Step 1: Stub canvas vazio helper**

```js
function emptyCanvasStub(size) {
  return {
    size,
    mode: 'canvas',
    canvasCode:
      'function draw(ctx, size) {\n  ctx.imageSmoothingEnabled = false;\n}\n',
    stub: true,
  };
}
```

- [ ] **Step 2: GET por id**

No `createServer`, antes do 404:

```js
const getMatch = url.pathname.match(/^\/api\/sprites\/(.+)$/);
if (req.method === 'GET' && getMatch) {
  const id = safeEnemyId(getMatch[1]);
  if (!id) return sendJson(res, 400, { error: 'invalid id' });
  const png = path.join(TEXTURES, id + '.png');
  if (!fs.existsSync(png)) return sendJson(res, 404, { error: 'png missing' });
  const jsonPath = path.join(SOURCES, id + '.json');
  if (!fs.existsSync(jsonPath)) {
    return sendJson(res, 200, emptyCanvasStub(SIZE_DEFAULTS[id] || 32));
  }
  const doc = JSON.parse(fs.readFileSync(jsonPath, 'utf8'));
  return sendJson(res, 200, doc);
}
```

- [ ] **Step 3: PUT por id**

```js
if (req.method === 'PUT' && getMatch) {
  const id = safeEnemyId(getMatch[1]);
  if (!id) return sendJson(res, 400, { error: 'invalid id' });
  const pngPath = path.join(TEXTURES, id + '.png');
  if (!fs.existsSync(pngPath) && !fs.existsSync(path.dirname(pngPath))) {
    /* still allow write if folder exists */
  }
  if (!fs.existsSync(path.join(TEXTURES, 'enemies'))) {
    return sendJson(res, 404, { error: 'textures enemies missing' });
  }
  const raw = await readBody(req);
  let body;
  try { body = JSON.parse(raw.toString('utf8')); }
  catch (_) { return sendJson(res, 400, { error: 'invalid json' }); }

  const size = Number(body.size) || SIZE_DEFAULTS[id] || 32;
  const mode = body.mode;
  if (mode !== 'pixels' && mode !== 'canvas') {
    return sendJson(res, 400, { error: 'mode must be pixels|canvas' });
  }

  const outDir = path.join(SOURCES, path.dirname(id));
  fs.mkdirSync(outDir, { recursive: true });
  const jsonPath = path.join(SOURCES, id + '.json');

  if (mode === 'pixels') {
    if (!Array.isArray(body.pixels)) {
      return sendJson(res, 400, { error: 'pixels required' });
    }
    if (typeof body.pngBase64 !== 'string' || !body.pngBase64) {
      return sendJson(res, 400, { error: 'pngBase64 required' });
    }
    const doc = { size, mode: 'pixels', pixels: body.pixels };
    fs.writeFileSync(jsonPath, JSON.stringify(doc, null, 2) + '\n', 'utf8');
    fs.writeFileSync(pngPath, Buffer.from(body.pngBase64, 'base64'));
    return sendJson(res, 200, { ok: true });
  }

  // mode canvas
  if (typeof body.canvasCode !== 'string') {
    return sendJson(res, 400, { error: 'canvasCode required' });
  }
  const doc = { size, mode: 'canvas', canvasCode: body.canvasCode };
  fs.writeFileSync(jsonPath, JSON.stringify(doc, null, 2) + '\n', 'utf8');
  if (typeof body.pngBase64 === 'string' && body.pngBase64) {
    fs.writeFileSync(pngPath, Buffer.from(body.pngBase64, 'base64'));
  }
  return sendJson(res, 200, { ok: true });
}
```

Nota: no GET/PUT, `getMatch` deve ser calculado uma vez no início do handler (`const getMatch = url.pathname.match(...)`).

- [ ] **Step 4: Testar PUT pixels e GET**

Run (com server up):
```bash
# cria JSON pixels mínimo 2x2 (só para testar API; depois seeds reais)
python3 - <<'PY'
import json,base64,struct,zlib,urllib.request
# 1x1 red png
def png1():
  # use tiny valid png
  return base64.b64encode(open('ChamberCrawl/Assets/Textures/enemies/projectile.png','rb').read()).decode()
body=json.dumps({"size":32,"mode":"pixels","pixels":[[None]*32 for _ in range(32)],"pngBase64":png1()}).encode()
req=urllib.request.Request('http://127.0.0.1:3847/api/sprites/enemies/enemy-pspam',data=body,method='PUT',headers={'Content-Type':'application/json'})
print(urllib.request.urlopen(req).read())
print(urllib.request.urlopen('http://127.0.0.1:3847/api/sprites/enemies/enemy-pspam').read()[:200])
PY
```
Expected: `{"ok":true}` e GET com `"mode":"pixels"` sem `canvasCode`.

- [ ] **Step 5: Remover o JSON/PNG de teste se sobrescreveu pspam** (restaurar png do git se necessário)

```bash
git checkout -- ChamberCrawl/Assets/Textures/enemies/enemy-pspam.png
rm -f ChamberCrawl/Assets/SpriteSources/enemies/enemy-pspam.json
```

---

### Task 3: Editor HTML — lista, load canvas/pixels, paint, autosave

**Files:**
- Replace: `tools/sprite-studio/index.html` (partir de `sprite_generator_sheet_exporter.html`, sem localStorage como fonte da verdade)

**Interfaces:**
- Consumes: `GET /api/sprites`, `GET /api/sprites/:id`, `PUT /api/sprites/:id`
- Produces: `loadSprite(id)`, `rasterizeCanvasCode(code, size) → pixels[][]`, `scheduleSave()`, `buildPngBase64(size)`

- [ ] **Step 1: UI shell**

Copiar estrutura do editor atual (toolbar, paleta, canvas 512, preview). Adicionar:

- `<select id="spriteSelect">` ou lista `#spriteList` na sidebar
- `#saveStatus` (“—”, “Salvando…”, “Salvo”, erro)
- `#modeBadge` (`canvas` / `pixels`)
- Remover botões “Exportar código” / “Reset Diskette” / download local **ou** manter download PNG como extra; autosave é o caminho principal.
- Manter CDN Tailwind/FA.
- Fontes: só system stack.

- [ ] **Step 2: Rasterizar `canvasCode`**

```js
function rasterizeCanvasCode(canvasCode, size) {
  const off = document.createElement('canvas');
  off.width = size;
  off.height = size;
  const ctx = off.getContext('2d', { willReadFrequently: true });
  ctx.imageSmoothingEnabled = false;
  ctx.clearRect(0, 0, size, size);
  const runner = new Function(`${canvasCode}\n; if (typeof draw !== 'function') throw new Error('draw() missing'); draw(ctx, size);`);
  runner();
  const { data } = ctx.getImageData(0, 0, size, size);
  const pixels = [];
  for (let y = 0; y < size; y++) {
    const row = [];
    for (let x = 0; x < size; x++) {
      const i = (y * size + x) * 4;
      const a = data[i + 3];
      row.push(a < 16 ? null : '#' + [data[i], data[i + 1], data[i + 2]].map((v) => v.toString(16).padStart(2, '0')).join(''));
    }
    pixels.push(row);
  }
  return pixels;
}
```

- [ ] **Step 3: Load / lista / estado**

```js
let currentId = null;
let nativeSize = 32;
let sourceMode = 'canvas'; // badge only until first pixel save
let pixels = [];
let saveTimer = null;

async function refreshList() {
  const { sprites } = await fetch('/api/sprites').then((r) => r.json());
  const list = document.getElementById('spriteList');
  list.innerHTML = '';
  sprites.forEach((s) => {
    const btn = document.createElement('button');
    btn.textContent = s.id.replace('enemies/', '');
    btn.dataset.id = s.id;
    btn.addEventListener('click', () => loadSprite(s.id));
    list.appendChild(btn);
  });
}

async function loadSprite(id) {
  currentId = id;
  const doc = await fetch('/api/sprites/' + encodeURIComponent(id)).then((r) => r.json());
  nativeSize = doc.size || 32;
  sourceMode = doc.mode;
  document.getElementById('modeBadge').textContent = sourceMode;
  if (doc.mode === 'pixels') {
    pixels = doc.pixels;
  } else {
    pixels = rasterizeCanvasCode(doc.canvasCode || '', nativeSize);
  }
  // resize editor zoom: editor canvas = nativeSize * 16
  const ZOOM = 16;
  editorCanvas.width = nativeSize * ZOOM;
  editorCanvas.height = nativeSize * ZOOM;
  previewNative.width = nativeSize;
  previewNative.height = nativeSize;
  redraw();
}
```

- [ ] **Step 4: Autosave debounce → PUT pixels**

```js
function buildPngBase64(size) {
  const off = document.createElement('canvas');
  off.width = size;
  off.height = size;
  const ctx = off.getContext('2d');
  ctx.imageSmoothingEnabled = false;
  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const hex = pixels[y][x];
      if (!hex) continue;
      ctx.fillStyle = hex;
      ctx.fillRect(x, y, 1, 1);
    }
  }
  return off.toDataURL('image/png').split(',')[1];
}

function scheduleSave() {
  if (!currentId) return;
  const el = document.getElementById('saveStatus');
  el.textContent = 'Alterado…';
  clearTimeout(saveTimer);
  saveTimer = setTimeout(persistPixels, 400);
}

async function persistPixels() {
  if (!currentId) return;
  const el = document.getElementById('saveStatus');
  el.textContent = 'Salvando…';
  const body = {
    size: nativeSize,
    mode: 'pixels',
    pixels,
    pngBase64: buildPngBase64(nativeSize),
  };
  const res = await fetch('/api/sprites/' + encodeURIComponent(currentId), {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    el.textContent = 'Erro ao salvar';
    return;
  }
  sourceMode = 'pixels';
  document.getElementById('modeBadge').textContent = 'pixels';
  el.textContent = 'Salvo';
}
```

Chamar `scheduleSave()` em toda mutação de pixel (após undo/paint), **não** no load puro.

- [ ] **Step 5: Portar paint/undo/paleta** do HTML atual, usando `nativeSize` e `ZOOM` dinâmicos (células `0..nativeSize-1`).

- [ ] **Step 6: Smoke manual**

Run:
```bash
node tools/sprite-studio/server.js
# abrir http://127.0.0.1:3847
# selecionar enemy-pspam (stub vazio ok), pintar 1 pixel, esperar "Salvo"
ls ChamberCrawl/Assets/SpriteSources/enemies/
file ChamberCrawl/Assets/Textures/enemies/enemy-pspam.png
```
Expected: JSON `mode: pixels` e PNG atualizado.

---

### Task 4: Seeds Canvas dos 5 inimigos

**Files:**
- Create: `ChamberCrawl/Assets/SpriteSources/enemies/enemy-pspam.json`
- Create: `ChamberCrawl/Assets/SpriteSources/enemies/enemy-trojan.json`
- Create: `ChamberCrawl/Assets/SpriteSources/enemies/enemy-trojan-active.json`
- Create: `ChamberCrawl/Assets/SpriteSources/enemies/enemy-adware-king.json`
- Create: `ChamberCrawl/Assets/SpriteSources/enemies/projectile.json`

**Interfaces:**
- Cada arquivo: `{ size, mode: "canvas", canvasCode }` com `function draw(ctx, size)`
- Estilo: `docs/SPRITE_STYLE.md` (bordas duras, paleta curta, transparente)

- [ ] **Step 1: Escrever seeds** (um `draw` distinto por inimigo)

Temas sugeridos:
- **pspam**: envelope/popup vermelho-amarelo com “!” 
- **trojan**: cavalo/caixa suspeita escura, olho
- **trojan-active**: mesma base + glow/alerta
- **adware-king** (64): janela Win95 + coroa
- **projectile** (64): bit/spark azul pequeno centrado

Exemplo mínimo de forma (expandir em cada arquivo):

```json
{
  "size": 32,
  "mode": "canvas",
  "canvasCode": "function draw(ctx, size) {\n  ctx.imageSmoothingEnabled = false;\n  const px = (x,y,w,h,c) => { ctx.fillStyle = c; ctx.fillRect(x,y,w,h); };\n  px(8, 10, 16, 12, '#c43c3c');\n  px(14, 13, 4, 6, '#f0e8d6');\n}\n"
}
```

Não gravar PNG ainda no seed (só JSON). PNG antigo permanece até o humano/autosave.

- [ ] **Step 2: Verificar load canvas no studio**

Run server, abrir cada sprite: badge `canvas`, silhueta aparece. Sem salvar, JSON no disco permanece `mode: canvas`.

- [ ] **Step 3: Documentar como rodar** no topo de `tools/sprite-studio/server.js` (comentário):

```js
// Uso: node tools/sprite-studio/server.js
// Abra http://127.0.0.1:3847
```

---

### Task 5: Verificação ponta a ponta + limpeza

**Files:**
- Possibly leave `sprite_generator_sheet_exporter.html` na raiz (legado); não apagar na v1 salvo pedido.

- [ ] **Step 1: Checklist**

1. `node tools/sprite-studio/server.js` sobe sem erros.
2. `GET /api/sprites` retorna 5 enemies.
3. Abrir `enemy-adware-king` → canvas 64×64 (editor 1024px).
4. Editar 1 pixel em `projectile` → autosave → JSON pixels sem `canvasCode` + PNG muda (`stat` / hash).
5. Reload página → `projectile` carrega `mode: pixels`.
6. Path traversal: `curl -s -o /dev/null -w '%{http_code}' http://127.0.0.1:3847/api/sprites/enemies%2F..%2Fplayer%2Fplayer` → `400`.

- [ ] **Step 2: Nota no spec ou README curto**

Se não existir README do studio, adicionar 5 linhas em comentário do server (já no Task 4) basta. Não criar markdown extra a menos que o usuário peça.

---

## Spec coverage (self-review)

| Spec item | Task |
|-----------|------|
| `server.js` http+fs only, :3847 | 1 |
| Espelho SpriteSources ↔ Textures | 2, 4 |
| GET list / GET id / PUT | 1–2 |
| Schema canvas vs pixels, drop canvasCode | 2, 3 |
| Editor + autosave | 3 |
| Seeds 5 enemies | 4 |
| CDN ok, sem npm | 1, 3 |
| Path safety enemies only | 2, 5 |

## Placeholder scan

Nenhum TBD/TODO deixado nos steps.
