# Chamber Crawl MonoGame Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Substituir o MVP web por Chamber Crawl em MonoGame DesktopGL (.NET), portando o mesmo gameplay e fixes.

**Architecture:** Solution C# com projeto `ChamberCrawl` (DesktopGL) + `ChamberCrawl.Tests` (xUnit). Lógica em camadas Models/World/Systems/Rendering. `Game1` avança `timeMs` uma vez por frame e chama sistemas na ordem movement → AI → combat → pickups → doors → progression → draw. Texturas PNG via `Texture2D.FromStream` (sem MGCB obrigatório).

**Tech Stack:** .NET 8 ou 9 SDK, MonoGame.Framework.DesktopGL, MonoGame.Templates.CSharp (`mgdesktopgl`), xUnit, Linux DesktopGL

## Global Constraints

- Idioma UI/commits: português brasileiro
- Remover stack Node do produto (package.json, src TS, Vite, node_modules, dist)
- Controles: WASD move, setas atiram, R reinicia
- Mesmo MVP: 2 andares, 6–8 salas, 4 inimigos, 6 itens, loja, boss
- Fixes obrigatórios: claimedSpawns, auto-coleta de moedas ao clear, portal offset+500ms, i-frames, boss phase=0
- `timeMs` só no `Game1.Update` (não dentro de Combat)
- Spec: `docs/superpowers/specs/2026-10-21-chamber-crawl-monogame-design.md`
- Referência TS (enquanto existir): `src/` na branch atual — portar regras, depois apagar

## File Structure

```
ChamberCrawl.sln
ChamberCrawl/
  ChamberCrawl.csproj
  Program.cs
  Game1.cs
  Models/GameTypes.cs
  Models/Constants.cs
  World/FloorGen.cs
  World/RoomGen.cs
  Data/Items.cs
  Data/Enemies.cs
  Systems/InputSystem.cs
  Systems/Collision.cs
  Systems/Movement.cs
  Systems/Combat.cs
  Systems/Ai.cs
  Systems/Doors.cs
  Systems/Pickups.cs
  Systems/Progression.cs
  Systems/RunFactory.cs
  Rendering/TextureAtlas.cs
  Rendering/GameRenderer.cs
  Assets/Textures/*.png
ChamberCrawl.Tests/
  ChamberCrawl.Tests.csproj
  FloorGenTests.cs
  RoomGenTests.cs
  ItemsTests.cs
  CollisionTests.cs
  DoorsTests.cs
  PickupsTests.cs
  ProgressionTests.cs
README.md
docs/superpowers/...
```

---

### Task 1: Instalar .NET, scaffold MonoGame, limpar web

**Files:**
- Create: `ChamberCrawl.sln`, `ChamberCrawl/*` (template), `ChamberCrawl.Tests/*`
- Delete: `package.json`, `package-lock.json`, `vite.config.ts`, `tsconfig.json`, `index.html`, `src/`, `dist/`, `node_modules/` (se versionado), manter `docs/`
- Modify: `.gitignore` (dotnet), `README.md` stub mínimo ok se Task 14 reescreve

**Interfaces:**
- Produz: `dotnet build` e `dotnet test` (0 testes) verdes, janela MonoGame com `dotnet run`

- [ ] **Step 1: Instalar SDK se necessário**

```bash
dotnet --version || (wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh && bash /tmp/dotnet-install.sh --channel 8.0 && export DOTNET_ROOT=$HOME/.dotnet && export PATH=$PATH:$HOME/.dotnet)
dotnet --version
```

Expected: versão 8.x ou 9.x impressa.

- [ ] **Step 2: Templates e projeto**

Na raiz do repo `/home/andre/Projetos/game`:

```bash
dotnet new install MonoGame.Templates.CSharp
dotnet new sln -n ChamberCrawl
dotnet new mgdesktopgl -n ChamberCrawl -o ChamberCrawl
dotnet sln ChamberCrawl.sln add ChamberCrawl/ChamberCrawl.csproj
dotnet new xunit -n ChamberCrawl.Tests -o ChamberCrawl.Tests
dotnet sln ChamberCrawl.sln add ChamberCrawl.Tests/ChamberCrawl.Tests.csproj
dotnet add ChamberCrawl.Tests/ChamberCrawl.Tests.csproj reference ChamberCrawl/ChamberCrawl.csproj
```

