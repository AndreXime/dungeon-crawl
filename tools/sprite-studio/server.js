// Uso: node tools/sprite-studio/server.js
// Abra http://127.0.0.1:3847
// Bake: node tools/sprite-studio/server.js --bake
'use strict';
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const zlib = require('node:zlib');

const HOST = '127.0.0.1';
const PORT = 3847;
const ROOT = path.resolve(__dirname, '../..');
const TEXTURES = path.join(ROOT, 'ChamberCrawl/Assets/Textures');
const SOURCES = path.join(ROOT, 'ChamberCrawl/Assets/SpriteSources');
const STUDIO = __dirname;

const FOLDERS = ['enemies', 'items', 'player', 'tiles'];

const SIZE_DEFAULTS = {
  'enemies/enemy-bat': 32,
  'enemies/enemy-skeleton': 32,
  'enemies/enemy-slime': 32,
  'enemies/enemy-specter': 32,
  'enemies/enemy-necromancer': 64,
  'enemies/enemy-rat': 32,
  'enemies/enemy-slime-king': 64,
  'enemies/enemy-ogre': 64,
  'enemies/projectile': 64,
  'items/coin': 64,
  'items/heart': 64,
  'items/item-damage': 64,
  'items/item-firerate': 64,
  'items/item-fortune': 64,
  'items/item-greedy': 64,
  'items/item-haste': 64,
  'items/item-hp': 64,
  'items/item-range': 64,
  'items/item-sharp': 64,
  'items/item-speed': 64,
  'items/item-triple': 64,
  'items/item-vigor': 64,
  'player/player': 32,
  'player/bolt': 32,
  'tiles/floor': 32,
  'tiles/wall': 32,
  'tiles/door': 32,
  'tiles/door-tile': 32,
  'tiles/floor-2': 32,
  'tiles/wall-2': 32,
  'tiles/door-2': 32,
  'tiles/door-tile-2': 32,
  'tiles/floor-3': 32,
  'tiles/wall-3': 32,
  'tiles/door-3': 32,
  'tiles/door-tile-3': 32,
  'tiles/crate': 32,
  'tiles/barrel': 32,
  'tiles/obstacle': 32,
  'tiles/spike': 32,
  'tiles/puddle': 32,
  'tiles/portal': 64,
};

function safeSpriteId(id) {
  if (typeof id !== 'string') return null;
  const decoded = decodeURIComponent(id);
  const folder = decoded.split('/')[0];
  if (!FOLDERS.includes(folder)) return null;
  if (decoded.includes('..') || path.isAbsolute(decoded)) return null;
  if (!/^[a-z0-9/_-]+$/i.test(decoded)) return null;
  return decoded;
}

