# Design: Chamber Crawl em MonoGame (substitui stack web)

Data: 2026-10-21  
Status: aprovado pelo usuário (brainstorming)  
Substitui: MVP Vite + TypeScript + rot.js + SVG

## Objetivo

Reescrever o roguelike tempo real estilo Binding of Isaac no **MonoGame DesktopGL** (.NET 8), no Linux (e Windows quando possível). Remover o stack Node/web deste repositório. Manter o mesmo MVP de gameplay e os fixes já validados no protótipo web.

## Decisões

| Tema | Escolha |
|------|---------|
| Destino do web | **A** Remover do repo (só MonoGame) |
| Plataforma | **DesktopGL** (Linux + Windows) |
| Gameplay | **Mesmo MVP** do design Isaac/rot.js |
| Arte | **PNGs** + `SpriteBatch` (sem Content Pipeline obrigatório) |
| Abordagem | **Port direto** da arquitetura TS → C# |

## Escopo do MVP

- 2 andares, ~6–8 salas por andar (start, normal, item, shop, boss)
- Controles: WASD move, setas atiram, R reinicia
- 4 inimigos: chaser, shooter, wander, boss (charge + rajada 8 direções)
- 6 itens (dano, velocidade, HP, fire rate, triple, greedy)
- Moedas + loja
- Portas trancadas até limpar a sala
- Portal andar 1 → andar 2 (offset + atraso de ativação)
- Vitória: boss do andar 2
- Morte / vitória → R reinicia o run

### Fora do MVP

- Content Pipeline MGCB obrigatório, áudio polido, mobile, pathfinding A*, multiplayer, saves

## Arquitetura

**Stack:** .NET 8, MonoGame.Framework.DesktopGL, xUnit (testes), `dotnet run` / `dotnet test`.

**Loop:** `Game1.Update` / `Draw` com `dt` de `GameTime`. Ordem dos sistemas: movement → AI → combat → pickups → doors → progression → draw.

**Camadas:**

| Camada | Responsabilidade |
|--------|------------------|
| `Game1` / `Program` | Bootstrap, loop, wiring |
| `Models` | `RunState`, entidades, floor, stats |
| `World` | `FloorGen`, `RoomGen` |
| `Systems` | Input, Movement, Ai, Combat, Doors, Pickups, Progression |
| `Rendering` | Tilemap, sprites, HUD |
| `Data` | Definições de itens e inimigos |
| `Assets/Textures` | PNGs |

**RNG:** `System.Random` com seed (sem rot.js). Chase inimigo em linha reta + colisão de tile (como o web atual).

**Tempo:** `timeMs` avançado em **um** lugar só no `Update` do jogo (não dentro de combat isolado), para i-frames e portal `activeAt` ficarem consistentes.

## Fixes obrigatórios no port

Trazer do protótipo web:

1. `claimedSpawns` por sala: itens/loja não respawnam ao reentrar
2. Ao limpar a sala, auto-coletar moedas soltas
3. Portal do boss andar 1: offset do centro + atraso (~500 ms) antes de ativar
4. I-frames e dano de contato no sistema de combate
5. Boss inicia `phase = 0`

## Arte

- PNGs ~32×32 em `Assets/Textures/` (ou `Content/Textures/`)
- Carregar via `Texture2D.FromStream` / arquivo no diretório do jogo (MVP sem MGCB)
- Paleta alinhada ao protótipo: player azul, chaser vermelho, shooter roxo, wander verde, boss laranja, projétil amarelo
- Tiles: floor, wall, door, obstacle

## Estrutura do repositório (alvo)

```
ChamberCrawl/
  ChamberCrawl.csproj
  Program.cs
  Game1.cs
  Models/
  World/
  Systems/
  Rendering/
  Data/
  Assets/Textures/
ChamberCrawl.Tests/
  *.cs
docs/superpowers/specs/
docs/superpowers/plans/
README.md
```

Remover: `package.json`, `src/` TypeScript, Vite, `node_modules`, `dist`, dependências npm.

Manter: docs de design/plan anteriores como histórico (ou arquivar), README atualizado para MonoGame.

## Testes

Funções puras cobertas por xUnit:

- `generateFloor` (6–8 salas, start/boss/shop/item, conectividade, seed)
- `generateRoom` (bordas, portas, spawns por tipo)
- `applyItem`, `isRoomCleared`
- claimed spawns / sem respawn de item
- portal delay / offset (não avança no mesmo frame sem atraso)

Smoke: `dotnet run` no DesktopGL.

## Critérios de sucesso

1. Repo sem stack Node do jogo (só .NET/MonoGame para o produto)
2. Run completo jogável (2 andares → boss final)
3. Controles WASD + setas + R
4. Economia sem farm de item/loja por reentrada
5. `dotnet test` verde e `dotnet run` abre a janela no Linux

## Referência

Gameplay e regras detalhadas: `docs/superpowers/specs/2026-10-21-isaac-roguelike-design.md` (protótipo web). Este documento governa a **migração** e o stack MonoGame.
