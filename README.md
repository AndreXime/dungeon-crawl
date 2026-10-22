# Isaac Roguelike (MVP)

Protótipo de jogo roguelike top-down em tempo real desenvolvido com TypeScript, Vite e `rot-js`.

## Objetivo do MVP

Explorar 2 andares gerados proceduralmente (6 a 8 salas por andar), contendo salas de combate, sala de item, loja e sala de chefe. As portas permanecem trancadas até que todos os inimigos da sala atual sejam derrotados. Colete moedas e itens para fortalecer os atributos do personagem e derrote os chefes dos dois andares para vencer a partida.

## Controles

- **WASD**: Movimentação do personagem
- **Setas direcionais (Cima, Baixo, Esquerda, Direita)**: Disparo de projéteis nas 4 direções
- **R**: Reiniciar a partida após morte ou vitória

## Como instalar e executar

Instale as dependências:

```bash
npm install
```

Inicie o servidor de desenvolvimento:

```bash
npm run dev
```

Gere a build de produção:

```bash
npm run build
```

## Como executar os testes

Execute a suíte de testes automatizados com Vitest:

```bash
npm test
```