Se `mgdesktopgl` falhar, criar classlib + adicionar NuGet `MonoGame.Framework.DesktopGL` e `Program.cs`/`Game1.cs` mínimos conforme docs MonoGame.

- [ ] **Step 3: Remover stack web**

```bash
rm -rf node_modules dist src
rm -f package.json package-lock.json vite.config.ts tsconfig.json index.html
```

Atualizar `.gitignore`:

```
bin/
obj/
.vs/
*.user
node_modules/
dist/
.superpowers/
.worktrees/
```

- [ ] **Step 4: Verificar**

```bash
dotnet build
dotnet test
dotnet run --project ChamberCrawl/ChamberCrawl.csproj
```

Expected: build OK, testes OK (vazios ou UnitTest1), janela cornflower blue abre (pode fechar manualmente / timeout).

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "$(cat <<'EOF'
chore: scaffold MonoGame DesktopGL e remove stack web

EOF
)"
```

---

### Task 2: Models e Constants

**Files:**
- Create: `ChamberCrawl/Models/Constants.cs`, `ChamberCrawl/Models/GameTypes.cs`
- Delete: `ChamberCrawl.Tests/UnitTest1.cs` se existir

**Interfaces:**
- Produz: `GameConstants`, enums/records alinhados ao TS (`Dir`, `RoomType`, `RunState`, entidades, `claimedSpawns`, portal `ActiveAt`)

- [ ] **Step 1: `Constants.cs`**

```csharp
namespace ChamberCrawl.Models;

public static class GameConstants
{
    public const int RoomW = 15;
    public const int RoomH = 11;
    public const int Cell = 32;
    public const float PlayerRadius = 10f;
    public const float ProjectileRadius = 4f;
    public const float IframeMs = 600f;
    public const float PortalDelayMs = 500f;

    public const int TileFloor = 0;
    public const int TileWall = 1;
    public const int TileDoor = 2;
    public const int TileObstacle = 3;
}
```

- [ ] **Step 2: `GameTypes.cs`**

Portar tipos de `src/game/types.ts` para C#:

```csharp
namespace ChamberCrawl.Models;

public enum Dir { N, S, E, W }
public enum RoomType { Normal, Item, Shop, Boss, Start }
public enum RunStatus { Playing, Dead, Won }
public enum EnemyKind { Chaser, Shooter, Wander, Boss }
public enum EntityKind { Player, Enemy, Projectile, Pickup, Door }
public enum PickupKind { Coin, Item, Portal }

public sealed class PlayerStats
{
    public int MaxHp { get; set; }
    public int Hp { get; set; }
    public float Speed { get; set; }
    public float Damage { get; set; }
    public float FireRate { get; set; }
    public float Range { get; set; }
    public bool TripleShot { get; set; }
    public int CoinMultiplier { get; set; }
    public float IframeUntil { get; set; }
}

public sealed class FloorRoomNode
{
    public string Id { get; set; } = "";
    public RoomType Type { get; set; }
    public Dictionary<Dir, string> Neighbors { get; set; } = new();
    public bool Cleared { get; set; }
    public bool Visited { get; set; }
    public int RoomSeed { get; set; }
    public List<int> ClaimedSpawns { get; set; } = new();
}

public sealed class Floor
{
    public int Depth { get; set; } // 1 or 2
    public Dictionary<string, FloorRoomNode> Rooms { get; set; } = new();
    public string StartRoomId { get; set; } = "";
    public string BossRoomId { get; set; } = "";
}

public sealed class RoomData
{
    public int[,] Tiles { get; set; } = new int[GameConstants.RoomH, GameConstants.RoomW];
    public List<SpawnSpec> Spawns { get; set; } = new();
}

