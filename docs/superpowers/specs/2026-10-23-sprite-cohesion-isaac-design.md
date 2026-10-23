# Design: Coesão visual dos sprites (estilo Isaac / dungeon clássico)

Data: 2026-10-23  
Status: aprovado em brainstorming  
Projeto: `ChamberCrawl` / Dungeon Crawl  
Pipeline: Sprite Studio (`tools/sprite-studio`) + GenerateImage para personagens

## Objetivo

Substituir o lote atual de sprites (inconsistente em paleta, outline e silhueta) por um **set único** legível no estilo Binding of Isaac / dungeon clássico, em que tiles, itens e personagens claramente pertencem à mesma família visual.

## Decisões

| Tema | Escolha |
|------|---------|
| Estilo | Isaac / dungeon clássico (outline forte, formas cheias, luz TL) |
| Escopo | Regenerar **todo** o inventário de texturas em uso (24 ids) |
| Produção | Híbrido: tiles + itens/HUD em Canvas compartilhado; personagens via GenerateImage alinhados à mesma paleta |
| Abordagem | Paleta global + lote único (não misturar estilos por pasta) |
| Âncora de qualidade | Thumbnail legível a 32px; mesma outline/luz/paleta; zero cyan Diskette |

## Kit visual (obrigatório em todos os sprites)

1. **Outline** preto/quase-preto (`#0d0a08`) 1px seletivo na silhueta.
2. **Luz** canto superior esquerdo (1 highlight + 1 mid + 1 shade por material).
3. **Fundo** transparente (alpha 0) em entidades/itens. Tiles de mapa (`floor`, `wall`, `door-tile`) opacos.
4. **Bordas** duras: sem anti-alias, sem halo, sem gradiente fotográfico.
5. **Paleta curta**: ≤ 16 cores opacas no sprite útil (ideal 8–12), tiradas do kit abaixo.
6. **Silhueta Isaac**: formas cheias e legíveis, pouco detalhe miúdo; priorizar leitura no chão escuro.

## Paleta compartilhada

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

Qualquer cor fora desta tabela precisa de justificativa no review visual.

## Inventário (24 ids)

### Canvas compartilhado (tiles + itens)

**Tiles 32×32:** `floor`, `wall`, `door`, `door-tile`, `crate`, `barrel`, `spike`, `obstacle`, `portal` (portal pode ser 64×64 se já for o padrão do atlas).

**Itens/HUD 64×64:** `coin`, `heart`, `item-damage`, `item-speed`, `item-hp`, `item-firerate`, `item-triple`, `item-greedy`.

Helpers Canvas obrigatórios (mesmo módulo/arquivo de seed):

- `px(x,y,w,h,c)` fillRect inteiro
- `outlineRect` / stroke 1px com `#0d0a08`
- constantes nomeadas da paleta (não hex soltos espalhados)

### GenerateImage → limpeza → SpriteSources (personagens)

**32×32:** `player`, `bolt`, `enemy-bat`, `enemy-skeleton`, `enemy-slime`  
**64×64:** `enemy-ogre`, `projectile` (inimigo)

Fluxo por sprite:

1. Prompt fixo: `native NNxNN pixel art, Binding of Isaac inspired dungeon crawler style, hard pixel edges, no anti-aliasing, no blur, selective black outline, top-left lighting, limited palette, transparent background` + sujeito + cores do kit.
2. Se vier maior: nearest-neighbor para NN×NN.
3. Remover fundo sólido; garantir alpha limpo.
4. Salvar PNG em `Textures/` e espelhar fonte no Sprite Studio (`pixels` ou canvas amostrado) para edição futura.
5. Conferir lado a lado com `coin` (após coin regenerado no kit) e `floor`.

## Fora de escopo

- Novos ids de textura / animações multi-frame.
- Mudança de mecânica ou AI.
- Áudio.
- Redesign de UI além dos sprites de item/HUD.

## Critérios de sucesso

1. Nenhum sprite do inventário com cyan Diskette ou silhueta “bloco aleatório” ilegível.
2. `heart` é claramente um coração; `crate` é caixa de madeira (não baú genérico confuso); inimigos reconhecíveis (bat / skeleton / slime / ogre).
3. Comparação visual em grade: mesma família de outline e luz.
4. `node tools/sprite-studio/server.js --bake` 24/24 ok.
5. `dotnet test` verde (sem mudança de código esperada).

## Referência

- Guideline: `docs/SPRITE_STYLE.md` (atualizar na implementação para este kit)
- Spec retheme: `docs/superpowers/specs/2026-10-23-dungeon-crawl-retheme-design.md`
