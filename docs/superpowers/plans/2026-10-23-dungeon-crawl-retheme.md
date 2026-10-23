# Dungeon Crawl Retheme Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Substituir a temática Diskette/Partition C por Dungeon Crawl medieval: domínio renomeado, AIs Bat/Skeleton/Slime/Ogre, obstáculos crate/barrel/spike, bolt mágico, sprites via Sprite Studio.

**Architecture:** Manter o loop MonoGame atual (um andar, vitória no boss sem portal). Renomear tiles e `EnemyKind`, reescrever `Ai.cs`, atualizar RoomGen/Combat/Render/docs, e regenerar texturas no Sprite Studio. Namespaces e pasta `ChamberCrawl` permanecem.

**Tech Stack:** C# / .NET 9, MonoGame DesktopGL, xUnit, Sprite Studio (`tools/sprite-studio`, Canvas + bake PNG).

## Global Constraints

- Spec: `docs/superpowers/specs/2026-10-23-dungeon-crawl-retheme-design.md`
- Nome de exibição: **Dungeon Crawl** (janela, menu, README). Código: `ChamberCrawl`
- Depth 1 only. Boss limpo → `Won`. Sem portal / sem `AdvanceFloor`
- TDD: teste falhando → implementação → verde → commit por task
- Commits Conventional Commits, descrição em português
- GlyphFont: A-Z, 0-9, espaço → título menu `DUNGEON CRAWL`
- Sem áudio, sem depths 2+, sem rename de namespaces

## File map

| File | Role |
|------|------|
| `ChamberCrawl/Models/Constants.cs` | `TileCrate`, `TileBarrel`, `TileSpike` |
| `ChamberCrawl/Models/GameTypes.cs` | `EnemyKind` Bat/Skeleton/Slime/Ogre; remover `BitGlyph` |
| `ChamberCrawl/Data/Enemies.cs` | Defs + texturas novas |
| `ChamberCrawl/Data/Items.cs` | Nomes medievais |
| `ChamberCrawl/Data/FloorThemes.cs` | Lookup crate/barrel/spike |
| `ChamberCrawl/Systems/Collision.cs` | Sólidos novos |
| `ChamberCrawl/Systems/Combat.cs` | Destrói crate; bolt sem BitGlyph |
| `ChamberCrawl/Systems/Ai.cs` | Bat / Skeleton / Slime / Ogre |
| `ChamberCrawl/Systems/RunFactory.cs` | CanSplit Slime; cooldowns |
| `ChamberCrawl/World/RoomGen.cs` | Pool + obstáculos dungeon |
| `ChamberCrawl/Rendering/EntityRenderer.cs` | bolt, ogre size, kinds |
| `ChamberCrawl/Game1.cs`, `MenuRenderer.cs`, `README.md`, `docs/SPRITE_STYLE.md` | Branding |
| `tools/sprite-studio/server.js` | SIZE_DEFAULTS novos ids |
| `Assets/SpriteSources/**`, `Assets/Textures/**` | Arte medieval |
| `ChamberCrawl.Tests/*` | Testes atualizados |

---

### Task 1: Tiles Crate / Barrel / Spike

**Files:**
- Modify: `ChamberCrawl/Models/Constants.cs`
- Modify: `ChamberCrawl/Systems/Collision.cs`
- Modify: `ChamberCrawl/Systems/Combat.cs`
- Modify: `ChamberCrawl/Data/FloorThemes.cs`
- Modify: `ChamberCrawl.Tests/CollisionTests.cs`
- Modify: `ChamberCrawl.Tests/ObstacleCombatTests.cs`
- Modify: `ChamberCrawl.Tests/FloorThemesTests.cs`

**Interfaces:**
- Produces: `GameConstants.TileCrate = 4`, `TileBarrel = 5`, `TileSpike = 6` (remover Folder/Popup/BadSector)
- Produces: `Collision.IsSolidTile` inclui crate/barrel/spike
- Produces: tiro do jogador em `TileCrate` → `TileFloor`

- [ ] **Step 1: Atualizar testes** para os novos nomes