public abstract class SpawnSpec { }
public sealed class EnemySpawn : SpawnSpec
{
    public EnemyKind Enemy { get; set; }
    public int Tx { get; set; }
    public int Ty { get; set; }
}
public sealed class ItemSpawn : SpawnSpec
{
    public string ItemId { get; set; } = "";
    public int Tx { get; set; }
    public int Ty { get; set; }
    public int? Price { get; set; }
    public int SpawnIndex { get; set; }
}
public sealed class DoorSpawn : SpawnSpec
{
    public Dir Dir { get; set; }
    public int Tx { get; set; }
    public int Ty { get; set; }
}

public abstract class Entity
{
    public string Id { get; set; } = "";
    public EntityKind Kind { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Vx { get; set; }
    public float Vy { get; set; }
    public float Radius { get; set; }
}

public sealed class PlayerEntity : Entity
{
    public int Hp { get; set; }
    public PlayerEntity() { Kind = EntityKind.Player; }
}

public sealed class EnemyEntity : Entity
{
    public EnemyKind Enemy { get; set; }
    public float Hp { get; set; }
    public float Cooldown { get; set; }
    public float Phase { get; set; }
    public EnemyEntity() { Kind = EntityKind.Enemy; }
}

public sealed class ProjectileEntity : Entity
{
    public bool FromPlayer { get; set; }
    public float Damage { get; set; }
    public float Life { get; set; }
    public ProjectileEntity() { Kind = EntityKind.Projectile; }
}

public sealed class PickupEntity : Entity
{
    public PickupKind Pickup { get; set; }
    public string? ItemId { get; set; }
    public int? Price { get; set; }
    public int Amount { get; set; } = 1;
    public int? SpawnIndex { get; set; }
    public float? ActiveAt { get; set; }
    public PickupEntity() { Kind = EntityKind.Pickup; }
}

public sealed class DoorEntity : Entity
{
    public Dir Dir { get; set; }
    public bool Locked { get; set; }
    public DoorEntity() { Kind = EntityKind.Door; }
}

public sealed class RunState
{
    public int Seed { get; set; }
    public Floor Floor { get; set; } = new();
    public string CurrentRoomId { get; set; } = "";
    public RoomData Room { get; set; } = new();
    public List<Entity> Entities { get; set; } = new();
    public PlayerStats PlayerStats { get; set; } = new();
    public int Coins { get; set; }
    public List<string> Inventory { get; set; } = new();
    public RunStatus Status { get; set; } = RunStatus.Playing;
    public float ShootCooldown { get; set; }
    public float TimeMs { get; set; }
}
```

- [ ] **Step 3: Build + commit**

```bash
dotnet build
git add ChamberCrawl/Models
git commit -m "$(cat <<'EOF'
feat: models e constantes do Chamber Crawl MonoGame

EOF
)"
```

---

### Task 3: FloorGen (TDD)

**Files:**
- Create: `ChamberCrawl/World/FloorGen.cs`, `ChamberCrawl.Tests/FloorGenTests.cs`

**Interfaces:**
- Produz: `FloorGen.Generate(int seed, int depth): Floor`

- [ ] **Step 1: Teste falhando**

```csharp
using ChamberCrawl.Models;
using ChamberCrawl.World;
using Xunit;

public class FloorGenTests
{
    [Fact]
    public void Generate_CreatesConnectedRoomsWithRequiredTypes()
    {
        var floor = FloorGen.Generate(42, 1);
        Assert.InRange(floor.Rooms.Count, 6, 8);
        Assert.Equal(RoomType.Start, floor.Rooms[floor.StartRoomId].Type);
        Assert.Equal(RoomType.Boss, floor.Rooms[floor.BossRoomId].Type);
        Assert.Contains(floor.Rooms.Values, r => r.Type == RoomType.Shop);
        Assert.Contains(floor.Rooms.Values, r => r.Type == RoomType.Item);

        var seen = new HashSet<string>();
        var stack = new Stack<string>();
        stack.Push(floor.StartRoomId);
        while (stack.Count > 0)
        {
            var id = stack.Pop();
            if (!seen.Add(id)) continue;
            foreach (var n in floor.Rooms[id].Neighbors.Values)
                stack.Push(n);
        }
        Assert.Equal(floor.Rooms.Count, seen.Count);
    }

