# Dungeon Crawl

Roguelike top-down em C# com MonoGame (DesktopGL). Você controla um **mago** que dispara **bolts** mágicos por salas de dungeon medieval.

## Objetivo

Sobreviva a **5 andares** gerados proceduralmente, limpe salas, colete moedas, compre itens e derrote o boss de cada bioma. A vitória da run é no andar 5.

| Andar | Bioma | Destaques |
|------:|-------|-----------|
| 1 | Masmorra | Morcego, Esqueleto, Slime · boss **Ogre** |
| 2 | Catacumbas | Espectro · boss **Necromante** · spikes ósseos |
| 3 | Esgotos | Rato · boss **Rei Lodo** · poças lentas |
| 4 | Arsenal | Guarda · boss **Golem de Ferro** · barricadas/bigornas |
| 5 | Santuário | Acólito · boss **Senhor das Runas** · runas/pedestais |

Loja e salas de item ficam mais generosas em raridade conforme o depth. Inimigos ganham HP/dano por andar.

## Requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) 9 (ou 8+ com roll-forward)
- Plataforma MonoGame DesktopGL (incluída no projeto)

## Como rodar

```bash
dotnet restore
dotnet run --project ChamberCrawl
```

## Testes e build

```bash
dotnet test
dotnet build
```

## Menu

No menu inicial: **Começar**, **Configurações** (tela cheia e escala 1x–4x) e **Sair**.
Preferências ficam em `settings.json` ao lado do executável.
Em tela cheia a escala fica em **AJUSTE**: preenche o monitor o máximo possível sem cortar o jogo.

| Tecla (menu) | Ação |
|--------------|------|
| W / S ou ↑ / ↓ | Navegar |
| Enter / Espaço | Confirmar |
| ← / → | Alternar tela cheia ou ciclo de escala (só no modo janela) |
| Esc | Voltar (configurações). No menu principal não fecha o jogo |

## Controles (durante a run)

| Tecla | Ação |
|-------|------|
| WASD | Mover |
| Setas | Atirar |
| R | Reiniciar a run (após morrer ou vencer) |
| Esc | Pause (Continuar / Menu). Esc de novo continua |
