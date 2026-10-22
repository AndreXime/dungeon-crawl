# Chamber Crawl

Roguelike top-down em C# com MonoGame (DesktopGL).

## Objetivo do MVP

Sobreviva em salas geradas proceduralmente, derrote inimigos, colete moedas, compre itens na loja e avance de andar pelo portal até completar a run.

## Requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) 8 ou superior
- Plataforma MonoGame DesktopGL (incluída no projeto)

### Instalar .NET e PATH (Linux)

```bash
# Exemplo com instalador oficial em $HOME/.dotnet
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
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

## Controles

| Tecla | Ação |
|-------|------|
| WASD ou setas | Mover |
| R | Reiniciar a run |
