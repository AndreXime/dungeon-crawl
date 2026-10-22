# Diskette Partition C (Fatia 1) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Entregar a fatia 1 do retema Diskette: um andar Partition C jogável com Diskette/Bits, obstáculos novos, Pspam, Trojan e Adware King, vitória no boss sem portal.

**Architecture:** Estender o MVP MonoGame existente. Novos tiles e `EnemyKind`, AI em `Ai.cs`, combate/colisão conscientes de pastas/bad sectors, `Progression` vence no depth 1. Arte PNG gerada no Cursor substitui texturas da Partition C. Namespaces/`ChamberCrawl` permanecem.

**Tech Stack:** C# / .NET 9, MonoGame DesktopGL, xUnit, PNGs via `TextureAtlas`.

## Global Constraints

- Spec: `docs/superpowers/specs/2026-10-22-diskette-theme-design.md`
- Nome de exibição: **Diskette** (janela, menu, README). Código: `ChamberCrawl`
- Fatia 1: só depth 1. Boss limpo → `Won`. Sem portal / sem `AdvanceFloor`
- Sem áudio nesta fatia (ponto de extensão ok no spawn do tiro)
- TDD: teste falhando → implementação → verde → commit por task
- Commits Conventional Commits em português (descrição)
- GlyphFont: A-Z, 0-9, espaço, `-`, `:`, `?` → título menu `DISKETTE`

## File map

| File | Role |
|------|------|
| `ChamberCrawl/Models/Constants.cs` | `TileFolder`, `TilePopup`, `TileBadSector` |
| `ChamberCrawl/Models/GameTypes.cs` | `EnemyKind`, `CanSplit` / `IframeUntil` em enemy, `BitGlyph` em projétil |
| `ChamberCrawl/Data/Enemies.cs` | Defs Pspam, Trojan, AdwareKing |
| `ChamberCrawl/Data/FloorThemes.cs` | Lookup texturas depth 1 |
| `ChamberCrawl/Systems/Collision.cs` | Sólidos novos + helper de tile sob círculo |
| `ChamberCrawl/Systems/Combat.cs` | Destrói folder; Bits do topo; i-frames enemy; onda boss |
| `ChamberCrawl/Systems/Ai.cs` | Pspam / Trojan / AdwareKing |
| `ChamberCrawl/Systems/Progression.cs` | Win depth 1 |
| `ChamberCrawl/World/RoomGen.cs` | Pool + obstáculos Partition C |
| `ChamberCrawl/Systems/RunFactory.cs` | `CanSplit` default no spawn |
| `ChamberCrawl/Rendering/*` | Tiles, sprites, Bits, boss scale |
| `ChamberCrawl/Game1.cs` + `MenuRenderer.cs` + `README.md` | Branding |
| `ChamberCrawl/Assets/Textures/*.png` | Arte gerada |
| `ChamberCrawl.Tests/*` | Novos/atualizados |

---

### Task 1: Tiles sólidos Partition C + Collision

**Files:**
- Modify: `ChamberCrawl/Models/Constants.cs`
- Modify: `ChamberCrawl/Systems/Collision.cs`
- Modify: `ChamberCrawl.Tests/CollisionTests.cs`

**Interfaces:**
- Produces: `GameConstants.TileFolder = 4`, `TilePopup = 5`, `TileBadSector = 6`
- Produces: `Collision.IsSolidTile(int tile) -> bool` (wall, obstacle, folder, popup, badsector)
- Produces: `Collision.TrySampleSolidTile(float x, float y, float radius, int[,] tiles, out int tx, out int ty, out int tile) -> bool`

- [ ] **Step 1: Write failing tests** (adicionar em `CollisionTests.cs`)

```csharp
[Theory]
[InlineData(GameConstants.TileFolder)]
[InlineData(GameConstants.TilePopup)]
[InlineData(GameConstants.TileBadSector)]
public void CircleHitsSolid_BlocksNewPartitionTiles(int tile)
{
    var tiles = new[,] { { GameConstants.TileFloor, tile }, { GameConstants.TileDoor, GameConstants.TileFloor } };
    Assert.True(Collision.CircleHitsSolid(GameConstants.Cell * 1.5f, GameConstants.Cell * 0.5f, 6, tiles));
}

[Fact]
public void TrySampleSolidTile_ReturnsFolderUnderProjectile()
{
    var tiles = new[,] { { GameConstants.TileFloor, GameConstants.TileFolder } };
    var ok = Collision.TrySampleSolidTile(
        GameConstants.Cell * 1.5f, GameConstants.Cell * 0.5f, 4, tiles, out var tx, out var ty, out var tile);
    Assert.True(ok);
    Assert.Equal(1, tx);
    Assert.Equal(0, ty);
    Assert.Equal(GameConstants.TileFolder, tile);
}
```

