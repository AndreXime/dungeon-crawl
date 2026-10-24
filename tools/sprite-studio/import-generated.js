#!/usr/bin/env node
'use strict';
// Import GenerateImage outputs → resize NN → chroma-key → quantize kit → Textures + SpriteSources.
// Usage: node tools/sprite-studio/import-generated.js

const fs = require('node:fs');
const path = require('node:path');
const { execFileSync } = require('node:child_process');

const ROOT = path.resolve(__dirname, '../..');
const ASSETS_GEN = path.join(
  process.env.HOME,
  '.cursor/projects/home-andre-Projetos-game/assets',
);
const TEXTURES = path.join(ROOT, 'ChamberCrawl/Assets/Textures');
const SOURCES = path.join(ROOT, 'ChamberCrawl/Assets/SpriteSources');

const OUTLINE = '#0d0a08';
const KIT = [
  '#0d0a08', '#2a2620', '#4a443c', '#6a6358', '#3d2814', '#8b5a2b', '#c4a06a',
  '#d4a017', '#f0d060', '#8b1a1a', '#d43030', '#2d5a2d', '#4a9a3a', '#e8dcc0',
  '#c4b898', '#4a2060', '#6b3a8a', '#e8c8a0', '#3d5a3d', '#c45cff', '#f0e0ff',
];

/** @type {{ id: string, gen: string, size: number, opaque: boolean }[]} */
const JOBS = [
  { id: 'tiles/floor', gen: 'gen-floor.jpg', size: 32, opaque: true },
  { id: 'tiles/wall', gen: 'gen-wall.jpg', size: 32, opaque: true },
  { id: 'tiles/door', gen: 'gen-door.jpg', size: 32, opaque: false },
  { id: 'tiles/door-tile', gen: 'gen-door-tile.jpg', size: 32, opaque: true },
  { id: 'tiles/floor-2', gen: 'gen-floor-2.jpg', size: 32, opaque: true },
  { id: 'tiles/wall-2', gen: 'gen-wall-2.jpg', size: 32, opaque: true },
  { id: 'tiles/door-2', gen: 'gen-door-2.jpg', size: 32, opaque: false },
  { id: 'tiles/door-tile-2', gen: 'gen-door-tile-2.jpg', size: 32, opaque: true },
  { id: 'tiles/floor-3', gen: 'gen-floor-3.jpg', size: 32, opaque: true },
  { id: 'tiles/wall-3', gen: 'gen-wall-3.jpg', size: 32, opaque: true },
  { id: 'tiles/door-3', gen: 'gen-door-3.jpg', size: 32, opaque: false },
  { id: 'tiles/door-tile-3', gen: 'gen-door-tile-3.jpg', size: 32, opaque: true },
  { id: 'tiles/floor-4', gen: 'gen-floor-4.jpg', size: 32, opaque: true },
  { id: 'tiles/wall-4', gen: 'gen-wall-4.jpg', size: 32, opaque: true },
  { id: 'tiles/door-4', gen: 'gen-door-4.jpg', size: 32, opaque: false },
  { id: 'tiles/door-tile-4', gen: 'gen-door-tile-4.jpg', size: 32, opaque: true },
  { id: 'tiles/crate', gen: 'gen-crate.jpg', size: 32, opaque: false },
  { id: 'tiles/barrel', gen: 'gen-barrel.jpg', size: 32, opaque: false },
  { id: 'tiles/spike', gen: 'gen-spike.jpg', size: 32, opaque: false },
  { id: 'tiles/obstacle', gen: 'gen-obstacle.jpg', size: 32, opaque: false },
  { id: 'tiles/puddle', gen: 'gen-puddle.jpg', size: 32, opaque: false },
  { id: 'tiles/portal', gen: 'gen-portal.jpg', size: 64, opaque: false },
  { id: 'enemies/enemy-specter', gen: 'gen-enemy-specter.jpg', size: 32, opaque: false },
  { id: 'enemies/enemy-necromancer', gen: 'gen-enemy-necromancer.jpg', size: 64, opaque: false },
  { id: 'enemies/enemy-rat', gen: 'gen-enemy-rat.jpg', size: 32, opaque: false },
  { id: 'enemies/enemy-slime-king', gen: 'gen-enemy-slime-king.jpg', size: 64, opaque: false },
  { id: 'enemies/enemy-guard', gen: 'gen-enemy-guard.jpg', size: 32, opaque: false },
  { id: 'enemies/enemy-iron-golem', gen: 'gen-enemy-iron-golem.jpg', size: 64, opaque: false },
  { id: 'items/coin', gen: 'gen-coin.jpg', size: 64, opaque: false },
  { id: 'items/heart', gen: 'gen-heart.jpg', size: 64, opaque: false },
  { id: 'items/item-damage', gen: 'gen-item-damage.jpg', size: 64, opaque: false },
  { id: 'items/item-speed', gen: 'gen-item-speed.jpg', size: 64, opaque: false },
  { id: 'items/item-hp', gen: 'gen-item-hp.jpg', size: 64, opaque: false },
  { id: 'items/item-firerate', gen: 'gen-item-firerate.jpg', size: 64, opaque: false },
  { id: 'items/item-triple', gen: 'gen-item-triple.jpg', size: 64, opaque: false },
  { id: 'items/item-greedy', gen: 'gen-item-greedy.jpg', size: 64, opaque: false },
  { id: 'items/item-range', gen: 'gen-item-range.jpg', size: 64, opaque: false },
  { id: 'items/item-sharp', gen: 'gen-item-sharp.jpg', size: 64, opaque: false },
  { id: 'items/item-haste', gen: 'gen-item-haste.jpg', size: 64, opaque: false },
  { id: 'items/item-vigor', gen: 'gen-item-vigor.jpg', size: 64, opaque: false },
  { id: 'items/item-fortune', gen: 'gen-item-fortune.jpg', size: 64, opaque: false },
];

