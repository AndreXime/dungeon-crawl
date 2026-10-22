# Diskette

Roguelike top-down em C# com MonoGame (DesktopGL). Você controla **Diskette**, um disquete 3,5" que dispara Bits pelos setores corrompidos do PC.

## Objetivo do MVP

Sobreviva em salas geradas proceduralmente na **Partition C**, derrote inimigos (Pspam, Trojan), colete moedas, compre itens na loja e vença o **Adware King** para completar a run.

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

No menu inicial: **Começar**, **Configurações** (tela cheia Sim/Não) e **Sair**.
Preferência de tela cheia fica em `settings.json` ao lado do executável.

| Tecla (menu) | Ação |
|--------------|------|
| W / S ou ↑ / ↓ | Navegar |
| Enter / Espaço | Confirmar |
| ← / → | Alternar tela cheia |
| Esc | Voltar (configurações). No menu principal não fecha o jogo |

## Controles (durante a run)

| Tecla | Ação |
|-------|------|
| WASD | Mover |
| Setas | Atirar |
| R | Reiniciar a run (após morrer ou vencer) |
| Esc | Pause (Continuar / Menu). Esc de novo continua |