Em `CollisionTests.cs` trocar InlineData/asserts `TileFolder`→`TileCrate`, `TilePopup`→`TileBarrel`, `TileBadSector`→`TileSpike`.

Em `ObstacleCombatTests.cs`:

```csharp
[Fact]
public void PlayerProjectile_DestroysCrate()
{
    var run = BareRun();
    run.Room.Tiles[2, 3] = GameConstants.TileCrate;
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
public void PlayerProjectile_StopsOnSpike_WithoutDestroying()
{
    var run = BareRun();
    run.Room.Tiles[2, 3] = GameConstants.TileSpike;
    run.Entities.Add(new ProjectileEntity
    {
        Id = "p1", FromPlayer = true, X = 3.5f * GameConstants.Cell, Y = 2.5f * GameConstants.Cell,
        Vx = 0, Vy = 0, Radius = 4, Damage = 1, Life = 1,
    });
    Combat.Update(run, new InputSnapshot(), 0.016f);
    Assert.Equal(GameConstants.TileSpike, run.Room.Tiles[2, 3]);
    Assert.DoesNotContain(run.Entities, e => e.Id == "p1");
}

[Fact]
public void EnemyProjectile_DoesNotDestroyCrate()
{
    var run = BareRun();
    run.Room.Tiles[2, 3] = GameConstants.TileCrate;
    run.Entities.Add(new ProjectileEntity
    {
        Id = "e1", FromPlayer = false, X = 3.5f * GameConstants.Cell, Y = 2.5f * GameConstants.Cell,
        Vx = 0, Vy = 0, Radius = 4, Damage = 1, Life = 1,
    });
    Combat.Update(run, new InputSnapshot(), 0.016f);
    Assert.Equal(GameConstants.TileCrate, run.Room.Tiles[2, 3]);
    Assert.DoesNotContain(run.Entities, e => e.Id == "e1");
}
```

Em `FloorThemesTests.cs`:

```csharp
[InlineData(GameConstants.TileCrate, "crate")]
[InlineData(GameConstants.TileBarrel, "barrel")]
[InlineData(GameConstants.TileSpike, "spike")]
```

- [ ] **Step 2: Rodar testes — expect FAIL** (constantes antigas / nomes)

```bash
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~CollisionTests|FullyQualifiedName~ObstacleCombatTests|FullyQualifiedName~FloorThemesTests" --nologo
```

- [ ] **Step 3: Implementar**

`Constants.cs`:

```csharp
public const int TileCrate = 4;
public const int TileBarrel = 5;
public const int TileSpike = 6;
```

`Collision.IsSolidTile`:

```csharp
public static bool IsSolidTile(int tile) =>
    tile is GameConstants.TileWall
        or GameConstants.TileObstacle
        or GameConstants.TileCrate
        or GameConstants.TileBarrel
        or GameConstants.TileSpike;
```

`Combat.cs` linha do folder:

```csharp
if (projectile.FromPlayer && hitTile == GameConstants.TileCrate)
    state.Room.Tiles[hitTy, hitTx] = GameConstants.TileFloor;
```

`FloorThemes.cs` (ambos lookups):

```csharp
GameConstants.TileCrate => "crate",
GameConstants.TileBarrel => "barrel",
GameConstants.TileSpike => "spike",
```

Atualizar qualquer referência restante a Folder/Popup/BadSector neste passo **exceto** RoomGen (Task 7) — se RoomGen quebrar o build, trocar só os nomes de constantes lá para compilar, deixando a lógica Trojan/AdwareKing para Tasks 2–7.

- [ ] **Step 4: PASS + commit**

```bash
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~CollisionTests|FullyQualifiedName~ObstacleCombatTests|FullyQualifiedName~FloorThemesTests" --nologo
git add ChamberCrawl/Models/Constants.cs ChamberCrawl/Systems/Collision.cs ChamberCrawl/Systems/Combat.cs ChamberCrawl/Data/FloorThemes.cs ChamberCrawl.Tests/CollisionTests.cs ChamberCrawl.Tests/ObstacleCombatTests.cs ChamberCrawl.Tests/FloorThemesTests.cs ChamberCrawl/World/RoomGen.cs
git commit -m "$(cat <<'EOF'
refactor: tiles crate, barrel e spike

EOF
)"
```

