# Dungeon Crawl

Roguelike top-down. Você é um **mago** que dispara **bolts** mágicos por salas de dungeon medieval.

https://github.com/user-attachments/assets/df57e047-3ee4-4d96-96f7-7ba78abc235e

## Objetivo

Sobreviva a **5 andares** gerados proceduralmente, limpe salas, colete moedas, compre itens e derrote o boss de cada bioma. A vitória da run é no andar 5.

| Andar | Bioma | Destaques |
|------:|-------|-----------|
| 1 | Masmorra | Morcego, Esqueleto, Slime · boss **Ogre** |
| 2 | Catacumbas | Espectro · boss **Necromante** · spikes ósseos |
| 3 | Esgotos | Rato · boss **Rei Lodo** · poças lentas |
| 4 | Arsenal | Guarda · boss **Golem de Ferro** · barricadas/bigornas |
| 5 | Santuário | Acólito · boss **Senhor das Runas** · runas/pedestais |

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

## Menu

**Começar**, **Configurações** (tela cheia e escala 1×–4×) e **Sair**.
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

## Quer contribuir ou compilar?

Veja [DEVELOPMENT.md](DEVELOPMENT.md).