    [Fact]
    public void Generate_IsDeterministic()
    {
        var a = FloorGen.Generate(7, 2);
        var b = FloorGen.Generate(7, 2);
        Assert.Equal(a.Rooms.Keys.OrderBy(x => x), b.Rooms.Keys.OrderBy(x => x));
        Assert.Equal(a.BossRoomId, b.BossRoomId);
    }
}
```

- [ ] **Step 2: `dotnet test` → FAIL**

- [ ] **Step 3: Implementar `FloorGen`**

Portar algoritmo de `src/world/floorGen.ts` usando `Random(seed + depth * 10007)`. Incluir `ClaimedSpawns = new()` em cada nó. Start `Cleared = true`.

- [ ] **Step 4: `dotnet test` → PASS + commit**

```bash
git add ChamberCrawl/World/FloorGen.cs ChamberCrawl.Tests/FloorGenTests.cs
git commit -m "$(cat <<'EOF'
feat: geração procedural do grafo de salas em C#

EOF
)"
```

---

### Task 4: RoomGen (TDD)

**Files:**
- Create: `ChamberCrawl/World/RoomGen.cs`, `ChamberCrawl.Tests/RoomGenTests.cs`

**Interfaces:**
- Produz: `RoomGen.Generate(FloorRoomNode node, IReadOnlyList<Dir> neighbors): RoomData`
- Item spawns com `SpawnIndex` sequencial para claimedSpawns

- [ ] **Step 1–4:** Espelhar testes/comportamento de `src/world/roomGen.ts` e `tests/roomGen.test.ts` (bordas, portas, tipos, determinismo).

```bash
dotnet test --filter FloorGenTests|RoomGenTests
git add ChamberCrawl/World/RoomGen.cs ChamberCrawl.Tests/RoomGenTests.cs
git commit -m "$(cat <<'EOF'
feat: geração de tilemap e spawns por tipo de sala

EOF
)"
```

---

### Task 5: Collision + Items (TDD)

**Files:**
- Create: `ChamberCrawl/Systems/Collision.cs`, `ChamberCrawl/Data/Items.cs`, `ChamberCrawl.Tests/CollisionTests.cs`, `ChamberCrawl.Tests/ItemsTests.cs`

**Interfaces:**
- `Collision.CirclesOverlap`, `Collision.CircleHitsSolid(float x,y,r,int[,] tiles)`
- `Items.Defs`, `Items.Apply(PlayerStats, string itemId): PlayerStats` (retorna clone atualizado)
- `Doors.IsRoomCleared(IEnumerable<Entity>)` pode viver em `Doors.cs` já criado mínimo

- [ ] Implementar com TDD espelhando `src/systems/collision.ts` e `src/data/items.ts` (6 itens: damage_up, speed_up, hp_up, fire_rate, triple, greedy).

```bash
dotnet test
git commit -m "$(cat <<'EOF'
feat: colisão círculo-tile e aplicação de itens

EOF
)"
```

---

### Task 6: Input, RunFactory, Movement, Enemies data

**Files:**
- Create: `ChamberCrawl/Systems/InputSystem.cs`, `ChamberCrawl/Systems/RunFactory.cs`, `ChamberCrawl/Systems/Movement.cs`, `ChamberCrawl/Data/Enemies.cs`
- Test: `ChamberCrawl.Tests/RunFactoryTests.cs`

**Interfaces:**
- `InputSnapshot { MoveX, MoveY, ShootX, ShootY, Restart }`
- `InputSystem.Poll(): InputSnapshot` via `Keyboard.GetState()` (Keys.W/A/S/D, arrows, R)
- `RunFactory.Create(int seed): RunState`
- `RunFactory.EnterRoom(RunState, string roomId, Dir? entryDir)`
- `Movement.Update(RunState, InputSnapshot, float dtSeconds)`
- `Enemies.Defs` com hp/speed/damage/preferredDist/chargeSpeed

Portar `src/game/run.ts`, `input.ts`, `movement.ts`, `data/enemies.ts`. Stats iniciais: hp 6, speed 120, damage 1, fireRate 4, range 320. Boss `Phase = 0`. Respeitar `ClaimedSpawns` ao spawnar itens.

```bash
dotnet test
git commit -m "$(cat <<'EOF'
feat: input, bootstrap do run e movimento com colisão