function listSprites() {
  const out = [];
  for (const folder of FOLDERS) {
    const dir = path.join(TEXTURES, folder);
    if (!fs.existsSync(dir)) continue;
    const names = fs.readdirSync(dir).filter((f) => f.endsWith('.png')).sort();
    for (const file of names) {
      const id = `${folder}/${file.replace(/\.png$/, '')}`;
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
      out.push({
        id,
        pngPath: `${folder}/${file}`,
        jsonPath: `${folder}/${file.replace(/\.png$/, '.json')}`,
        size,
        mode,
        hasSource,
      });
    }
  }
  return out;
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

function emptyCanvasStub(size) {
  return {
    size,
    mode: 'canvas',
    canvasCode:
      'function draw(ctx, size) {\n  ctx.imageSmoothingEnabled = false;\n}\n',
    stub: true,
  };
}

function parseColor(style) {
  if (style == null) return [0, 0, 0, 0];
  const s = String(style).trim();
  if (s === 'transparent') return [0, 0, 0, 0];
  const m8 = /^#([0-9a-f]{8})$/i.exec(s);
  if (m8) {
    const n = parseInt(m8[1], 16);
    return [(n >>> 24) & 255, (n >>> 16) & 255, (n >>> 8) & 255, n & 255];
  }
  const m6 = /^#([0-9a-f]{6})$/i.exec(s);
  if (m6) {
    const n = parseInt(m6[1], 16);
    return [(n >>> 16) & 255, (n >>> 8) & 255, n & 255, 255];
  }
  const m3 = /^#([0-9a-f]{3})$/i.exec(s);
  if (m3) {
    const r = parseInt(m3[1][0] + m3[1][0], 16);
    const g = parseInt(m3[1][1] + m3[1][1], 16);
    const b = parseInt(m3[1][2] + m3[1][2], 16);
    return [r, g, b, 255];
  }
  return [0, 0, 0, 255];
}

function createRaster(size) {
  const data = new Uint8ClampedArray(size * size * 4);
  let fillStyle = '#000000';

  function put(x, y, rgba) {
    if (x < 0 || y < 0 || x >= size || y >= size) return;
    const i = (y * size + x) * 4;
    const srcA = rgba[3] / 255;
    if (srcA <= 0) {
      data[i] = 0;
      data[i + 1] = 0;
      data[i + 2] = 0;
      data[i + 3] = 0;
      return;
    }
    if (srcA >= 1) {
      data[i] = rgba[0];
      data[i + 1] = rgba[1];
      data[i + 2] = rgba[2];
      data[i + 3] = 255;
      return;
    }
    const dstA = data[i + 3] / 255;
    const outA = srcA + dstA * (1 - srcA);
    if (outA <= 0) {
      data[i] = data[i + 1] = data[i + 2] = data[i + 3] = 0;
      return;
    }
    data[i] = Math.round((rgba[0] * srcA + data[i] * dstA * (1 - srcA)) / outA);
    data[i + 1] = Math.round((rgba[1] * srcA + data[i + 1] * dstA * (1 - srcA)) / outA);
    data[i + 2] = Math.round((rgba[2] * srcA + data[i + 2] * dstA * (1 - srcA)) / outA);
    data[i + 3] = Math.round(outA * 255);
  }

  function fillRect(x, y, w, h) {
    const rgba = parseColor(fillStyle);
    const x0 = Math.floor(Number(x));
    const y0 = Math.floor(Number(y));
    const ww = Math.floor(Number(w));
    const hh = Math.floor(Number(h));
    for (let yy = y0; yy < y0 + hh; yy++) {
      for (let xx = x0; xx < x0 + ww; xx++) put(xx, yy, rgba);
    }
  }

  function clearRect(x, y, w, h) {
    const x0 = Math.floor(Number(x));
    const y0 = Math.floor(Number(y));
    const ww = Math.floor(Number(w));
    const hh = Math.floor(Number(h));
    for (let yy = y0; yy < y0 + hh; yy++) {
      for (let xx = x0; xx < x0 + ww; xx++) put(xx, yy, [0, 0, 0, 0]);
    }
  }

  const ctx = {
    imageSmoothingEnabled: false,
    get fillStyle() {
      return fillStyle;
    },
    set fillStyle(v) {
      fillStyle = v;
    },
    fillRect,
    clearRect,
  };

  return { ctx, data, size };
}

function encodePng(size, rgba) {
  const raw = Buffer.alloc((size * 4 + 1) * size);
  for (let y = 0; y < size; y++) {
    const rowStart = y * (size * 4 + 1);
    raw[rowStart] = 0;
    Buffer.from(rgba.buffer, rgba.byteOffset + y * size * 4, size * 4).copy(raw, rowStart + 1);
  }
  const compressed = zlib.deflateSync(raw, { level: 9 });

  function chunk(type, data) {
    const len = Buffer.alloc(4);
    len.writeUInt32BE(data.length, 0);
    const typeBuf = Buffer.from(type, 'ascii');
    const crc = Buffer.alloc(4);
    crc.writeUInt32BE(zlib.crc32(Buffer.concat([typeBuf, data])) >>> 0, 0);
    return Buffer.concat([len, typeBuf, data, crc]);
  }

  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(size, 0);
  ihdr.writeUInt32BE(size, 4);
  ihdr[8] = 8;
  ihdr[9] = 6;
  ihdr[10] = 0;
  ihdr[11] = 0;
  ihdr[12] = 0;

  return Buffer.concat([
    Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]),
    chunk('IHDR', ihdr),
    chunk('IDAT', compressed),
    chunk('IEND', Buffer.alloc(0)),
  ]);
}

function rasterizeCanvasCode(canvasCode, size) {
  const { ctx, data } = createRaster(size);
  const runner = new Function(
    'ctx',
    'size',
    `${canvasCode}\n; if (typeof draw !== 'function') throw new Error('draw() missing'); draw(ctx, size);`,
  );
  runner(ctx, size);
  return data;
}

function pixelsFromRgba(size, rgba) {
  const pixels = [];
  for (let y = 0; y < size; y++) {
    const row = [];
    for (let x = 0; x < size; x++) {
      const i = (y * size + x) * 4;
      if (rgba[i + 3] < 16) row.push(null);
      else {
        const hex =
          '#' +
          [rgba[i], rgba[i + 1], rgba[i + 2]]
            .map((v) => v.toString(16).padStart(2, '0'))
            .join('');
        row.push(hex);
      }
    }
    pixels.push(row);
  }
  return pixels;
}