function hexToRgb(hex) {
  const n = parseInt(hex.slice(1), 16);
  return [(n >>> 16) & 255, (n >>> 8) & 255, n & 255];
}
function rgbToHex(r, g, b) {
  return '#' + [r, g, b].map((v) => Math.max(0, Math.min(255, v | 0)).toString(16).padStart(2, '0')).join('');
}
function dist2(a, b) {
  const dr = a[0] - b[0];
  const dg = a[1] - b[1];
  const db = a[2] - b[2];
  return dr * dr + dg * dg + db * db;
}

const KIT_RGB = KIT.map((h) => ({ hex: h, rgb: hexToRgb(h) }));

function nearestHex(rgb) {
  let best = KIT_RGB[0];
  let bestD = Infinity;
  for (const entry of KIT_RGB) {
    const d = dist2(rgb, entry.rgb);
    if (d < bestD) {
      bestD = d;
      best = entry;
    }
  }
  return best.hex;
}

function encodePng(size, rgba) {
  const zlib = require('node:zlib');
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
  return Buffer.concat([
    Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]),
    chunk('IHDR', ihdr),
    chunk('IDAT', compressed),
    chunk('IEND', Buffer.alloc(0)),
  ]);
}

function loadRgba(pngPath) {
  const sizeStr = execFileSync('magick', [pngPath, '-format', '%wx%h', 'info:']).toString().trim();
  const [w, h] = sizeStr.split('x').map(Number);
  if (w !== h) throw new Error(`${pngPath}: expected square, got ${sizeStr}`);
  const raw = execFileSync('magick', [pngPath, '-alpha', 'on', '-depth', '8', 'rgba:-']);
  return { size: w, rgba: new Uint8ClampedArray(raw.buffer, raw.byteOffset, raw.byteLength) };
}

function preprocess(job) {
  const src = path.join(ASSETS_GEN, job.gen);
  if (!fs.existsSync(src)) throw new Error(`missing gen: ${src}`);
  const tmp = path.join(ASSETS_GEN, `_tmp-${job.id.replace('/', '-')}.png`);

  if (job.opaque) {
    execFileSync('magick', [
      src,
      '-alpha', 'off',
      '-filter', 'point',
      '-resize', `${job.size}x${job.size}!`,
      tmp,
    ]);
  } else {
    // Remove near-white / pastel studio backdrop via flood from corners, then NN resize.
    execFileSync('magick', [
      src,
      '-alpha', 'set',
      '-fuzz', '18%',
      '-fill', 'none',
      '-draw', 'color 0,0 floodfill',
      '-draw', `color %[fx:w-1],0 floodfill`,
      '-draw', `color 0,%[fx:h-1] floodfill`,
      '-draw', `color %[fx:w-1],%[fx:h-1] floodfill`,
      '-filter', 'point',
      '-resize', `${job.size}x${job.size}!`,
      tmp,
    ]);
  }
  return tmp;
}

function isOpaque(rgba, size, x, y) {
  if (x < 0 || y < 0 || x >= size || y >= size) return false;
  return rgba[(y * size + x) * 4 + 3] >= 128;
}

function getHex(rgba, size, x, y) {
  const i = (y * size + x) * 4;
  return rgbToHex(rgba[i], rgba[i + 1], rgba[i + 2]);
}

