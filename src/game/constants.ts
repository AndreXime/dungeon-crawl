export const ROOM_W = 15;
export const ROOM_H = 11;
export const CELL = 32;
export const PLAYER_RADIUS = 10;
export const PROJECTILE_RADIUS = 4;
export const IFRAME_MS = 600;

export const TILE = {
  floor: 0,
  wall: 1,
  door: 2,
  obstacle: 3,
} as const;

export type TileId = (typeof TILE)[keyof typeof TILE];