- [ ] **Step 2: Run tests — expect FAIL**

```bash
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~CollisionTests" --nologo
```

Expected: falha por constantes/método ausentes.

- [ ] **Step 3: Implement**

Em `Constants.cs` adicionar após `TileObstacle`:

```csharp
public const int TileFolder = 4;
public const int TilePopup = 5;
public const int TileBadSector = 6;
```

Em `Collision.cs`:

```csharp
public static bool IsSolidTile(int tile) =>
    tile is GameConstants.TileWall
        or GameConstants.TileObstacle
        or GameConstants.TileFolder
        or GameConstants.TilePopup
        or GameConstants.TileBadSector;

private static bool IsTileSolid(int tx, int ty, int[,] tiles)
{
    if (ty < 0 || ty >= tiles.GetLength(0) || tx < 0 || tx >= tiles.GetLength(1)) return true;
    return IsSolidTile(tiles[ty, tx]);
}

public static bool TrySampleSolidTile(
    float x, float y, float radius, int[,] tiles, out int tx, out int ty, out int tile)
{
    tx = ty = 0;
    tile = GameConstants.TileFloor;
    for (var i = 0; i < SampleCount; i++)
    {
        var angle = MathF.PI * 0.25f * i;
        var sx = (int)MathF.Floor((x + MathF.Cos(angle) * radius) / GameConstants.Cell);
        var sy = (int)MathF.Floor((y + MathF.Sin(angle) * radius) / GameConstants.Cell);
        if (sy < 0 || sy >= tiles.GetLength(0) || sx < 0 || sx >= tiles.GetLength(1))
            continue;
        var t = tiles[sy, sx];
        if (!IsSolidTile(t)) continue;
        tx = sx;
        ty = sy;
        tile = t;
        return true;
    }
    return false;
}
```

- [ ] **Step 4: Run tests — expect PASS**

```bash
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~CollisionTests" --nologo
```

- [ ] **Step 5: Commit**

```bash
git add ChamberCrawl/Models/Constants.cs ChamberCrawl/Systems/Collision.cs ChamberCrawl.Tests/CollisionTests.cs
git commit -m "$(cat <<'EOF'
feat: tiles Partition C sólidos na colisão

EOF
)"
```

---

### Task 2: Combate vs pastas e bad sectors

**Files:**
- Modify: `ChamberCrawl/Systems/Combat.cs` (`UpdateProjectiles`)
- Create: `ChamberCrawl.Tests/ObstacleCombatTests.cs`

**Interfaces:**
- Consumes: `Collision.TrySampleSolidTile`
- Produces: tiro do jogador em `TileFolder` → tile vira `TileFloor` e projétil some
- Produces: qualquer projétil em popup/badsector/wall/obstacle → some sem destruir tile
- Produces: tiro inimigo em folder → some, pasta permanece

- [ ] **Step 1: Write failing tests**

