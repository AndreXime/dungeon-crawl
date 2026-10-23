# Dungeon Crawl — Guideline de sprites (Isaac / dungeon clássico)

**Referência positiva:** após Task 2, o `coin` regenerado no kit é a âncora visual (outline `#0d0a08`, luz TL, paleta do kit). Até lá, use `tools/sprite-studio/palette.md` como fonte de hex.

**Anti-exemplo:** sprites antigos com cyan Diskette, bordas moles, milhares de cores, ou ilustração encolhida (ex.: `heart.png` / vários `item-*.png` pré-retheme).

**Paleta:** `tools/sprite-studio/palette.md` (hex e regras de outline/luz).

## Resolução nativa

| Uso | Tamanho | Notas |
|-----|---------|--------|
| Tiles de mapa | **32×32** | 1 tile = 1 célula (`Cell = 32`) |
| Player, bolt, overlays de obstáculo | **32×32** | Silhueta legível em 1 célula |
| Itens, coin, heart (HUD), portal, projétil inimigo, boss | **64×64** | Mais detalhe, mas ainda pixel art nativo |

Nunca desenhar em 512/1024 e “só reduzir”. O pixel tem que nascer no tamanho final.

## Kit visual (todos os sprites)

1. Outline seletivo `#0d0a08` (1px na silhueta).
2. Luz canto superior esquerdo: 1 highlight + 1 mid + 1 shade por material.
3. Fundo transparente (alpha 0) em entidades/itens; tiles `floor`, `wall`, `door-tile` opacos.
4. Bordas duras: sem anti-alias, blur, halo ou gradiente fotográfico.
5. Paleta curta: **≤ 16 cores opacas** no sprite útil (ideal 8–12), só cores do kit.
6. Silhueta Isaac: formas cheias, pouco detalhe miúdo; legível no chão escuro e em thumbnail ~32px.

## O que é “bom” (âncora pós Task 2: coin)

- Pixels na grade, bordas duras
- Contorno seletivo `#0d0a08`
- Paleta do kit (`palette.md`), 1–3 tons por material
- Highlight TL consistente com tiles e personagens
- Fundo transparente sem halo cinza/preto
- Forma reconhecível em thumbnail

## O que é “ruim”

- Cyan / estética Diskette residual
- Anti-alias, blur, halo semi-transparente
- Gradiente fotográfico ou milhares de cores
- Silhueta confusa ou “bloco aleatório”
- Fundo opaco em item/entidade (exceto tiles de mapa)

## Temática

- **Binding of Isaac / dungeon clássico:** outline forte, formas cheias, pedra/madeira/ouro/sangue/slime
- Legível em fundo escuro; magia em roxo (`#c45cff` / `#f0e0ff`), **não cyan**
- Fantasia clássica (mago, morcego, esqueleto, ogro), não realismo

## Prompt / geração (Cursor GenerateImage)

Bloco base (ver também `palette.md`):

```text
native NNxNN pixel art, Binding of Isaac inspired dungeon crawler style, hard pixel edges, no anti-aliasing, no blur, selective black outline (#0d0a08), top-left lighting, limited palette from dungeon kit, transparent background
```

Acrescentar sujeito, tamanho (`32` ou `64`) e cores do kit. Quando existir, anexar `coin.png` regenerado para alinhar HUD/itens/personagens.

Depois da geração:

1. Redimensionar com **nearest-neighbor** se vier maior (`magick … -filter point -resize 64x64!`)
2. Remover fundo (floodfill / fuzz) e garantir canal alpha
3. Conferir lado a lado com `coin` (regenerado) e `floor`

## Inventário alvo (24 ids)

Tudo via **GenerateImage** + quantize (`tools/sprite-studio/quantize-characters.js` / `import-generated.js`), fonte `mode: "pixels"`.

**32×32:** `floor`, `wall`, `door`, `door-tile`, `crate`, `barrel`, `spike`, `obstacle`, `player`, `bolt`, `enemy-bat`, `enemy-skeleton`, `enemy-slime`.

**64×64:** `portal`, `coin`, `heart`, `item-damage`, `item-speed`, `item-hp`, `item-firerate`, `item-triple`, `item-greedy`, `enemy-ogre`, `projectile`.

## Checklist antes de merge

- [ ] Tamanho exato 32 ou 64
- [ ] Fundo transparente (exceto tile de mapa opaco)
- [ ] Sem halo de AA na silhueta
- [ ] Mesma família: outline `#0d0a08`, luz TL, paleta do kit
- [ ] Zero cyan Diskette; cores só do kit ou justificadas
- [ ] Parece da mesma família que o `coin` regenerado (itens/HUD/personagens)
- [ ] Nome do arquivo bate com o atlas (`Get("heart")` etc.)
