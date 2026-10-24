# Desenvolvimento — Dungeon Crawl

Stack: C# / .NET 9, MonoGame DesktopGL. Projeto em `ChamberCrawl/`.

## Requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) 9 (ou 8+ com roll-forward)
- [just](https://github.com/casey/just) opcional (`sudo apt install just`) — atalhos no `justfile`
- `zip` no PATH para `just publish-*`

## Comandos

```bash
just              # lista receitas
just run          # dotnet run --project ChamberCrawl
just test
just build
just publish-linux   # dist/linux-x64/ + dist/linux-x64.zip
just publish-win
just publish-all
```

Sem just:

```bash
dotnet run --project ChamberCrawl
dotnet test
dotnet build ChamberCrawl/ChamberCrawl.csproj -c Release
```

Os publishes são **self-contained** (o jogador não precisa de .NET). Artefatos em `dist/`.

## Docs e ferramentas

| Caminho | Conteúdo |
|---------|----------|
| [docs/FLOORS.md](docs/FLOORS.md) | Biomas 1–5 |
| [docs/MECHANICS.md](docs/MECHANICS.md) | Mecânicas da run |
| [docs/SPRITE_STYLE.md](docs/SPRITE_STYLE.md) | Estilo, paleta, inventário de sprites |
| `tools/sprite-studio/` | Editor + `import-generated.js` |
| `tools/music-studio/` | Temas Strudel → WAV |

Preferências em runtime: `settings.json` ao lado do executável (`AppContext.BaseDirectory`).
