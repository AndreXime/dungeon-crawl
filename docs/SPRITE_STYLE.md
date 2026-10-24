# Sprites — estilo e paleta (Isaac / dungeon clássico)

**Âncora visual:** `coin` e tiles do kit. **Pipeline:** Cursor `GenerateImage` → `node tools/sprite-studio/import-generated.js --only <id>` → PNG em `Assets/Textures/` + JSON em `Assets/SpriteSources/` (`mode: "pixels"`).

## Resolução nativa

| Uso | Tamanho | Notas |
|-----|---------|--------|
| Tiles de mapa e overlays de obstáculo | **32×32** | 1 tile = 1 célula (`Cell = 32`) |
| Player, bolt, inimigos comuns | **32×32** | Silhueta legível em 1 célula |
| Itens, coin, heart, portal, bosses, projétil | **64×64** | Mais detalhe, ainda pixel art nativo |

Nunca desenhar em 512/1024 e “só reduzir”. O pixel nasce no tamanho final.

## Kit visual

1. Outline seletivo `#0d0a08` (1px na silhueta).
2. Luz canto superior esquerdo: 1 highlight + 1 mid + 1 shade por material.
3. Fundo transparente (alpha 0) em entidades/itens; `floor-*` e `wall-*` opacos.
4. Portas (`door-1`…`door-5`, `door-open`): overlay com alpha; **flush na base**.
5. Bordas duras: sem anti-alias, blur, halo ou gradiente fotográfico.
6. Paleta curta: **≤ 16 cores opacas** (ideal 8–12), só cores da tabela abaixo.
7. Silhueta Isaac: formas cheias, pouco miúdo; legível no chão escuro.

Qualquer cor fora da tabela precisa de justificativa no review visual.

## Tabela de cores

| Papel | Hex | Uso |
|-------|-----|-----|
| Outline | `#0d0a08` | Contorno |
| Pedra escura | `#2a2620` | wall, sombra |
| Pedra mid | `#4a443c` | floor/wall |
| Pedra clara | `#6a6358` | highlight pedra |
| Madeira escura | `#3d2814` | crate/barrel/staff |
| Madeira mid | `#8b5a2b` | madeira |
| Madeira clara | `#c4a06a` | highlight madeira |
| Ouro | `#d4a017` / `#f0d060` | coin, metal |
| Sangue | `#8b1a1a` / `#d43030` | heart, olhos hostis |
| Slime | `#2d5a2d` / `#4a9a3a` | slime |
| Osso | `#e8dcc0` / `#c4b898` | skeleton |
| Mago robe | `#4a2060` / `#6b3a8a` | player |
| Pele | `#e8c8a0` | faces |
| Musgo | `#3d5a3d` | floor accents |
| Magia | `#c45cff` / `#f0e0ff` | bolt / gem (roxo, **não** cyan) |

## Temática

Pedra, madeira, ouro, sangue, slime, osso, magia roxa. Fantasia clássica, não realismo.

## Prompt base (GenerateImage)

```text
native NNxNN pixel art, Binding of Isaac inspired dungeon crawler style, hard pixel edges, no anti-aliasing, no blur, selective black outline (#0d0a08), top-left lighting, limited palette from dungeon kit, transparent background
```

Acrescentar sujeito, `32` ou `64`, e cores do kit. Depois: `import-generated.js` (resize point, chroma-key, quantize, outline).

## Inventário (IDs do atlas)

**Tiles 32×32**

| ID | Notas |
|----|--------|
| `floor-1`…`floor-5`, `wall-1`…`wall-5` | Opacos, um par por andar |
| `door-1`…`door-5` | Overlay fechada (tema do andar), flush bottom |
| `door-open` | Overlay aberta sobre o wall |
| `crate`, `barrel`, `spike`, `spike-bone` | Obstáculos / armadilha |
| `barricade`, `anvil` | Arsenal (depth 4) |
| `obstacle`, `puddle`, `rune`, `pedestal` | Genérico / esgotos / santuário |
| `portal` | 64×64 |

**Entidades**

| ID | Tamanho |
|----|---------|
| `player`, `bolt` | 32 |
| `enemy-bat`, `enemy-skeleton`, `enemy-slime`, `enemy-specter`, `enemy-rat`, `enemy-guard`, `enemy-acolyte` | 32 |
| `enemy-ogre`, `enemy-necromancer`, `enemy-slime-king`, `enemy-iron-golem`, `enemy-rune-lord` | 64 |
| `projectile` | 64 |

**HUD / itens 64×64:** `coin`, `heart`, `item-damage`, `item-speed`, `item-hp`, `item-firerate`, `item-triple`, `item-greedy`, `item-range`, `item-sharp`, `item-haste`, `item-vigor`, `item-fortune`.

Porta aberta = `wall-*` + `door-open` (não usar `door-tile`).

## Checklist

- [ ] Tamanho exato 32 ou 64
- [ ] Fundo transparente (exceto floor/wall opacos)
- [ ] Sem halo de AA
- [ ] Outline `#0d0a08`, luz TL, só cores do kit
- [ ] Portas/overlays flush na base
- [ ] Nome do arquivo = ID do atlas
