# Design: Diskette (retema e expansão temática)

Data: 2026-10-22  
Status: aprovado em brainstorming (seções 1–4)  
Projeto técnico: `ChamberCrawl` (MonoGame DesktopGL)  
Nome de exibição: **Diskette**

## Objetivo

Transformar o MVP genérico estilo Isaac em um roguelike com temática de PC corrompido dos anos 90/00. O jogador controla **Diskette**, um disquete 3,5" azul-desbotado que dispara **Bits** (`0`/`1`).

A visão cobre **4 andares** e o elenco completo de inimigos/bosses. A **fatia 1** entrega só a **Partition C** jogável de ponta a ponta.

## Decisões de produto

| Tema | Escolha |
|------|---------|
| Escopo de entrega | Spec completa (roadmap) + implementação em fatias |
| Fatia 1 | Só Partition C; vitória no Adware King |
| Arte | Gerada no Cursor (`GenerateImage`) + PNGs em `Assets/Textures/` |
| Mecânica fatia 1 | Visual + Pspam, Trojan, Adware King reais |
| Andares na fatia 1 | 1 andar jogável (sem portal) |
| Obstáculos Partition C | Pastas destruíveis; bad sector bloqueia tiros; pop-ups sólidos |
| Nome | UI/README/janela = Diskette; pasta/namespaces = `ChamberCrawl` |
| Abordagem técnica | Temática jogável na Partition C (não só skin, não framework completo) |

## Visão completa (roadmap)

### Personagem

- **Diskette:** disquete 3,5" azul-desbotado, olhos pixelados no rótulo rasgado, duas pernas curtas.
- **Projétil padrão (Bits):** quadrados amarelos que alternam visualmente entre `0` e `1`. Saem do topo do sprite, trajetória reta, velocidade média. Som alvo (roadmap): bipe mecânico retrô. **Fatia 1:** sem áudio (silêncio); reservar ponto de extensão no spawn do tiro.

### Andares

| Depth | Andar | Chefe | Passagem entre andares |
|------:|-------|-------|------------------------|
| 1 | Partition C (Setores Danificados) | Adware King | Ícone Lixeira (animação Esvaziar) |
| 2 | RAM (Memória Sobrecarregada) | Overclock Overlord | Slot PCI + fita de dados |
| 3 | Temp / Lixeira | Ransomware.exe | `FORMAT C:` no terminal |
| 4 | Kernel (Núcleo em Chamas) | Blue Screen | Kernel Panic (prompt) |

#### 1. Partition C

- Visual: SO final dos anos 90, pastéis esverdeados, pastas amareladas.
- Paredes: chrome Win95/98, title bar azul no topo das salas.
- Obstáculos: pastas empilhadas (destruíveis), pop-ups de erro estáticos, bad sectors (pretos com `?`, bloqueiam tiros).
- Portas: janelas de pasta com barra amarela durante combate; abrem ao limpar.
- Passagem: Lixeira (roadmap; não usada na fatia 1).

#### 2. RAM

- Visual: neon roxo + verde-limão, PCB piscando, dados nas trilhas.
- Paredes: PCB verde, barramentos de cobre, pinos dourados.
- Obstáculos: capacitores (explosão em área), pentes RAM inquebráveis, poças de silício (lentidão).
- Portas: gates AND/OR (lasers) que apagam ao limpar.
- Passagem: Slot PCI.

#### 3. Temp / Lixeira

- Visual: marrom, cinza-escuro, scanlines.
- Paredes: `.tmp` corrompidos, atalhos quebrados, `.bmp` distorcidos.
- Obstáculos: código truncado (bloqueia visão), zips acorrentados (explosão libera itens), vazamento de dados (dano).
- Portas: pasta “Arquivo Protegido” com X vermelho.
- Passagem: `FORMAT C:`.

#### 4. Kernel

- Visual: vermelho + preto, chuva de código estilo Matrix avermelhada.
- Paredes: microprocessadores, fios com faíscas.
- Obstáculos: dissipadores (espinhos), núcleos cuspindo fogo, barramentos energizados.
- Portas: Firewall de chamas digitais.
- Passagem: Kernel Panic → sala do boss final.

### Inimigos (visão)

