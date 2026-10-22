# Item HUD prompt

## Objetivo

Quando houver item(s) de upgrade no chão da sala atual, o HUD mostra o nome e o efeito do item mais próximo do jogador. Um prompt por vez.

## Escopo

- Todos os `PickupKind.Item` (sala de item e loja).
- Sem mudança de coleta, compra ou stats.
- Sem prompt para moedas ou portal.

## Comportamento

1. A cada frame de desenho do HUD, listar pickups `Item` em `state.Entities`.
2. Se a lista estiver vazia, não desenhar o prompt.
3. Se houver um ou mais, escolher o de menor distância ao `PlayerEntity` (centro a centro). Empate: ordem estável na lista de entidades.
4. Resolver `Items.Defs[itemId]` para nome e efeito.

## Layout

- Posição: abaixo da linha superior do HUD (corações / moedas / andar), alinhado à esquerda com o mesmo `HudPad`.
- Fonte: `GlyphFont` (já usada no menu). Escala 1 no prompt para caber.
- Linha 1: nome do item (ex.: `DENTE AFIADO`).
- Linha 2: efeito curto (ex.: `DANO +1`).
- Loja (`Price > 0`): anexar na linha 2 com hífen, ex.: `DANO +1 - 5`.
- Caracteres: só o que `GlyphFont` desenha (A-Z, 0-9, espaço, `-`, `:`, `?`). Sem `+`, `.`, `|`, `/`. Usar `MAIS` no lugar de `+` e `1-5` no lugar de `1.5`.

## Textos de efeito

| Item id       | Nome           | Efeito HUD           |
|---------------|----------------|----------------------|
| damage_up     | Dente Afiado   | DANO MAIS 1          |
| speed_up      | Botas          | VELOCIDADE MAIS 30   |
| hp_up         | Coração Extra  | CORACAO MAIS 1       |
| fire_rate     | Cafeína        | TIRO MAIS 1-5        |
| triple        | Tridente       | TIRO TRIPLO          |
| greedy        | Ímã            | MULT MOEDAS MAIS 1   |

Gerar a string a partir dos campos de `ItemDef` (não hardcodar por id), com fallback `UPGRADE` se o def for desconhecido. Floats: uma casa decimal com hífen (`1-5` para 1.5).

## Implementação

- Lógica de escolha + formatação em helper estático (ex. `ItemPrompt` em `Systems/` ou método privado em `HudRenderer`).
- Desenho em `HudRenderer.DrawHud`.
- Teste unitário do seletor “mais próximo” e da formatação de efeito (sem GPU).

## Fora de escopo

- Prompt flutuando sobre o pedestal.
- Tooltip ao aproximar com raio mínimo (sempre o mais perto da sala).
- Ícone do item no prompt.
