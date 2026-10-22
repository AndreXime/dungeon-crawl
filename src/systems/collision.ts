import { CELL, TILE, type TileId } from "../game/constants";
import { isRoomCleared } from "./doors";

export { isRoomCleared };

const SAMPLE_ANGLES = [
  0,
  Math.PI * 0.25,
  Math.PI * 0.5,
  Math.PI * 0.75,
  Math.PI,
  Math.PI * 1.25,
  Math.PI * 1.5,
  Math.PI * 1.75,
];

export function circlesOverlap(
  x1: number,
  y1: number,
  r1: number,
  x2: number,
  y2: number,
  r2: number,
): boolean {
  const dx = x1 - x2;
  const dy = y1 - y2;
  const radiusSum = r1 + r2;
  return dx * dx + dy * dy < radiusSum * radiusSum;
}

function isTileSolid(tx: number, ty: number, tiles: TileId[][]): boolean {
  const row = tiles[ty];
  if (!row) return true;
  const tile = row[tx];
  if (tile === undefined) return true;
  return tile === TILE.wall || tile === TILE.obstacle;
}

export function circleHitsSolid(
  x: number,
  y: number,
  radius: number,
  tiles: TileId[][],
): boolean {
  for (const angle of SAMPLE_ANGLES) {
    const px = x + Math.cos(angle) * radius;
    const py = y + Math.sin(angle) * radius;
    const tx = Math.floor(px / CELL);
    const ty = Math.floor(py / CELL);
    if (isTileSolid(tx, ty, tiles)) {
      return true;
    }
  }
  return false;
}

export const circleHitsTile = circleHitsSolid;
