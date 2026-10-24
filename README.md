# Dungeon Crawl

Roguelike top-down em C# com MonoGame (DesktopGL). Você controla um **mago** que dispara **bolts** mágicos por salas de dungeon medieval.

## Objetivo do MVP

Sobreviva em salas geradas proceduralmente no **dungeon** (até 5 andares), derrote inimigos (Morcego, Esqueleto, Slime), colete moedas, compre itens na loja e avance pelos bosses. No andar 1 o **Ogre** solta um portal; a vitória final é no andar 5.

## Requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) 9 (ou 8+ com roll-forward)
- Plataforma MonoGame DesktopGL (incluída no projeto)

### Instalar .NET e PATH (Linux)

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$HOME/.dotnet
```

Confirme com `dotnet --version`.

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
