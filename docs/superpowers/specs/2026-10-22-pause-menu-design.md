# Menu de pause (ESC)

## Objetivo

ESC não fecha o jogo. Em partida, ESC abre/fecha um menu de pause com Continuar e voltar ao Menu.

## Comportamento

| Tela        | ESC                         |
|-------------|-----------------------------|
| MainMenu    | nada (sair só via SAIR)     |
| Settings    | volta ao MainMenu (igual)   |
| Playing     | vai para Paused             |
| Paused      | volta para Playing          |

- Em `Paused` o mundo não atualiza (congelado).
- Opções: `CONTINUAR` (→ Playing), `MENU` (→ MainMenu, descarta a run).
- Controles do pause: setas/WASD + Enter/Space (igual aos outros menus).
- Dead/Won: ESC também abre pause (para voltar ao menu sem depender de R).

## Implementação

- Novo `AppScreen.Paused`.
- `MenuController` ganha índice/labels de pause.
- `MenuRenderer.DrawPause` overlay sobre o frame do jogo (desenhar jogo + overlay).
- Remover `exitGame()` no ESC do main menu.

## Fora de escopo

- Pause em configurações dentro do pause.
- Salvar run.