```csharp
using ChamberCrawl.Models;
using ChamberCrawl.Systems;

namespace ChamberCrawl.Tests;

public class ObstacleCombatTests
{
    private static RunState BareRun()
    {
        var run = new RunState { Status = RunStatus.Playing, TimeMs = 0 };
        for (var y = 0; y < GameConstants.RoomH; y++)
            for (var x = 0; x < GameConstants.RoomW; x++)
                run.Room.Tiles[y, x] = GameConstants.TileFloor;
        run.PlayerStats = new PlayerStats { MaxHp = 6, Hp = 6, Damage = 1, FireRate = 4, Range = 320 };
        run.Entities.Add(new PlayerEntity
        {
            Id = "player", X = 48, Y = 48, Radius = GameConstants.PlayerRadius, Hp = 6,
        });
        return run;
    }

    [Fact]
    public void PlayerProjectile_DestroysFolder()
    {
        var run = BareRun();
        run.Room.Tiles[2, 3] = GameConstants.TileFolder;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p1", FromPlayer = true, X = 3.5f * GameConstants.Cell, Y = 2.5f * GameConstants.Cell,
            Vx = 0, Vy = 0, Radius = 4, Damage = 1, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), 0.016f);
        Assert.Equal(GameConstants.TileFloor, run.Room.Tiles[2, 3]);
        Assert.DoesNotContain(run.Entities, e => e.Id == "p1");
    }

    [Fact]
    public void PlayerProjectile_StopsOnBadSector_WithoutDestroying()
    {
        var run = BareRun();
        run.Room.Tiles[2, 3] = GameConstants.TileBadSector;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "p1", FromPlayer = true, X = 3.5f * GameConstants.Cell, Y = 2.5f * GameConstants.Cell,
            Vx = 0, Vy = 0, Radius = 4, Damage = 1, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), 0.016f);
        Assert.Equal(GameConstants.TileBadSector, run.Room.Tiles[2, 3]);
        Assert.DoesNotContain(run.Entities, e => e.Id == "p1");
    }

    [Fact]
    public void EnemyProjectile_DoesNotDestroyFolder()
    {
        var run = BareRun();
        run.Room.Tiles[2, 3] = GameConstants.TileFolder;
        run.Entities.Add(new ProjectileEntity
        {
            Id = "e1", FromPlayer = false, X = 3.5f * GameConstants.Cell, Y = 2.5f * GameConstants.Cell,
            Vx = 0, Vy = 0, Radius = 4, Damage = 1, Life = 1,
        });
        Combat.Update(run, new InputSnapshot(), 0.016f);
        Assert.Equal(GameConstants.TileFolder, run.Room.Tiles[2, 3]);
        Assert.DoesNotContain(run.Entities, e => e.Id == "e1");
    }
}
```

- [ ] **Step 2: Run — expect FAIL**

```bash
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~ObstacleCombatTests" --nologo
```

- [ ] **Step 3: Implement** em `UpdateProjectiles`, trocar o bloco que só chama `CircleHitsSolid` por:

```csharp
if (projectile.Life <= 0)
{
    removed.Add(projectile);
    continue;
}

if (Collision.TrySampleSolidTile(
        projectile.X, projectile.Y, projectile.Radius, state.Room.Tiles,
        out var hitTx, out var hitTy, out var hitTile))
{
    if (projectile.FromPlayer && hitTile == GameConstants.TileFolder)
        state.Room.Tiles[hitTy, hitTx] = GameConstants.TileFloor;
    removed.Add(projectile);
    continue;
}
```

Remover o `|| Collision.CircleHitsSolid(...)` antigo desse `if`.

- [ ] **Step 4: Run — expect PASS** (+ regressão `CombatTests`)

```bash
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~CombatTests|FullyQualifiedName~ObstacleCombatTests" --nologo
```

- [ ] **Step 5: Commit**

```bash
git add ChamberCrawl/Systems/Combat.cs ChamberCrawl.Tests/ObstacleCombatTests.cs
git commit -m "$(cat <<'EOF'
feat: pastas destruíveis e bad sector bloqueia tiros

EOF
)"
```

---

### Task 3: EnemyKind Partition C + defs + campos

**Files:**
- Modify: `ChamberCrawl/Models/GameTypes.cs`
- Modify: `ChamberCrawl/Data/Enemies.cs`
- Modify: testes que referenciam `Chaser`/`Shooter`/`Wander`/`Boss` (atualizar kinds ou manter aliases)

**Interfaces:**
- Produces: `enum EnemyKind { Pspam, Trojan, AdwareKing }`
- Produces: `EnemyEntity.CanSplit` (default `true` só faz sentido para Pspam; spawn seta)
- Produces: `EnemyEntity.IframeUntil` (float, ms do `TimeMs`)
- Produces: `ProjectileEntity.BitGlyph` (0 ou 1)
- Produces: `Enemies.Defs` para os três kinds