function bakeAllSprites({ promotePixels = false } = {}) {
  const results = [];
  for (const folder of FOLDERS) {
    const dir = path.join(SOURCES, folder);
    if (!fs.existsSync(dir)) continue;
    const files = fs.readdirSync(dir).filter((f) => f.endsWith('.json')).sort();
    for (const file of files) {
      const id = `${folder}/${file.replace(/\.json$/, '')}`;
      const jsonPath = path.join(dir, file);
      const pngPath = path.join(TEXTURES, id + '.png');
      const doc = JSON.parse(fs.readFileSync(jsonPath, 'utf8'));
      const size = Number(doc.size) || SIZE_DEFAULTS[id] || 32;

      let rgba;
      if (doc.mode === 'canvas' && typeof doc.canvasCode === 'string') {
        rgba = rasterizeCanvasCode(doc.canvasCode, size);
      } else if (doc.mode === 'pixels' && Array.isArray(doc.pixels)) {
        rgba = new Uint8ClampedArray(size * size * 4);
        for (let y = 0; y < size; y++) {
          for (let x = 0; x < size; x++) {
            const hex = doc.pixels[y] && doc.pixels[y][x];
            const i = (y * size + x) * 4;
            if (!hex) continue;
            const [r, g, b, a] = parseColor(hex);
            rgba[i] = r;
            rgba[i + 1] = g;
            rgba[i + 2] = b;
            rgba[i + 3] = a;
          }
        }
      } else {
        results.push({ id, ok: false, error: 'unsupported mode' });
        continue;
      }

      fs.mkdirSync(path.dirname(pngPath), { recursive: true });
      fs.writeFileSync(pngPath, encodePng(size, rgba));

      if (promotePixels && doc.mode === 'canvas') {
        const next = { size, mode: 'pixels', pixels: pixelsFromRgba(size, rgba) };
        fs.writeFileSync(jsonPath, JSON.stringify(next, null, 2) + '\n', 'utf8');
      }

      results.push({ id, ok: true, size, mode: promotePixels ? 'pixels' : doc.mode });
    }
  }
  return results;
}

function startServer() {
  const server = http.createServer(async (req, res) => {
    const url = new URL(req.url, `http://${HOST}:${PORT}`);
    try {
      if (req.method === 'GET' && url.pathname === '/api/sprites') {
        return sendJson(res, 200, { sprites: listSprites() });
      }
      if (req.method === 'POST' && url.pathname === '/api/bake') {
        const promotePixels = url.searchParams.get('promote') === '1';
        const results = bakeAllSprites({ promotePixels });
        return sendJson(res, 200, { ok: true, count: results.length, results });
      }
      if (req.method === 'GET' && (url.pathname === '/' || url.pathname === '/index.html')) {
        const html = fs.readFileSync(path.join(STUDIO, 'index.html'));
        res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
        return res.end(html);
      }

      const getMatch = url.pathname.match(/^\/api\/sprites\/(.+)$/);
      if (req.method === 'GET' && getMatch) {
        const id = safeSpriteId(getMatch[1]);
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

      if (req.method === 'PUT' && getMatch) {
        const id = safeSpriteId(getMatch[1]);
        if (!id) return sendJson(res, 400, { error: 'invalid id' });
        const folder = id.split('/')[0];
        const pngPath = path.join(TEXTURES, id + '.png');
        if (!fs.existsSync(path.join(TEXTURES, folder))) {
          return sendJson(res, 404, { error: 'textures folder missing' });
        }
        const raw = await readBody(req);
        let body;
        try {
          body = JSON.parse(raw.toString('utf8'));
        } catch (_) {
          return sendJson(res, 400, { error: 'invalid json' });
        }

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

      sendJson(res, 404, { error: 'not found' });
    } catch (err) {
      sendJson(res, 500, { error: String(err && err.message ? err.message : err) });
    }
  });

  server.listen(PORT, HOST, () => {
    console.log(`Sprite Studio http://${HOST}:${PORT}`);
  });
}

if (process.argv.includes('--bake')) {
  const promotePixels = process.argv.includes('--promote');
  const results = bakeAllSprites({ promotePixels });
  const failed = results.filter((r) => !r.ok);
  for (const r of results) {
    console.log(r.ok ? `ok  ${r.id} (${r.size})` : `ERR ${r.id}: ${r.error}`);
  }
  console.log(`baked ${results.length - failed.length}/${results.length}`);
  process.exit(failed.length ? 1 : 0);
} else {
  startServer();
}