| Id | Conceito | Comportamento |
|----|----------|---------------|
| Pspam | Janela de propaganda | Flutua errática; a cada 4 s divide em 2 menores se não morta |
| Worm.exe | Verme de código verde | Atravessa paredes; rastro infectado no chão (dano) |
| Trojan | Presente/baú falso | Idle até proximidade; revela dentes e investe |
| Keylogger | Olho + teclado | Espelha cada tiro do jogador na mesma direção |
| Spyware | Aranha de cursores | No “teto”; teias rootam o jogador por 2 s |

### Bosses (visão)

**Adware King (Partition C)**  
Chuva de Botões (rajada 360°), Pop-up Spam (3 Pspams), Redimensionar (mini + i-frames 3 s + onda ao crescer).

**Ransomware.exe (Temp)**  
Laser de encriptação giratório, Bloqueio de Carteira (10 s sem bombas/itens ativos), Pedágio (atirar moedas = 15% HP sem dano ao jogador).

**Overclock Overlord (RAM)**  
Tempestade de vento (puxa/empurra), Superaquecimento (+50% speed/ataque + rastro de fogo), Pino Ejetor (pás bumerangue).

**Blue Screen (Kernel)**  
Fase 1: espirais hex. Fase 2: inverte controles 5 s. Fase 3: glitch + duas metades + invocações dos bosses anteriores com pouca vida.

## Fatia 1 — escopo implementável

### Inclui

- Branding Diskette (título da janela, menu, README).
- Run com **um** andar: Partition C (`Floor.Depth == 1` apenas).
- Vitória: sala do boss limpa → `RunStatus.Won` (**sem** portal / sem `AdvanceFloor`).
- Tema visual Partition C (tiles + sprites gerados).
- Diskette + Bits (alternância `0`/`1`).
- Tiles: `TileFolder` (destruível), `TilePopup` (sólido), `TileBadSector` (sólido + bloqueia projéteis).
- Portas: sprite com barra amarela quando trancadas (sem animação elaborada).
- Inimigos: **Pspam**, **Trojan**.
- Boss: **Adware King** com os 3 padrões.
- Testes xUnit cobrindo geração, obstáculos, split, ativação do Trojan, vitória sem portal.
- Manter: WASD/setas, moedas, loja, 6 itens atuais, pause/menu, tamanho de sala, ~6–8 salas.

### Fora da fatia 1

- Depths 2–4 e passagens animadas.
- Worm.exe, Keylogger, Spyware.
- Ransomware, Overclock Overlord, Blue Screen.
- Hazards de lentidão/dano/visão/root.
- Capacitores, zips, firewalls, etc.
- Rename de projeto/namespaces.
- Áudio polido; retema completo dos 6 itens.
- Bombas / itens ativos (relevantes ao Ransomware depois).

## Comportamento numérico (baseline fatia 1)

Valores iniciais, ajustáveis em playtest.

### Pspam

- HP 2 (filho: 1), speed ~50, raio ~9 (filho ~70% do pai).
- Sem tiro. Contato: 1 dano.
- A cada **4 s** com HP > 0: remove-se e spawna **2** filhos.
- Filhos **não** splitam (no máximo uma geração).
- Contato e limpeza de sala: filhos contam como inimigos vivos.

### Trojan

- HP 3, dano contato 1.
- Spawn longe do centro / canto.
- Idle (sprite baú/presente) até jogador a **~80 px**.
- Ao ativar: sprite “dentes”, investida ~140 na direção do jogador; depois chase ~70.
- Durante idle: dano de contato **aplica** (encostar no “baú” punido).

### Adware King

- HP ~40, raio ~22–26, dano contato 2.
- Loop: idle curto (~0,8 s) → padrão → próximo.
1. **Chuva de Botões:** ~12 projéteis em círculo (cast ~1,2 s).
2. **Pop-up Spam:** 3 Pspams pequenos com `CanSplit = false` em tiles livres.
3. **Redimensionar:** 3 s mini (escala no draw, sem sprite extra) + i-frames, move para canto; ao crescer, **onda de choque**: dano de contato em raio curto (~48 px) uma vez no impacto (sem rajada extra de projéteis).

### Obstáculos

