# Paleta compartilhada — kit Isaac / dungeon clássico

Referência para humanos e agentes ao desenhar no Sprite Studio (Canvas) ou gerar personagens (GenerateImage). Hex copiados da spec `docs/superpowers/specs/2026-10-23-sprite-cohesion-isaac-design.md`.

## Kit visual (obrigatório)

1. **Outline** preto/quase-preto (`#0d0a08`) 1px seletivo na silhueta.
2. **Luz** canto superior esquerdo (1 highlight + 1 mid + 1 shade por material).
3. **Fundo** transparente (alpha 0) em entidades/itens. Tiles de mapa (`floor`, `wall`, `door-tile`) opacos.
4. **Bordas** duras: sem anti-alias, sem halo, sem gradiente fotográfico.
5. **Paleta curta**: ≤ 16 cores opacas no sprite útil (ideal 8–12), tiradas do kit abaixo.
6. **Silhueta Isaac**: formas cheias e legíveis, pouco detalhe miúdo; priorizar leitura no chão escuro.

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
| Mago robe | `#4a2060` / `#6b3a8a` | player (sem cyan) |
| Pele | `#e8c8a0` | faces |
| Musgo | `#3d5a3d` | floor accents |
| Magia | `#c45cff` / `#f0e0ff` | bolt / gem (roxo, não cyan) |

## Canvas (tiles + itens)

Use constantes nomeadas desta paleta no módulo de seed (não hex soltos espalhados):

- `px(x,y,w,h,c)` — fillRect inteiro
- `outlineRect` / stroke 1px com `#0d0a08`

## Prompt base (GenerateImage)

Copiar e colar; substituir `NN` por `32` ou `64` e acrescentar sujeito + cores do kit:

```text
native NNxNN pixel art, Binding of Isaac inspired dungeon crawler style, hard pixel edges, no anti-aliasing, no blur, selective black outline (#0d0a08), top-left lighting, limited palette from dungeon kit, transparent background
```

Após gerar: nearest-neighbor para NN×NN, limpar fundo sólido, conferir lado a lado com `coin` (após regeneração no kit) e `floor`.
