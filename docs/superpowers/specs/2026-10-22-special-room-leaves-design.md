# Salas especiais como folhas

## Regras

- Boss, Shop e Item: exatamente 1 vizinho (uma porta).
- Boss: Manhattan `|x|+|y| > 3` a partir da entrada `(0,0)`.

## Geração

1. Crescer o andar (6–8 salas) e ligar adjacências na grade.
2. Escolher/criar folha para Boss com Manhattan `> 3`.
3. Escolher/criar folhas distintas para Shop e Item.
4. Start em `(0,0)`. Demais salas `Normal`.