---

### Task 2: EnemyKind + Enemies.Defs

**Files:**
- Modify: `ChamberCrawl/Models/GameTypes.cs`
- Modify: `ChamberCrawl/Data/Enemies.cs`
- Modify: todos os `.cs` / testes que citam `Pspam`/`Trojan`/`AdwareKing` o mínimo para **compilar** (stubs Ai ok até Tasks 3–6)

**Interfaces:**
- Produces: `enum EnemyKind { Bat, Skeleton, Slime, Ogre }`
- Produces:

```csharp
[EnemyKind.Bat] = new(3, 70, 1, 10, 0, 0, "enemy-bat"),
[EnemyKind.Skeleton] = new(3, 40, 1, 10, 1200, 160, "enemy-skeleton", PreferredDist: 90),
[EnemyKind.Slime] = new(2, 50, 1, 9, 0, 0, "enemy-slime"),
[EnemyKind.Ogre] = new(40, 45, 2, 24, 1200, 160, "enemy-ogre", ChargeSpeed: 90),
```

- [ ] **Step 1: Trocar enum + Defs**

```csharp
public enum EnemyKind { Bat, Skeleton, Slime, Ogre }
```

- [ ] **Step 2: Compilar**

Em `Ai.cs`: switch com os 4 kinds e corpos vazios `break` (ou chamar stubs). Remover métodos Adware/Pspam/Trojan **ou** deixar e não referenciar até Tasks 3–6 reescreverem o arquivo.

Em `RoomGen`: pool temporário `Bat, Skeleton, Slime`, boss `Ogre`, remover `PickTrojanTile` usage (usar spawn genérico).

Em `RunFactory`:

```csharp
Cooldown = enemy.Enemy switch
{
    EnemyKind.Slime => Ai.SlimeSplitMs, // adicionar const temporária = 4000f se Ai ainda não tiver
    EnemyKind.Ogre => 0,
    _ => def.FireCooldownMs,
},
CanSplit = enemy.Enemy == EnemyKind.Slime,
```

Em testes: substituir kinds (Pspam→Slime, Trojan→Bat, AdwareKing→Ogre) nos arquivos que precisam compilar. Testes de comportamento antigo do Trojan/Adware **falharão** até Tasks 3–6 — nesta task, comentar/remover asserts específicos de Trojan charge e Adware burst **ou** deixar falhando e só garantir `dotnet build`. Preferência: nesta task só `dotnet build` verde; AiTests de comportamento reescritos nas tasks seguintes.

```bash
dotnet build ChamberCrawl/ChamberCrawl.csproj --nologo
dotnet build ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --nologo
```

- [ ] **Step 3: Commit**

```bash
git add -u ChamberCrawl ChamberCrawl.Tests
git commit -m "$(cat <<'EOF'
refactor: EnemyKind Bat, Skeleton, Slime, Ogre

EOF
)"
```

---

### Task 3: AI Slime (multiply)

**Files:**
- Modify: `ChamberCrawl/Systems/Ai.cs`
- Modify: `ChamberCrawl.Tests/AiTests.cs`

**Interfaces:**
- Produces: `Ai.SlimeSplitMs = 4000f`
- Produces: `UpdateSlime` (wander + split uma geração), espelho do antigo Pspam com `EnemyKind.Slime`

- [ ] **Step 1: Testes**

```csharp
[Fact]
public void Update_Slime_SplitsIntoTwoChildrenAfter4Seconds()
{
    var run = OpenRoomRun();
    var parent = AddEnemy(run, EnemyKind.Slime, 200, 160);
    parent.CanSplit = true;
    parent.Cooldown = 4000f;
    parent.Hp = 2;

    Ai.Update(run, 4.1f);

    Assert.DoesNotContain(run.Entities, e => e.Id == parent.Id);
    var kids = run.Entities.OfType<EnemyEntity>().Where(e => e.Enemy == EnemyKind.Slime).ToList();
    Assert.Equal(2, kids.Count);
    Assert.All(kids, k => Assert.False(k.CanSplit));
    Assert.All(kids, k => Assert.Equal(1, k.Hp));
}

[Fact]
public void Update_SlimeChild_DoesNotSplit()
{
    var run = OpenRoomRun();
    var child = AddEnemy(run, EnemyKind.Slime, 200, 160);
    child.CanSplit = false;
    child.Cooldown = 4000f;
    Ai.Update(run, 5f);
    Assert.Contains(run.Entities, e => e.Id == child.Id);
    Assert.Single(run.Entities.OfType<EnemyEntity>());
}
```