function setHex(rgba, size, x, y, hex) {
  const [r, g, b] = hexToRgb(hex);
  const i = (y * size + x) * 4;
  rgba[i] = r;
  rgba[i + 1] = g;
  rgba[i + 2] = b;
  rgba[i + 3] = 255;
}

function mergeToMaxColors(rgba, size, maxColors) {
  const countColors = () => {
    const counts = new Map();
    for (let y = 0; y < size; y++) {
      for (let x = 0; x < size; x++) {
        const i = (y * size + x) * 4;
        if (rgba[i + 3] < 128) continue;
        const h = getHex(rgba, size, x, y);
        counts.set(h, (counts.get(h) || 0) + 1);
      }
    }
    return counts;
  };
  let counts = countColors();
  while (counts.size > maxColors) {
    let victim = null;
    let victimCount = Infinity;
    for (const [hex, n] of counts) {
      if (hex === OUTLINE) continue;
      if (n < victimCount) {
        victimCount = n;
        victim = hex;
      }
    }
    if (!victim) break;
    const victimRgb = hexToRgb(victim);
    let target = OUTLINE;
    let targetD = Infinity;
    for (const hex of counts.keys()) {
      if (hex === victim) continue;
      const d = dist2(victimRgb, hexToRgb(hex));
      if (d < targetD) {
        targetD = d;
        target = hex;
      }
    }
    for (let y = 0; y < size; y++) {
      for (let x = 0; x < size; x++) {
        if (getHex(rgba, size, x, y) === victim) setHex(rgba, size, x, y, target);
      }
    }
    counts = countColors();
  }
}

function processRgba(rgba, size, opaque) {
  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const i = (y * size + x) * 4;
      if (!opaque && rgba[i + 3] < 128) {
        rgba[i + 3] = 0;
        continue;
      }
      const hex = nearestHex([rgba[i], rgba[i + 1], rgba[i + 2]]);
      setHex(rgba, size, x, y, hex);
    }
  }

  if (!opaque) {
    const outlineLayer = [];
    for (let y = 0; y < size; y++) {
      for (let x = 0; x < size; x++) {
        if (isOpaque(rgba, size, x, y)) continue;
        const touch =
          isOpaque(rgba, size, x - 1, y) ||
          isOpaque(rgba, size, x + 1, y) ||
          isOpaque(rgba, size, x, y - 1) ||
          isOpaque(rgba, size, x, y + 1);
        if (touch) outlineLayer.push([x, y]);
      }
    }
    for (const [x, y] of outlineLayer) setHex(rgba, size, x, y, OUTLINE);
  }

  mergeToMaxColors(rgba, size, 16);
}

function pixelsFromRgba(size, rgba) {
  const pixels = [];
  for (let y = 0; y < size; y++) {
    const row = [];
    for (let x = 0; x < size; x++) {
      const i = (y * size + x) * 4;
      if (rgba[i + 3] < 128) row.push(null);
      else row.push(getHex(rgba, size, x, y));
    }
    pixels.push(row);
  }
  return pixels;
}

function main() {
  const onlyArg = process.argv.indexOf('--only');
  const only = onlyArg >= 0
    ? new Set(process.argv[onlyArg + 1].split(',').map((s) => s.trim()).filter(Boolean))
    : null;
  for (const job of JOBS) {
    if (only && !only.has(job.id)) continue;
    const tmp = preprocess(job);
    const { size, rgba } = loadRgba(tmp);
    if (size !== job.size) throw new Error(`${job.id}: size ${size} != ${job.size}`);
    processRgba(rgba, size, job.opaque);

    const pngPath = path.join(TEXTURES, job.id + '.png');
    const jsonPath = path.join(SOURCES, job.id + '.json');
    fs.mkdirSync(path.dirname(pngPath), { recursive: true });
    fs.mkdirSync(path.dirname(jsonPath), { recursive: true });
    fs.writeFileSync(pngPath, encodePng(size, rgba));
    fs.writeFileSync(
      jsonPath,
      JSON.stringify({ size, mode: 'pixels', pixels: pixelsFromRgba(size, rgba) }, null, 2) + '\n',
      'utf8',
    );
    fs.unlinkSync(tmp);

    let colors = 0;
    const seen = new Set();
    for (let i = 0; i < rgba.length; i += 4) {
      if (rgba[i + 3] < 128) continue;
      seen.add(rgbToHex(rgba[i], rgba[i + 1], rgba[i + 2]));
    }
    colors = seen.size;
    console.log(`ok ${job.id} colors=${colors} opaque=${job.opaque}`);
  }
}

main();
