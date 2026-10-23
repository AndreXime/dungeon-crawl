#!/usr/bin/env node
'use strict';
// Quantize Task 4 character PNGs to kit palette, outer outline, update SpriteSources.
// Usage: node tools/sprite-studio/quantize-characters.js [--verify-only]

const fs = require('node:fs');
const path = require('node:path');
const zlib = require('node:zlib');
const { execFileSync } = require('node:child_process');

const ROOT = path.resolve(__dirname, '../..');
const TEXTURES = path.join(ROOT, 'ChamberCrawl/Assets/Textures');
const SOURCES = path.join(ROOT, 'ChamberCrawl/Assets/SpriteSources');

const OUTLINE = '#0d0a08';

const KIT = [
  '#0d0a08',
  '#2a2620',
  '#4a443c',
  '#6a6358',
  '#3d2814',
  '#8b5a2b',
  '#c4a06a',
  '#d4a017',
  '#f0d060',
  '#8b1a1a',
  '#d43030',
  '#2d5a2d',
  '#4a9a3a',
  '#e8dcc0',
  '#c4b898',
  '#4a2060',
  '#6b3a8a',
  '#e8c8a0',
  '#3d5a3d',
  '#c45cff',
  '#f0e0ff',
];

const MAGIC_IDS = new Set(['player/bolt', 'enemies/projectile']);
const MAGIC_PALETTE = ['#0d0a08', '#4a2060', '#6b3a8a', '#c45cff', '#f0e0ff', '#8b1a1a', '#d43030'];

const SPRITES = [
  'player/player',
  'player/bolt',
  'enemies/enemy-bat',
  'enemies/enemy-skeleton',
  'enemies/enemy-slime',
  'enemies/enemy-ogre',
  'enemies/projectile',
];

function hexToRgb(hex) {
  const n = parseInt(hex.slice(1), 16);
  return [(n >>> 16) & 255, (n >>> 8) & 255, n & 255];
}

function rgbToHex(r, g, b) {
  return (
    '#' +
    [r, g, b]
      .map((v) => Math.max(0, Math.min(255, v | 0)).toString(16).padStart(2, '0'))
      .join('')
  );
}

const KIT_RGB = KIT.map((h) => ({ hex: h, rgb: hexToRgb(h) }));
const MAGIC_RGB = MAGIC_PALETTE.map((h) => ({ hex: h, rgb: hexToRgb(h) }));

function dist2(a, b) {
  const dr = a[0] - b[0];
  const dg = a[1] - b[1];
  const db = a[2] - b[2];
  return dr * dr + dg * dg + db * db;
}

function nearestHex(rgb, paletteRgb) {
  let best = paletteRgb[0];
  let bestD = Infinity;
  for (const entry of paletteRgb) {
    const d = dist2(rgb, entry.rgb);
    if (d < bestD) {
      bestD = d;
      best = entry;
    }
  }
  return best.hex;
}

function loadRgba(pngPath) {
  const sizeStr = execFileSync('magick', [pngPath, '-format', '%wx%h', 'info:']).toString().trim();
  const [w, h] = sizeStr.split('x').map(Number);
  if (w !== h) throw new Error(`${pngPath}: expected square, got ${sizeStr}`);
  const raw = execFileSync('magick', [pngPath, '-alpha', 'on', '-depth', '8', 'rgba:-']);
  return { size: w, rgba: new Uint8ClampedArray(raw.buffer, raw.byteOffset, raw.byteLength) };
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

function remapMagicRgb(r, g, b) {
  const lum = 0.299 * r + 0.587 * g + 0.114 * b;
  const max = Math.max(r, g, b);
  if (max > 185 || lum > 175) return hexToRgb('#f0e0ff');
  if (max > 120 || lum > 95) return hexToRgb('#c45cff');
  if (lum > 55) return hexToRgb('#6b3a8a');
  return hexToRgb('#4a2060');
}

function processSprite(id) {
  const pngPath = path.join(TEXTURES, id + '.png');
  const { size, rgba } = loadRgba(pngPath);
  const paletteRgb = MAGIC_IDS.has(id) ? MAGIC_RGB : KIT_RGB;
  const orig = new Uint8ClampedArray(rgba);

  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const i = (y * size + x) * 4;
      if (rgba[i + 3] < 128) {
        rgba[i + 3] = 0;
        continue;
      }
      let r = rgba[i];
      let g = rgba[i + 1];
      let b = rgba[i + 2];
      let hex = null;

      if (MAGIC_IDS.has(id)) {
        [r, g, b] = remapMagicRgb(r, g, b);
        const lum = 0.299 * orig[i] + 0.587 * orig[i + 1] + 0.114 * orig[i + 2];
        if (id === 'enemies/projectile' && lum > 140) hex = '#f0e0ff';
        else if (id === 'enemies/projectile' && lum > 85) hex = '#c45cff';
      }

      if (id === 'enemies/enemy-bat') {
        const or = orig[i];
        const og = orig[i + 1];
        const ob = orig[i + 2];
        const bright = or > 170 && og > 170 && ob > 170;
        const eyeZone = y >= size * 0.25 && y <= size * 0.55 && x >= size * 0.3 && x <= size * 0.7;
        if (bright || (eyeZone && or > 120 && og > 100 && ob > 100 && Math.abs(or - og) < 40)) {
          hex = '#d43030';
        }
      }

      if (!hex) hex = nearestHex([r, g, b], paletteRgb);
      setHex(rgba, size, x, y, hex);
    }
  }

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

  mergeToMaxColors(rgba, size, 16);

  return { id, size, rgba };
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

function audit(id, rgba, size) {
  const colors = new Map();
  let outlinePx = 0;
  let batEyeRed = 0;
  for (let y = 0; y < size; y++) {
    for (let x = 0; x < size; x++) {
      const i = (y * size + x) * 4;
      if (rgba[i + 3] < 128) continue;
      const h = getHex(rgba, size, x, y);
      colors.set(h, (colors.get(h) || 0) + 1);
      if (h === OUTLINE) outlinePx++;
      if (id === 'enemies/enemy-bat' && h === '#d43030') batEyeRed++;
    }
  }
  return {
    id,
    opaqueColors: colors.size,
    outlinePx,
    batEyeRed,
    hexList: [...colors.keys()].sort(),
  };
}

function verifyOnly() {
  const rows = [];
  for (const id of SPRITES) {
    const pngPath = path.join(TEXTURES, id + '.png');
    const { size, rgba } = loadRgba(pngPath);
    rows.push(audit(id, rgba, size));
  }
  return rows;
}

function main() {
  const verifyOnlyMode = process.argv.includes('--verify-only');
  if (verifyOnlyMode) {
    console.log(JSON.stringify(verifyOnly(), null, 2));
    return;
  }

  const audits = [];
  for (const id of SPRITES) {
    const { size, rgba } = processSprite(id);
    const pngPath = path.join(TEXTURES, id + '.png');
    const jsonPath = path.join(SOURCES, id + '.json');
    fs.writeFileSync(pngPath, encodePng(size, rgba));
    const doc = { size, mode: 'pixels', pixels: pixelsFromRgba(size, rgba) };
    fs.writeFileSync(jsonPath, JSON.stringify(doc, null, 2) + '\n', 'utf8');
    audits.push(audit(id, rgba, size));
    console.log(`ok ${id} colors=${audits[audits.length - 1].opaqueColors} outline=${audits[audits.length - 1].outlinePx}`);
  }
  console.log(JSON.stringify(audits, null, 2));
}

main();