- [ ] **Step 2: FAIL → implementar UpdateSlime/SplitSlime/wander (copiar Pspam, trocar kind/const) → PASS**

```bash
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~AiTests.Update_Slime" --nologo
```

- [ ] **Step 3: Commit** `feat: AI do Slime com split de uma geracao`

---

### Task 4: AI Bat (chase only)

**Files:**
- Modify: `ChamberCrawl/Systems/Ai.cs`
- Modify: `ChamberCrawl.Tests/AiTests.cs`

**Interfaces:**
- Produces: `UpdateBat` — sempre `Steer` em direção ao player com `Enemies.Defs[Bat].Speed`. Sem idle, sem charge.

- [ ] **Step 1: Teste**

```csharp
[Fact]
public void Update_Bat_ChasesPlayer()
{
    var run = OpenRoomRun();
    var enemy = AddEnemy(run, EnemyKind.Bat, 100, 160);
    run.Entities.OfType<PlayerEntity>().Single().X = 300;
    var before = enemy.X;
    Ai.Update(run, Dt);
    Assert.True(enemy.X > before);
}
```

Remover testes Trojan idle/charge.

- [ ] **Step 2: Implementar**

```csharp
private static void UpdateBat(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
{
    var def = Enemies.Defs[EnemyKind.Bat];
    var dx = player.X - enemy.X;
    var dy = player.Y - enemy.Y;
    var dist = MathF.Sqrt(dx * dx + dy * dy);
    if (dist == 0) return;
    Steer(state, enemy, dx / dist, dy / dist, def.Speed, dt);
}
```

- [ ] **Step 3: PASS + commit** `feat: AI do Bat so persegue`

---

### Task 5: AI Skeleton (shoot + distância)

**Files:**
- Modify: `ChamberCrawl/Systems/Ai.cs`
- Modify: `ChamberCrawl.Tests/AiTests.cs`

**Interfaces:**
- Produces: `UpdateSkeleton` — move para manter `PreferredDist` (~90); `Cooldown` conta fire rate; ao zerar chama `Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage)` e reseta `Cooldown = def.FireCooldownMs`.

- [ ] **Step 1: Testes**

```csharp
[Fact]
public void Update_Skeleton_FiresProjectileAfterCooldown()
{
    var run = OpenRoomRun();
    var enemy = AddEnemy(run, EnemyKind.Skeleton, 200, 160);
    enemy.Cooldown = 0;
    Ai.Update(run, Dt);
    Assert.Single(run.Entities.OfType<ProjectileEntity>());
    Assert.False(run.Entities.OfType<ProjectileEntity>().Single().FromPlayer);
    Assert.Equal(Enemies.Defs[EnemyKind.Skeleton].FireCooldownMs, enemy.Cooldown);
}

[Fact]
public void Update_Skeleton_MovesAwayWhenTooClose()
{
    var run = OpenRoomRun();
    var player = run.Entities.OfType<PlayerEntity>().Single();
    player.X = 200; player.Y = 160;
    var enemy = AddEnemy(run, EnemyKind.Skeleton, 210, 160);
    enemy.Cooldown = 99999f;
    var before = enemy.X;
    Ai.Update(run, Dt);
    Assert.True(enemy.X > before); // afasta para a direita
}
```

- [ ] **Step 2: Implementar**