EOF
)"
```

---

### Task 7: Combat

**Files:**
- Create: `ChamberCrawl/Systems/Combat.cs`, `ChamberCrawl.Tests/CombatTests.cs`

**Interfaces:**
- `Combat.Update(RunState, InputSnapshot, float dt, Func<float>? rng = null)`
- `Combat.SpawnEnemyProjectile(RunState, EnemyEntity from, float towardX, float towardY, float damage)`
- NÃO avançar `TimeMs` aqui (fica no Game1)

Portar tiro player (triple ±15°), projéteis, hits, i-frames, contato, drops de moeda (~50%), death → `RunStatus.Dead`. Sync `PlayerEntity.Hp`.

```bash
dotnet test
git commit -m "$(cat <<'EOF'
feat: sistema de projéteis, dano e i-frames

EOF
)"
```

---

### Task 8: AI

**Files:**
- Create: `ChamberCrawl/Systems/Ai.cs`, `ChamberCrawl.Tests/AiTests.cs`

**Interfaces:**
- `Ai.Update(RunState, float dt, Func<float>? rng = null)`
- Sem dano de contato (já no Combat)
- Usa `Combat.SpawnEnemyProjectile`

Comportamentos: chaser, wander, shooter, boss (2s charge / 1s rajada 8 dirs).

```bash
dotnet test
git commit -m "$(cat <<'EOF'
feat: IA chaser, shooter, wander e boss

EOF
)"
```

---

### Task 9: Doors + Progression (com fixes)

**Files:**
- Create/Modify: `ChamberCrawl/Systems/Doors.cs`, `ChamberCrawl/Systems/Progression.cs`
- Test: `ChamberCrawl.Tests/DoorsTests.cs`, `ChamberCrawl.Tests/ProgressionTests.cs`

**Interfaces:**
- `Doors.IsRoomCleared`, `Doors.Update(RunState)` — unlock, `EnterRoom`, **auto-coleta moedas** ao clear
- `Progression.Update(RunState)` — portal offset (1 tile sul do centro) + `ActiveAt = TimeMs + PortalDelayMs`, advance floor 1→2, won no boss depth 2
- `Progression.AdvanceFloor(RunState)`

Testes: clear, porta locked, transição, moedas auto, portal não ativa imediato no centro, vitória depth 2.

```bash
dotnet test
git commit -m "$(cat <<'EOF'
feat: portas, progressão de andares e fixes de economia/portal

EOF
)"
```

---

### Task 10: Pickups

**Files:**
- Create: `ChamberCrawl/Systems/Pickups.cs`, `ChamberCrawl.Tests/PickupsTests.cs`

**Interfaces:**
- `Pickups.Update(RunState)` — coin/item/shop, ignora portal, grava `ClaimedSpawns` ao pegar item

Teste: pegar item → `EnterRoom` de volta → item não respawna.

```bash
dotnet test
git commit -m "$(cat <<'EOF'
feat: coleta de moedas e itens da loja

EOF
)"
```

---

### Task 11: Texturas PNG

**Files:**
- Create: `ChamberCrawl/Assets/Textures/*.png` (player, enemies, projectile, heart, coin, door, tiles floor/wall/door/obstacle, 6 items, portal)
- Modify: `ChamberCrawl.csproj` para copiar assets:

```xml
<ItemGroup>
  <None Update="Assets/Textures/**/*.png">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

Gerar PNGs 32×32 (ImageMagick `convert`, Python PIL, ou export dos SVGs antigos se ainda em git history `git show 66e3ec3:src/assets/svg/player.svg`). Cores flat conforme spec.

```bash
git add ChamberCrawl/Assets ChamberCrawl/ChamberCrawl.csproj
git commit -m "$(cat <<'EOF'
feat: texturas PNG flat para entidades e tiles

EOF
)"
```

