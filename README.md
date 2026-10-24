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

## Baixar e jogar

Na página **Releases** do repositório no GitHub, baixe o zip da sua plataforma (`linux-x64.zip` ou `win-x64.zip`). Não precisa instalar .NET.

**Linux**

```bash
unzip linux-x64.zip
cd linux-x64
chmod +x ChamberCrawl
./ChamberCrawl
```

**Windows**

Descompacte `win-x64.zip` e execute `ChamberCrawl.exe`.

Preferências (tela cheia / escala) gravam em `settings.json` na mesma pasta do executável. A pasta precisa ser gravável.

## Desenvolvimento

### Requisitos

- [.NET SDK](https://dotnet.microsoft.com/download) 9 (ou 8+ com roll-forward)
- Plataforma MonoGame DesktopGL (incluída no projeto)
- [just](https://github.com/casey/just) opcional (`sudo apt install just`) para atalhos do `justfile`

### Como rodar

```bash
just run
# ou: dotnet run --project ChamberCrawl
```

### Testes e build

```bash
just test
just build
```

### Distribuição (gerar os zips das Releases)

Gera pasta self-contained e zip em `dist/` (são os artefatos publicados nas Releases):

```bash
just publish-linux   # dist/linux-x64/ + dist/linux-x64.zip
just publish-win     # dist/win-x64/ + dist/win-x64.zip
just publish-all
```

`just` (sem argumentos) lista todas as receitas.

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
