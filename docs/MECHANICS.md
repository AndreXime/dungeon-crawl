# Mecânicas

Como a run funciona. Biomas e pools por andar: [FLOORS.md](FLOORS.md).

Sala fixa **15×11** tiles (**32 px**). Até **5** andares.

## Loop da run

1. **Menu** → Começar (seed nova), Configurações ou Sair.
2. Andares 1–5: limpe salas, pegue itens, derrote o boss.
3. Boss limpo com depth &lt; 5 → **portal** no centro (ativo após ~0,8 s). Tocar avança o andar (mantém HP, itens, moedas).
4. Boss do andar **5** limpo → **vitória** (sem portal).
5. HP ≤ 0 → **derrota**. **R** reinicia a run (morto ou vencedor).

**Esc** em partida abre pause (Continuar / Menu).

## Mapa do andar

- **6–8 salas** geradas a partir da Start, conexões cardinais.
- Tipos: **Start**, **Normal**, **Shop**, **Item**, **Boss**.
- Shop, Item e Boss são **folhas** (uma porta). Boss fica longe da origem.
- Minimapa: salas visitadas e vizinhas desconhecidas (`?`).
- Moedas no chão persistem se você sair e voltar na sala.

## Limpar sala e portas

- Sala **cleared** quando não resta inimigo vivo.
- Portas **trancam por combate**, não por chave: com inimigos vivos a porta é sólida; ao limpar, destranca.
- Porta aberta: atravessar leva à sala vizinha (entrada no lado oposto).
- Start já começa cleared. Salas cleared não respawnam inimigos; itens já pegos não voltam.

## Jogador

| Stat inicial | Valor |
|--------------|------:|
| HP / Max | 6 / 6 (1 coração no HUD = 2 HP) |
| Speed | 110 |
| Damage | 1 |
| Fire rate | 2/s |
| Range | 200 |
| Triple shot | off |
| Coin multiplier | 1 |

- **WASD** move, **setas** atiram (bolt mágico, velocidade 300).
- **Triple shot:** três projéteis com spread ±15°.
- **I-frames:** 600 ms após dano (contato, projétil inimigo ou runa).
- Projéteis do jogador param em sólidos; **crate** destruída vira chão.

## Combate inimigo

- Contato ou projétil aplica o dano do tipo (escalado pelo andar).
- **Espectro:** não esfrega dano contínuo; hit no blink.
- **Runa** (chão): 1 de dano por contato (respeita i-frames).
- Ao morrer: **50%** de dropar moeda (× `CoinMultiplier`).
- HP inimigo sobe ~15% por andar além do 1; dano base +1 a partir do andar 3.

Comportamentos por tipo e bosses: ver [FLOORS.md](FLOORS.md). Em resumo: chase (morcego/rato/guarda), distância + tiro (esqueleto/acólito), divide (slime / rei lodo), blink (espectro), fases de carga (ogro / golem), summon (necromante / senhor das runas).

## Obstáculos e hazards

| Tile | Efeito |
|------|--------|
| Parede, spike, barrel, barricade, anvil, pedestal | Sólidos |
| Crate | Sólido; destrutível pelo bolt |
| Poça (esgotos) | Andável; velocidade × 0,5 (slimes imunes) |
| Runa (santuário) | Andável; dano 1 |

Andares 1–3 usam crate/barrel/spike (no 2 o spike é `spike-bone`). Arsenal: barricade/anvil/spike. Santuário: pedestal + runas.

## Itens e loja

- **Item (sala):** um upgrade grátis no centro. Toque coleta.
- **Shop:** 2–3 itens pagos; precisa de moedas suficientes.
- Raridade **Common / Rare / Legendary**. Andares fundos pesam mais Rare/Legendary; preço da loja sobe um pouco com o depth.
- Efeitos acumulam (dano, speed, HP máx, cadência, range, triple, sharpness, haste, vigor, greedy, fortune, etc.).

O HUD mostra nome e efeito do item de upgrade mais próximo no chão.

## Configurações

Tela cheia e escala 1×–4× (só em janela). Gravadas em `settings.json` ao lado do executável.