---

### Task 12: Rendering

**Files:**
- Create: `ChamberCrawl/Rendering/TextureAtlas.cs`, `ChamberCrawl/Rendering/GameRenderer.cs`

**Interfaces:**
- `TextureAtlas.Load(GraphicsDevice, contentRootPath)`
- `GameRenderer.Draw(SpriteBatch, RunState, TextureAtlas)` — tiles + entities + HUD (corações, moedas, Andar N, minimapa visitado, banner morto/vitória)

Usar `SpriteFont`: ou bitmap font simples gerada, ou desenhar HUD só com sprites (corações/moedas) + retângulos coloridos para texto via texturas 1×1 e dígitos PNG, ou `SpriteFont` do Content Pipeline se o template já tiver. Preferência MVP: texturas para números 0–9 + labels curtas em PNG, OU MonoGame Extended bitmap. Mais simples: `SpriteBatch.DrawString` com fonte default se template incluir `Content/Fonts`. Se não houver fonte, desenhar HUD com sprites e um `Texture2D` 1×1 para barras/texto improviso (minimapa = quadrados coloridos).

```bash
dotnet build
git commit -m "$(cat <<'EOF'
feat: render SpriteBatch tilemap, sprites e HUD

EOF
)"
```

---

### Task 13: Game1 loop + wiring

**Files:**
- Modify: `ChamberCrawl/Game1.cs`, `ChamberCrawl/Program.cs`

**Interfaces:**
- `LoadContent`: carregar atlas
- `Update`: `TimeMs += dt*1000` **uma vez**; se Playing: Movement → Ai → Combat → Pickups → Doors → Progression; se Dead/Won e Restart → `Create(seed)`
- `Draw`: clear + renderer
- Janela ~ `RoomW*Cell` × `RoomH*Cell` (+ margem HUD)

```csharp
protected override void Update(GameTime gameTime)
{
    var dt = MathF.Min(0.05f, (float)gameTime.ElapsedGameTime.TotalSeconds);
    var input = InputSystem.Poll();
    if (_state.Status != RunStatus.Playing)
    {
        if (input.Restart)
            _state = RunFactory.Create(Environment.TickCount);
    }
    else
    {
        _state.TimeMs += dt * 1000f;
        Movement.Update(_state, input, dt);
        Ai.Update(_state, dt);
        Combat.Update(_state, input, dt);
        Pickups.Update(_state);
        Doors.Update(_state);
        Progression.Update(_state);
    }
    base.Update(gameTime);
}
```

```bash
dotnet test && dotnet build
# smoke: dotnet run --project ChamberCrawl (manual)
git commit -m "$(cat <<'EOF'
feat: loop MonoGame e integração completa do MVP

EOF
)"
```

---

### Task 14: README e verificação final

**Files:**
- Create/Replace: `README.md`

- [ ] README em PT-BR: `dotnet restore`, `dotnet run --project ChamberCrawl`, `dotnet test`, controles, objetivo MVP, requisito .NET SDK + DesktopGL
- [ ] `dotnet test` e `dotnet build` verdes
- [ ] Confirmar ausência de `package.json` / `src/*.ts` no produto

```bash
git add README.md
git commit -m "$(cat <<'EOF'
docs: README MonoGame com controles e como rodar

EOF
)"
```

---

## Spec coverage (self-review)

| Requisito | Task |
|-----------|------|
| Instalar .NET + DesktopGL | 1 |
| Remover web | 1 |
| Models + claimedSpawns + ActiveAt | 2 |
| Floor/Room gen | 3–4 |
| Collision/Items | 5 |
| Input/Run/Movement | 6 |
| Combat + i-frames | 7 |
| AI 4 tipos | 8 |
| Doors + portal delay + auto coins + advance | 9 |
| Pickups + claimed | 10 |
| PNGs | 11 |
| Render | 12 |
| Loop timeMs único | 13 |
| README / critérios sucesso | 14 |

Sem placeholders. Nomes C# alinhados entre tasks (`FloorGen.Generate`, `RunFactory.Create`, `Doors.Update`, `Progression.Update`).