```csharp
private static void UpdateSkeleton(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
{
    var def = Enemies.Defs[EnemyKind.Skeleton];
    var dx = player.X - enemy.X;
    var dy = player.Y - enemy.Y;
    var dist = MathF.Sqrt(dx * dx + dy * dy);
    var preferred = def.PreferredDist ?? 90f;

    if (dist > 0)
    {
        float dirX, dirY;
        if (dist > preferred + 8f) { dirX = dx / dist; dirY = dy / dist; }
        else if (dist < preferred - 8f) { dirX = -dx / dist; dirY = -dy / dist; }
        else { dirX = 0; dirY = 0; }
        if (dirX != 0 || dirY != 0)
            Steer(state, enemy, dirX, dirY, def.Speed, dt);
    }

    enemy.Cooldown -= dt * 1000f;
    if (enemy.Cooldown > 0) return;
    Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage);
    enemy.Cooldown = def.FireCooldownMs;
}
```

- [ ] **Step 3: PASS + commit** `feat: AI do Skeleton atirador`

---

### Task 6: AI Ogre (2 fases)

**Files:**
- Modify: `ChamberCrawl/Systems/Ai.cs`
- Modify: `ChamberCrawl.Tests/AiTests.cs`
- Modify: `ChamberCrawl.Tests/RunFactoryTests.cs` (cooldown inicial Ogre)

**Interfaces:**
- Produces: `Ai.OgrePhase1MaxMs = 8000f`
- Produces: Phase 0 = chase (`ChargeSpeed` ?? Speed). Transição para Phase 1 se `Hp <= Defs.Hp * 0.5f` **ou** `Cooldown <= 0` (Cooldown inicia em `OgrePhase1MaxMs` no spawn).
- Produces: Phase 1 = anda lento (`Speed`) + atira como Skeleton com `FireCooldownMs`.
- Removes: burst 12, spam adds, resize, shockwave.

- [ ] **Step 1: Testes**

```csharp
[Fact]
public void Update_Ogre_ChasesInPhase0()
{
    var run = OpenRoomRun();
    var boss = AddEnemy(run, EnemyKind.Ogre, 100, 160, 24);
    boss.Phase = 0;
    boss.Cooldown = Ai.OgrePhase1MaxMs;
    boss.Hp = 40;
    run.Entities.OfType<PlayerEntity>().Single().X = 300;
    var before = boss.X;
    Ai.Update(run, Dt);
    Assert.Equal(0, boss.Phase);
    Assert.True(boss.X > before);
}

[Fact]
public void Update_Ogre_EntersPhase1AtHalfHp()
{
    var run = OpenRoomRun();
    var boss = AddEnemy(run, EnemyKind.Ogre, 200, 160, 24);
    boss.Phase = 0;
    boss.Cooldown = Ai.OgrePhase1MaxMs;
    boss.Hp = Enemies.Defs[EnemyKind.Ogre].Hp * 0.5f;
    Ai.Update(run, Dt);
    Assert.Equal(1, boss.Phase);
}

[Fact]
public void Update_Ogre_ShootsInPhase1()
{
    var run = OpenRoomRun();
    var boss = AddEnemy(run, EnemyKind.Ogre, 200, 160, 24);
    boss.Phase = 1;
    boss.Cooldown = 0;
    Ai.Update(run, Dt);
    Assert.NotEmpty(run.Entities.OfType<ProjectileEntity>());
}
```

Remover testes AdwareKing burst/spam.

`RunFactoryTests`: boss `EnemyKind.Ogre`, cooldown inicial `Ai.OgrePhase1MaxMs`.

- [ ] **Step 2: Implementar UpdateOgre; limpar código Adware do Ai.cs**

```csharp
public const float OgrePhase1MaxMs = 8000f;

private static void UpdateOgre(RunState state, EnemyEntity enemy, PlayerEntity player, float dt)
{
    var def = Enemies.Defs[EnemyKind.Ogre];
    if ((int)enemy.Phase == 0)
    {
        enemy.Cooldown -= dt * 1000f;
        if (enemy.Hp <= def.Hp * 0.5f || enemy.Cooldown <= 0)
        {
            enemy.Phase = 1;
            enemy.Cooldown = def.FireCooldownMs;
        }
        else
        {
            var dx = player.X - enemy.X;
            var dy = player.Y - enemy.Y;
            var dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist > 0)
                Steer(state, enemy, dx / dist, dy / dist, def.ChargeSpeed ?? def.Speed, dt);
            return;
        }
    }

    // Phase 1: slow chase + shoot
    var dx1 = player.X - enemy.X;
    var dy1 = player.Y - enemy.Y;
    var dist1 = MathF.Sqrt(dx1 * dx1 + dy1 * dy1);
    if (dist1 > 0)
        Steer(state, enemy, dx1 / dist1, dy1 / dist1, def.Speed, dt);

    enemy.Cooldown -= dt * 1000f;
    if (enemy.Cooldown > 0) return;
    Combat.SpawnEnemyProjectile(state, enemy, player.X, player.Y, def.Damage);
    enemy.Cooldown = def.FireCooldownMs;
}
```