**Decisão:** remover kinds legados do enum. Atualizar **todos** os testes/AI que citam Chaser/Boss nesta e nas tasks seguintes (Task 4+ reescreve AI; nesta task só compilar: stubs temporários em `Ai` mapeando Trojan≈chaser e AdwareKing≈boss antigo **ou** comentar cases e deixar Task 4–6. Preferência: nesta task manter switch com `default: break` e mover lógica antiga para fora — testes Ai antigos quebram até Task 4–6. **Ordem:** nesta task atualizar `Enemies.Defs` + enum + campos; na mesma task ajustar `Ai.cs` para compilar com `default`; atualizar `CombatTests`/`CollisionTests`/`Doors` refs de kind para `Trojan` onde era Chaser; `RoomGen`/`RunFactoryTests`/`RoomGenTests` quebram até Task 7 — atualizar temporariamente boss spawn para `AdwareKing` e pool vazio? Melhor: Task 3 inclui atualização mínima de referências de compilação: `EnemyKind.Trojan` no lugar de Chaser nos testes de combate/colisão; AiTests reescritos nas tasks 4–6; RoomGen na Task 7.

- [ ] **Step 1: Change models + defs**

```csharp
public enum EnemyKind { Pspam, Trojan, AdwareKing }
```

Em `EnemyEntity`:

```csharp
public bool CanSplit { get; set; }
public float IframeUntil { get; set; }
```

Em `ProjectileEntity`:

```csharp
public int BitGlyph { get; set; }
```

`Enemies.cs`:

```csharp
[EnemyKind.Pspam] = new(2, 50, 1, 9, 0, 0, "enemy-pspam"),
[EnemyKind.Trojan] = new(3, 70, 1, 10, 0, 0, "enemy-trojan", ChargeSpeed: 140),
[EnemyKind.AdwareKing] = new(40, 45, 2, 24, 1200, 160, "enemy-adware-king", ChargeSpeed: 90),
```

- [ ] **Step 2: Fix compile** — `Ai.cs` switch só com os três kinds (stubs vazios ok); `EntityRenderer` boss size para `AdwareKing`; `RoomGen` pool/boss temporário `Trojan`/`AdwareKing`; substituir `EnemyKind.Chaser`→`Trojan`, `Boss`→`AdwareKing` nos testes que precisam compilar.

- [ ] **Step 3:**

```bash
dotnet build ChamberCrawl.sln --nologo
```

Expected: build OK (testes Ai antigos podem falhar runtime — reescrever nas tasks 4–6 ou deletar testes Chaser/Boss agora e recriar).

- [ ] **Step 4: Commit**

```bash
git add ChamberCrawl/Models/GameTypes.cs ChamberCrawl/Data/Enemies.cs ChamberCrawl/Systems/Ai.cs ChamberCrawl/Rendering/EntityRenderer.cs ChamberCrawl/World/RoomGen.cs ChamberCrawl.Tests
git commit -m "$(cat <<'EOF'
refactor: EnemyKind Partition C (Pspam, Trojan, AdwareKing)

EOF
)"
```

---

### Task 4: AI Pspam (wander + split)

**Files:**
- Modify: `ChamberCrawl/Systems/Ai.cs`
- Modify: `ChamberCrawl.Tests/AiTests.cs` (substituir testes Chaser/Wander relevantes)

**Interfaces:**
- Consumes: `EnemyEntity.CanSplit`, `Cooldown` como timer de split (ms)
- Produces: a cada 4000 ms, se `CanSplit` e HP>0: remove pai, spawna 2 filhos com `CanSplit=false`, HP=1, Radius*=0.7
- Produces: movimento errático = lógica atual de Wander
- Const: `Ai.PspamSplitMs = 4000f`

- [ ] **Step 1: Failing tests**

```csharp
[Fact]
public void Update_Pspam_SplitsIntoTwoChildrenAfter4Seconds()
{
    var run = OpenRoomRun();
    var parent = AddEnemy(run, EnemyKind.Pspam, 200, 160);
    parent.CanSplit = true;
    parent.Cooldown = 4000f;
    parent.Hp = 2;

    Ai.Update(run, 4.1f);

    Assert.DoesNotContain(run.Entities, e => e.Id == parent.Id);
    var kids = run.Entities.OfType<EnemyEntity>().Where(e => e.Enemy == EnemyKind.Pspam).ToList();
    Assert.Equal(2, kids.Count);
    Assert.All(kids, k => Assert.False(k.CanSplit));
    Assert.All(kids, k => Assert.Equal(1, k.Hp));
}

[Fact]
public void Update_PspamChild_DoesNotSplit()
{
    var run = OpenRoomRun();
    var child = AddEnemy(run, EnemyKind.Pspam, 200, 160);
    child.CanSplit = false;
    child.Cooldown = 4000f;
    Ai.Update(run, 5f);
    Assert.Contains(run.Entities, e => e.Id == child.Id);
    Assert.Single(run.Entities.OfType<EnemyEntity>());
}
```

- [ ] **Step 2: Run — FAIL**

```bash
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~AiTests.Update_Pspam" --nologo
```

- [ ] **Step 3: Implement `UpdatePspam`** — reutilizar wander para movimento; no início do update do Pspam: `enemy.Cooldown -= dt*1000`; se `<=0` e `CanSplit`: spawn 2 com offsets ±8 px, `CanSplit=false`, `Hp=1`, `Radius=def.Radius*0.7f`, `Cooldown=PspamSplitMs`, remover pai; senão se `!CanSplit` resetar cooldown alto; se `CanSplit` e ainda vivo após mover, quando cooldown zera faz split (init `Cooldown = PspamSplitMs` no spawn via RunFactory).

Spawn room: em `RunFactory`, `CanSplit = enemy.Enemy == EnemyKind.Pspam`.

- [ ] **Step 4: PASS + commit**

```bash
git commit -m "$(cat <<'EOF'
feat: AI do Pspam com split de uma geração

EOF
)"
```

---

### Task 5: AI Trojan (idle → charge → chase)

**Files:**
- Modify: `ChamberCrawl/Systems/Ai.cs`
- Modify: `ChamberCrawl.Tests/AiTests.cs`

**Interfaces:**
- `Phase == 0`: idle; `Phase == 1`: ativo
- Ativa se distância ao player ≤ `Ai.TrojanWakeRadius` (80)
- No frame da ativação: `Vx/Vy` = direção * `ChargeSpeed` (140); depois `Steer` com `Speed` (70)
- Const: `TrojanWakeRadius = 80f`

- [ ] **Step 1: Tests**

```csharp
[Fact]
public void Update_Trojan_StaysIdleFarAway()
{
    var run = OpenRoomRun();
    var enemy = AddEnemy(run, EnemyKind.Trojan, 100, 160);
    enemy.Phase = 0;
    run.Entities.OfType<PlayerEntity>().Single().X = 300;
    Ai.Update(run, Dt);
    Assert.Equal(0, enemy.Phase);
    Assert.Equal(100, enemy.X);
}

[Fact]
public void Update_Trojan_ChargesWhenPlayerClose()
{
    var run = OpenRoomRun();
    var enemy = AddEnemy(run, EnemyKind.Trojan, 200, 160);
    enemy.Phase = 0;
    run.Entities.OfType<PlayerEntity>().Single().X = 250;
    Ai.Update(run, Dt);
    Assert.Equal(1, enemy.Phase);
    Assert.True(enemy.X > 200);
}
```

- [ ] **Step 2–4:** implementar `UpdateTrojan`, PASS, commit `feat: AI do Trojan com emboscada`

---

### Task 6: AI Adware King (3 padrões)

**Files:**
- Modify: `ChamberCrawl/Systems/Ai.cs`
- Modify: `ChamberCrawl/Systems/Combat.cs` (ignorar dano se `TimeMs < enemy.IframeUntil`; expor `DamagePlayer` interno via shockwave helper se necessário)
- Modify: `ChamberCrawl.Tests/AiTests.cs`

**Interfaces / máquina de estados** (usar `Phase` como índice de estágio 0..5, `Cooldown` como ms restantes no estágio):

| Phase | Nome | Duração | Ação |
|------:|------|---------|------|
| 0 | Idle | 800 | parado |
| 1 | Buttons | 1200 | no **enter**: 12 projéteis 360°; fica parado |
| 2 | Idle | 800 | parado |
| 3 | Spam | 200 | no enter: spawn 3 Pspam `CanSplit=false` HP1 em tiles floor livres |
| 4 | Idle | 800 | parado |
| 5 | Resize | 3000 | no enter: `IframeUntil = TimeMs+3000`, move a ChargeSpeed a um canto; no **exit**: se player em raio 48, aplicar dano de contato do boss |

Após phase 5, volta a 0. Avançar phase quando `Cooldown <= 0`.

Consts: `AdwareIdleMs=800`, `AdwareButtonsMs=1200`, `AdwareSpamMs=200`, `AdwareResizeMs=3000`, `AdwareShockRadius=48`, `AdwareBurstCount=12`.

- [ ] **Step 1: Tests** (mínimo)

```csharp
[Fact]
public void Update_AdwareKing_FiresButtonBurstOnEnterPhase1()
{
    var run = OpenRoomRun();
    var boss = AddEnemy(run, EnemyKind.AdwareKing, 200, 160, 24);
    boss.Hp = 40;
    boss.Phase = 0;
    boss.Cooldown = 0; // força troca para phase 1 no update
    Ai.Update(run, Dt);
    Assert.Equal(1, boss.Phase);
    Assert.Equal(12, run.Entities.OfType<ProjectileEntity>().Count());
}

[Fact]
public void Update_AdwareKing_SpawnsThreePspamsOnPhase3()
{
    var run = OpenRoomRun();
    var boss = AddEnemy(run, EnemyKind.AdwareKing, 200, 160, 24);
    boss.Phase = 2;
    boss.Cooldown = 0;
    Ai.Update(run, Dt);
    Assert.Equal(3, run.Entities.OfType<EnemyEntity>().Count(e => e.Enemy == EnemyKind.Pspam));
    Assert.All(
        run.Entities.OfType<EnemyEntity>().Where(e => e.Enemy == EnemyKind.Pspam),
        p => Assert.False(p.CanSplit));
}

[Fact]
public void Combat_IgnoresDamageWhileEnemyIframes()
{
    var run = OpenRoomRun();
    var boss = AddEnemy(run, EnemyKind.AdwareKing, 200, 160, 24);
    boss.Hp = 40;
    boss.IframeUntil = 10_000;
    run.TimeMs = 100;
    run.Entities.Add(new ProjectileEntity
    {
        Id = "hit", FromPlayer = true, X = 200, Y = 160, Vx = 0, Vy = 0,
        Radius = 4, Damage = 5, Life = 1,
    });
    Combat.Update(run, new InputSnapshot(), Dt);
    Assert.Equal(40, boss.Hp);
}
```

`OpenRoomRun` nos AiTests deve setar `Status = Playing` e tiles abertos (já existe).

- [ ] **Step 2–4:** implementar, PASS, commit `feat: AI do Adware King com tres padroes`

Em `Combat` ao acertar inimigo:

```csharp
if (state.TimeMs < target.IframeUntil) { removed.Add(projectile); continue; }
```

Shockwave no exit do phase 5: se overlap player com raio `AdwareShockRadius`, chamar lógica de dano (extrair `DamagePlayer` para `internal`/`public` static se ainda private — tornar `public static` para teste/AI ou método `Combat.TryContactDamage(state, amount)`).

---

### Task 7: RoomGen Partition C

**Files:**
- Modify: `ChamberCrawl/World/RoomGen.cs`
- Modify: `ChamberCrawl.Tests/RoomGenTests.cs`

**Interfaces:**
- Pool normal: só `Pspam`, `Trojan`
- Boss: `AdwareKing`
- Obstáculos em Normal: escolher aleatoriamente entre `TileFolder`, `TilePopup`, `TileBadSector` (não `TileObstacle`)
- Garantir que em N seeds haja pelo menos um de cada tipo ao longo de várias gerações **ou** assert por sala com `obstacleCount` tiles ∈ {4,5,6}

- [ ] **Step 1: Tests**

```csharp
[Fact]
public void Generate_Normal_UsesPartitionEnemyPoolAndObstacleTiles()
{
    var room = RoomGen.Generate(Node(RoomType.Normal, seed: 7), new[] { Dir.N, Dir.E });
    var enemies = room.Spawns.OfType<EnemySpawn>().Select(s => s.Enemy).ToHashSet();
    Assert.Subset(new HashSet<EnemyKind> { EnemyKind.Pspam, EnemyKind.Trojan }, enemies);
    Assert.DoesNotContain(EnemyKind.AdwareKing, enemies);

    var obstacleTiles = new HashSet<int>();
    for (var y = 0; y < GameConstants.RoomH; y++)
        for (var x = 0; x < GameConstants.RoomW; x++)
        {
            var t = room.Tiles[y, x];
            if (t is GameConstants.TileFolder or GameConstants.TilePopup or GameConstants.TileBadSector)
                obstacleTiles.Add(t);
        }
    Assert.NotEmpty(obstacleTiles);
}

[Fact]
public void Generate_Boss_SpawnsAdwareKing()
{
    var boss = RoomGen.Generate(Node(RoomType.Boss), new[] { Dir.N });
    Assert.Contains(boss.Spawns.OfType<EnemySpawn>(), s => s.Enemy == EnemyKind.AdwareKing);
}
```

Ajustar helper `Node` existente se a assinatura mudar.

- [ ] **Step 2–4:** implementar pool/obstáculos, PASS, commit `feat: RoomGen Partition C`

Em `RunFactory` spawn: `CanSplit = enemy.Enemy == EnemyKind.Pspam`.

Atualizar `RunFactoryTests` para `AdwareKing` e Phase 0.

---

### Task 8: Progressão — vitória no depth 1

**Files:**
- Modify: `ChamberCrawl/Systems/Progression.cs`
- Modify: `ChamberCrawl.Tests/ProgressionTests.cs`

**Interfaces:**
- `Update`: se boss room limpa → `status = Won` (qualquer depth da fatia 1; remover branch portal/`AdvanceFloor` do caminho feliz)
- Manter `AdvanceFloor` no arquivo **sem uso** ou deletar se nenhum teste/chama — deletar para YAGNI da fatia 1 (roadmap reintroduz)

- [ ] **Step 1: Replace ProgressionTests**

```csharp
[Fact]
public void Update_WinsOnDepth1BossClear()
{
    var state = InBossRoom(1);
    Progression.Update(state);
    Assert.Equal(RunStatus.Won, state.Status);
    Assert.DoesNotContain(state.Entities, e => e.Id == "portal");
}

[Fact]
public void Update_DoesNotWinWhileBossAlive()
{
    var state = RunFactory.Create(42);
    RunFactory.EnterRoom(state, state.Floor.BossRoomId, null);
    Progression.Update(state);
    Assert.Equal(RunStatus.Playing, state.Status);
}
```

Remover testes de portal / depth 2.

- [ ] **Step 2–4:**

```csharp
public static void Update(RunState state)
{
    if (state.Status != RunStatus.Playing) return;
    if (!state.Floor.Rooms.TryGetValue(state.CurrentRoomId, out var node)) return;
    if (node.Type != RoomType.Boss || !Doors.IsRoomCleared(state.Entities)) return;
    state.Status = RunStatus.Won;
}
```

Commit: `feat: vitoria no Adware King sem portal`

Atualizar `PickupsTests` se dependerem de portal persistir — portal como pickup kind pode permanecer no enum sem spawn.

---

### Task 9: Bits (spawn do topo + BitGlyph)

**Files:**
- Modify: `ChamberCrawl/Systems/Combat.cs` (`UpdatePlayerShooting`)
- Modify: `ChamberCrawl/Rendering/EntityRenderer.cs`
- Modify: `ChamberCrawl.Tests/CombatTests.cs` (assert Y do projétil < player.Y e BitGlyph 0/1)

- [ ] **Step 1: Test**

```csharp
[Fact]
public void PlayerShot_SpawnsFromTopWithAlternatingBitGlyph()
{
    var run = BareCombatRun(); // helper existente ou criar
    var player = run.Entities.OfType<PlayerEntity>().Single();
    Combat.Update(run, new InputSnapshot { ShootX = 1, ShootY = 0 }, 0.016f);
    var shots = run.Entities.OfType<ProjectileEntity>().Where(p => p.FromPlayer).ToList();
    Assert.NotEmpty(shots);
    Assert.All(shots, s => Assert.True(s.Y < player.Y));
    Assert.Contains(shots, s => s.BitGlyph is 0 or 1);
}
```

- [ ] **Step 2–4:** spawn em `Y = player.Y - player.Radius * 0.75f`; `BitGlyph = projectileCounter & 1`; renderer `atlas.Get(proj.BitGlyph == 0 ? "bit-0" : "bit-1")` para FromPlayer. Commit `feat: Bits saem do topo do Diskette`

---

### Task 10: Rendering theme + branding

**Files:**
- Create: `ChamberCrawl/Data/FloorThemes.cs`
- Modify: `ChamberCrawl/Rendering/TilemapRenderer.cs`
- Modify: `ChamberCrawl/Rendering/EntityRenderer.cs`
- Modify: `ChamberCrawl/Rendering/MenuRenderer.cs`
- Modify: `ChamberCrawl/Game1.cs` (`Window.Title = "Diskette"` em `Initialize`)
- Modify: `README.md`

**FloorThemes:**

```csharp
namespace ChamberCrawl.Data;

public static class FloorThemes
{
    public static string TextureForTile(int tile) => tile switch
    {
        GameConstants.TileWall => "wall",
        GameConstants.TileDoor => "door-tile",
        GameConstants.TileFolder => "folder",
        GameConstants.TilePopup => "popup",
        GameConstants.TileBadSector => "bad-sector",
        GameConstants.TileObstacle => "obstacle",
        _ => "floor",
    };
}
```

Trojan: `Phase==0` → `enemy-trojan`, senão `enemy-trojan-active`.  
AdwareKing: tamanho draw `radius * 3.2` (mini: se `IframeUntil > TimeMs` durante resize, scale `* 0.45f`).

Menu title: `"DISKETTE"`.  
README: título Diskette, objetivo Partition C / Adware King, remover menção a portal entre andares.

- [ ] Build + commit `feat: branding Diskette e tiles Partition C no render`

---

### Task 11: Gerar e instalar sprites PNG

**Files:**
- Replace/Create under `ChamberCrawl/Assets/Textures/`:
  `player.png`, `bit-0.png`, `bit-1.png`, `floor.png`, `wall.png`, `door.png`, `door-tile.png`, `folder.png`, `popup.png`, `bad-sector.png`, `enemy-pspam.png`, `enemy-trojan.png`, `enemy-trojan-active.png`, `enemy-adware-king.png`

**Como:** usar Cursor `GenerateImage` (namespace `cursor`) com prompts pixel-art 32×32 (boss ~64×64), fundo transparente, paleta Win95 pastel. Salvar/copiar para `Assets/Textures/` com os nomes exatos (atlas carrega por filename).

Prompts base (ajustar por asset):

- Diskette: "32x32 pixel art game sprite, blue faded 3.5 inch floppy disk character with pixel eyes on torn paper label and two short legs, transparent background, top-down-ish readable silhouette"
- bit-0 / bit-1: "16x16 pixel art yellow glowing square with a black digit 0 (or 1), transparent background"
- floor: "32x32 seamless tile soft green pastel 1990s OS desktop pattern"
- wall: "32x32 pixel art Windows 95 window chrome gray beveled border with blue title bar strip on top"
- folder: "32x32 yellow classic folder icon pixel art"
- popup: "32x32 tiny error dialog window pixel art"
- bad-sector: "32x32 black square with yellow question mark pixel art"
- pspam: "32x32 tiny colorful adware popup window floating pixel art"
- trojan: "32x32 pixel gift box treasure chest closed"
- trojan-active: "32x32 pixel gift box with angry syntax teeth"
- adware-king: "64x64 pixel art giant floating Windows dialog full of banners and fake buttons"

Manter `coin`, `heart`, itens, `projectile` inimigo: se `projectile.png` sumir, EntityRenderer de inimigo usa `projectile` com tint — **manter** `projectile.png` para tiros inimigos ou apontar para `bit-0` tintado.

- [ ] Após copiar: `dotnet run --project ChamberCrawl` smoke visual
- [ ] Commit: `feat: sprites Partition C gerados para Diskette`

---

### Task 12: Suíte completa + limpeza

- [ ] **Step 1:**

```bash
dotnet test ChamberCrawl.sln --nologo
dotnet build ChamberCrawl.sln --nologo
```

Expected: todos verdes.

- [ ] **Step 2:** Remover código morto óbvio (`AdvanceFloor` se ainda existir, defs/texturas órfãs só se seguro). Não deletar itens/portal kind.

- [ ] **Step 3:** Commit final se houver leftovers `test: suíte verde após retema Partition C` ou `chore: limpeza pós Partition C`

---

## Self-review do plano vs spec

| Requisito spec | Task |
|----------------|------|
| Branding Diskette | 10 |
| 1 andar, win no boss | 8 |
| Diskette + Bits | 9, 11 |
| Folder/Popup/BadSector | 1, 2, 7, 10, 11 |
| Pspam split 1 geração | 4 |
| Trojan wake/charge | 5 |
| Adware King 3 padrões | 6 |
| RoomGen pool | 7 |
| Arte Cursor | 11 |
| Sem áudio fatia 1 | (explícito: não há task de áudio) |
| Testes listados | 1–8 |
| Roadmap andares 2–4 | fora do plano (só spec) |

Ambiguidades fechadas no plano: onda = raio 48; pasta só tiro jogador; mini boss = scale no draw; sem `AdvanceFloor` na fatia 1.
