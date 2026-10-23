# Dungeon Crawl — Guideline de sprites

Referência positiva: `Assets/Textures/items/coin.png`  
Anti-exemplo: `heart.png` / vários `item-*.png` que parecem ilustração em alta resoluçao encolhida (bordas moles, milhares de cores, anti-alias).

## Resolução nativa

| Uso | Tamanho | Notas |
|-----|---------|--------|
| Tiles de mapa | **32×32** | 1 tile = 1 célula (`Cell = 32`) |
| Player, bolt, overlays de obstáculo | **32×32** | Silhueta legível em 1 célula |
| Itens, coin, heart (HUD), portal, projétil inimigo, boss | **64×64** | Mais detalhe, mas ainda pixel art nativo |

Nunca desenhar em 512/1024 e “só reduzir”. O pixel tem que nascer no tamanho final.

## O que é “bom” (coin)

- Pixels alinhados à grade, bordas duras
- Contorno seletivo escuro (marrom/quase-preto), não necessariamente preto puro
- Paleta curta e intencional (no alvo: **≤ 32 cores opacas** no sprite útil; ideal 8–16)
- 1–3 tons por material (base, sombra, highlight)
- Highlight direcional simples (em geral canto superior esquerdo)
- Fundo **transparente** (alpha 0), sem halo cinza/preto nas bordas
- Forma lida em thumbnail: dá para dizer o que é a 32 px de distância

## O que é “ruim” (heart / downscale)

- Anti-alias / blur / halo semi-transparente na borda
- Gradiente “fotográfico” com centenas/milhares de cores
- Ruído de compressão ou de reamostragem bilinear
- Silhueta confusa (ex.: coração que vira “dente”)
- Fundo preto opaco colado no sprite (só aceitável se for tile de chão/parede)

## Temática Dungeon Crawl

- Estética medieval dungeon: pedra, madeira, tochas, âmbar
- Cores legíveis em fundo escuro (ouro, vermelho poção, verde slime, cinza pedra)
- Silhuetas de fantasia clássica (mago, morcego, esqueleto, ogro), não realismo

## Prompt / geração (Cursor GenerateImage)

Incluir sempre no prompt:

1. `native NNxNN pixel art` (32 ou 64)
2. `hard pixel edges, no anti-aliasing, no blur, no soft gradients`
3. `limited palette, selective dark outline`
4. `transparent background` (se a ferramenta entregar fundo sólido, limpar alpha depois)
5. Referência visual: anexar `coin.png` quando o estilo tiver que bater com HUD/itens
6. Assunto + papel no jogo em uma frase

Depois da geração:

1. Redimensionar com **nearest-neighbor** se vier maior (`magick … -filter point -resize 64x64!`)
2. Remover fundo (floodfill / fuzz) e garantir canal alpha
3. Conferir com `identify` e inspeção visual ao lado do `coin.png`

## Inventário alvo (nomes atuais)

- `items/coin.png` — referência
- `items/heart.png` — HUD HP
- `items/item-*.png` — loot medieval (lâmina, botas, poção, etc.)
- `player/player.png`, `player/bolt.png` — mago e projétil 32×32
- Tiles dungeon — `floor`, `wall`, `door`, `crate`, `barrel`, `spike`
- `enemies/enemy-bat`, `enemy-skeleton`, `enemy-slime`, `enemy-ogre` — pool do depth 1

## Checklist antes de merge

- [ ] Tamanho exato 32 ou 64
- [ ] Fundo transparente (exceto tile de mapa opaco)
- [ ] Sem halo de AA na silhueta
- [ ] Parece da mesma família que `coin.png` (itens/HUD)
- [ ] Nome do arquivo bate com o atlas (`Get("heart")` etc.)