`RunFactory` spawn Ogre: `Cooldown = Ai.OgrePhase1MaxMs`, `Phase = 0`.

- [ ] **Step 3: PASS suite Ai + RunFactory + commit** `feat: AI do Ogre em duas fases`

```bash
dotnet test ChamberCrawl.Tests/ChamberCrawl.Tests.csproj --filter "FullyQualifiedName~AiTests|FullyQualifiedName~RunFactoryTests" --nologo
```

---

### Task 7: RoomGen dungeon

**Files:**
- Modify: `ChamberCrawl/World/RoomGen.cs`
- Modify: `ChamberCrawl.Tests/RoomGenTests.cs`

**Interfaces:**
- Produces: pool `{ Bat, Skeleton, Slime }`, boss `Ogre`, obstáculos `{ TileCrate, TileBarrel, TileSpike }`
- Removes: `PickTrojanTile` e regra de canto

- [ ] **Step 1: Testes**

```csharp
[Fact]
public void Generate_Normal_UsesDungeonEnemyPoolAndObstacleTiles()
{
    // seed estável; assert subset Bat/Skeleton/Slime; sem Ogre no normal;
    // pelo menos um tile crate|barrel|spike em salas Normal
}

[Fact]
public void Generate_Boss_SpawnsOgre()
{
    Assert.Contains(boss.Spawns.OfType<EnemySpawn>(), s => s.Enemy == EnemyKind.Ogre);
}
```

Remover `Generate_Normal_TrojansSpawnAwayFromCenter`.

- [ ] **Step 2: Implementar pool/obstáculos; spawn inimigo só tiles floor genéricos; PASS; commit** `feat: RoomGen dungeon medieval`

---

### Task 8: Bolt, branding, itens, render

**Files:**
- Modify: `ChamberCrawl/Models/GameTypes.cs` (remover `BitGlyph` de `ProjectileEntity`)
- Modify: `ChamberCrawl/Systems/Combat.cs` (não setar BitGlyph)
- Modify: `ChamberCrawl/Rendering/EntityRenderer.cs`
- Modify: `ChamberCrawl/Game1.cs`
- Modify: `ChamberCrawl/Rendering/MenuRenderer.cs`
- Modify: `ChamberCrawl/Data/Items.cs`
- Modify: `README.md`
- Modify: `docs/SPRITE_STYLE.md`
- Modify: testes que referenciem BitGlyph / AdwareKing draw se houver

**Interfaces:**
- Produces: projétil do jogador usa textura `"bolt"`
- Produces: `Window.Title = "Dungeon Crawl"`, menu `"DUNGEON CRAWL"`
- Produces: nomes de itens da spec (Lâmina, Botas, Poção, Pergaminho, Tridente, Ímã)
- Produces: Ogre draw size `radius * 3.2f` (sem scale mini/iframe)

- [ ] **Step 1: EntityRenderer**

```csharp
case ProjectileEntity proj:
    if (proj.FromPlayer)
        texture = atlas.Get("bolt");
    else
    {
        texture = atlas.Get("projectile");
        tint = new Color(180, 120, 255);
    }
    return true;
```

Enemy textures só via `Enemies.Defs[enemy.Enemy].Texture` (sem trojan-active).

Ogre:

```csharp
if (entity is EnemyEntity { Enemy: EnemyKind.Ogre } ogre)
    return MathF.Max(ogre.Radius * 3.2f, 40f);
```

- [ ] **Step 2: Branding + Items.Name + README + SPRITE_STYLE temática medieval**

