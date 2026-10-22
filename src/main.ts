import "./style.css";
import { startGame } from "./game/loop";

function element(id: string): HTMLElement {
  const el = document.getElementById(id);
  if (!el) throw new Error(`Elemento #${id} não encontrado`);
  return el;
}

startGame({
  tilemap: element("tilemap"),
  overlay: element("overlay"),
  hud: element("hud"),
});