| Tile | Sólido corpo | Projétil jogador | Projétil inimigo | Destruível |
|------|--------------|------------------|------------------|------------|
| Folder | Sim | Para e vira floor | Para, pasta permanece | Só tiro do jogador |
| Popup | Sim | Para | Para | Não |
| BadSector | Sim | Para | Para | Não |

`TileObstacle` genérico deixa de ser o default da Partition C (constante pode permanecer para compatibilidade de testes antigos até atualização).

### Progressão

- `FloorGen.Generate(seed, depth: 1)` apenas na run da fatia 1.
- `Progression.Update`: se boss room limpa → `Won`. Não spawna portal.

## Arquitetura (mudanças)

Camadas atuais preservadas. Acrescentos:

| Área | Mudança |
|------|---------|
| `Models` | `EnemyKind` com Pspam, Trojan, AdwareKing; novos `Tile*`; `CanSplit` em `EnemyEntity` (Pspam filho / spawn do boss) |
| `Data` | `Enemies.Defs` atualizado; `FloorThemes` lookup simples (depth 1 → ids de textura Partition C) |
| `World/RoomGen` | Pool Partition C; mistura folder/popup/badsector; boss = AdwareKing |
| `Systems/Ai` | UpdatePspam, UpdateTrojan, UpdateAdwareKing |
| `Systems/Combat` | Destruir folder; bad sector / popup param projéteis |
| `Systems/Collision` | Tratar novos tiles como sólidos |
| `Systems/Progression` | Vitória no depth 1 sem portal |
| `Rendering` | Sprites novos; Bits alternando; tema wall/floor/door |
| `Assets/Textures` | PNGs gerados (lista abaixo) |

Estado extra mínimo nos inimigos: reutilizar `Phase` / `Cooldown` (Trojan: phase 0 idle / 1 active; Pspam: timer de split no cooldown; Boss: índice de padrão + timers).

Evitar framework genérico de “todos os themes” além de um lookup simples de texturas por depth (depth 1 só).

## Arte (fatia 1)

Gerar com Cursor, pixel art ~32×32 (boss maior), fundo transparente, paleta Partition C:

- `player` (Diskette)
- `bit-0`, `bit-1` (tiro do jogador alterna a cada spawn ou por tempo de vida)
- `floor`, `wall`, `door` (trancada com barra amarela)
- `folder`, `popup`, `bad-sector`
- `enemy-pspam`, `enemy-trojan`, `enemy-trojan-active`
- `enemy-adware-king`
- Projétil inimigo: reutilizar textura genérica com tint (botões do boss = mesmo projétil tintado)

Arquivos temáticos **substituem** os PNGs usados pela Partition C (`player`, `floor`, `wall`, `door`, etc.). Sprites de inimigos legados não usados no pool podem ficar no disco até limpeza.

## Testes

- `RoomGen` Partition C: só Pspam/Trojan no pool normal; boss = AdwareKing; tiles folder/popup/badsector presentes em salas normais.
- Projétil do jogador em folder → tile vira floor.
- Projétil em bad sector → remove projétil, tile permanece.
- Pspam: após 4 s spawna 2 filhos; filhos não disparam novo split.
- Trojan: não investe antes do raio; investe depois.
- Boss room limpa → `Won` sem entidade portal.
- Regressão: loja, itens claimed, i-frames, portas trancadas.

## Critérios de sucesso (fatia 1)

1. Título e copy visíveis dizem Diskette.
2. Run completa só na Partition C até Adware King → vitória.
3. Temática legível (Win95 / pastas / Bits).
4. Pspam, Trojan e Adware King jogáveis conforme baseline.
5. Obstáculos B corretos.
6. `dotnet test` verde; `dotnet run` jogável no Linux.

## Fatias futuras (ordem sugerida)

1. **RAM:** theme + obstáculos RAM + Worm.exe + Overclock Overlord + passagem Lixeira→PCI.
2. **Temp:** theme + Keylogger/Spyware parciais + Ransomware (+ pedágio com moedas).
3. **Kernel:** theme + Blue Screen fasado + Kernel Panic.
4. Polimento: áudio, itens temáticos, animações de porta/passagem, balanceamento.

## Referência

- Spec MonoGame base: `docs/superpowers/specs/2026-10-21-chamber-crawl-monogame-design.md`
- Brief do usuário (temática Diskette): conversa 2026-10-22