- [ ] **Step 3: `dotnet test` completo verde (ainda pode faltar PNGs novos — atlas precisa dos arquivos; se TextureAtlas carrega todos os PNG da pasta, criar placeholders na Task 9 antes do run manual). Preferência: nesta task só garantir testes unitários que não carregam atlas.**

```bash
dotnet test --nologo
```

- [ ] **Step 4: Commit** `feat: branding Dungeon Crawl e projétil bolt`

---

### Task 9: Assets + Sprite Studio

**Files:**
- Modify: `tools/sprite-studio/server.js` (`SIZE_DEFAULTS`)
- Create/Replace: `ChamberCrawl/Assets/Textures/{enemies,items,player,tiles}/*.png`
- Create/Replace: `ChamberCrawl/Assets/SpriteSources/**/*.json`
- Delete: texturas/fontes órfãs (`folder`, `popup`, `bad-sector`, `enemy-pspam`, `enemy-trojan`, `enemy-trojan-active`, `enemy-adware-king`, `bit-0`, `bit-1`)

**Interfaces:**
- Produces ids finais da spec:

```
player/player, player/bolt
tiles/floor, wall, door, door-tile, crate, barrel, spike, obstacle, portal
enemies/enemy-bat, enemy-skeleton, enemy-slime, enemy-ogre, projectile
items/coin, heart, item-damage, item-speed, item-hp, item-firerate, item-triple, item-greedy
```

- [ ] **Step 1: Atualizar SIZE_DEFAULTS**

```javascript
const SIZE_DEFAULTS = {
  'enemies/enemy-bat': 32,
  'enemies/enemy-skeleton': 32,
  'enemies/enemy-slime': 32,
  'enemies/enemy-ogre': 64,
  'enemies/projectile': 64,
  'items/coin': 64,
  'items/heart': 64,
  'items/item-damage': 64,
  'items/item-firerate': 64,
  'items/item-greedy': 64,
  'items/item-hp': 64,
  'items/item-speed': 64,
  'items/item-triple': 64,
  'player/player': 32,
  'player/bolt': 32,
  'tiles/floor': 32,
  'tiles/wall': 32,
  'tiles/door': 32,
  'tiles/door-tile': 32,
  'tiles/crate': 32,
  'tiles/barrel': 32,
  'tiles/obstacle': 32,
  'tiles/spike': 32,
  'tiles/portal': 64,
};
```

- [ ] **Step 2: Renomear/criar arquivos** — `mv` para ids novos onde fizer sentido; criar Canvas seeds no studio (funções `drawBat`, `drawSkeleton`, `drawSlime`, `drawOgre`, `drawMage`, `drawBolt`, `drawCrate`, `drawBarrel`, `drawSpike`, etc.) ou editar via API save; paleta pedra/madeira/âmbar conforme spec.

- [ ] **Step 3: Remover PNGs/JSONs órfãos**

- [ ] **Step 4: Bake**

```bash
node tools/sprite-studio/server.js --bake
```

Expected: todos os sprites listados baked sem erro.

- [ ] **Step 5: Verificação**

```bash
dotnet test --nologo
dotnet run --project ChamberCrawl
```

Smoke manual: menu DUNGEON CRAWL, mage + bolt, bat/skeleton/slime, obstáculos, ogre 2 fases → vitória.

- [ ] **Step 6: Commit** `feat: sprites medievais Dungeon Crawl`

---

## Self-review (plan vs spec)

| Spec requirement | Task |
|------------------|------|
| Branding Dungeon Crawl | 8 |
| Mage + bolt | 8, 9 |
| Bat chase / Skeleton shoot / Slime multiply | 4, 5, 3 |
| Ogre 2 fases | 6 |
| Crate/barrel/spike | 1, 7, 9 |
| Item names | 8 |
| RoomGen pool + boss | 7 |
| Vitória sem portal | já existe (Progression) — regressão em testes |
| Sprite Studio + bake | 9 |
| Fora de escopo depths/áudio/namespaces | respeitado |

Sem TBD. Nomes de const/AI alinhados entre tasks (`SlimeSplitMs`, `OgrePhase1MaxMs`, `TileCrate`).
