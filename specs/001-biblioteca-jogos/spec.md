# Especificacao: Biblioteca de Jogos

## Objetivo

Criar uma aplicacao web em C#/.NET para cadastrar jogos, registrar avaliacoes e consultar relatorios usando LINQ com dados persistidos em SQLite.

## Requisitos Funcionais

- RF01: O sistema deve persistir jogos, avaliacoes e tags em banco de dados SQLite usando Entity Framework Core.
- RF02: O sistema deve permitir cadastrar jogos com titulo, genero, plataforma, ano, preco e tags.
- RF03: O sistema deve permitir registrar varias avaliacoes para um jogo.
- RF04: O sistema deve permitir associar varias tags a um jogo e reutilizar a mesma tag em varios jogos.
- RF05: O sistema deve exibir uma consulta LINQ com dados de `Jogo` e `Avaliacao`.
- RF06: O sistema deve exibir uma consulta LINQ com funcoes de grupo.
- RF07: O sistema deve exibir uma consulta LINQ com filtro principal e filtro de grupo, equivalente a `having`.

## Modelo de Dados

- `Jogo`: entidade principal da biblioteca.
- `Avaliacao`: entidade dependente, com relacionamento 1:N com `Jogo`.
- `Tag`: entidade classificadora, com relacionamento N:N com `Jogo` pela tabela `JogoTag`.

## Criterios de Aceitacao

- CA01: Ao executar a aplicacao, o banco `biblioteca.db` deve ser criado automaticamente se ainda nao existir.
- CA02: A listagem de jogos deve mostrar as tags associadas.
- CA03: O cadastro de jogo deve aceitar tags separadas por virgula.
- CA04: A rota `/consultas/jogos-avaliacoes` deve combinar dados de jogos e avaliacoes.
- CA05: A rota `/consultas/media-genero` deve agrupar avaliacoes por genero e calcular totais, medias, maior nota e menor nota.
- CA06: A rota `/consultas/generos-bem-avaliados` deve filtrar jogos por ano antes do agrupamento e filtrar grupos por media minima.

## Uso de IA / GitHub Spec Kit

Esta especificacao segue a tecnica de desenvolvimento guiado por especificacao do GitHub Spec Kit: primeiro os requisitos, modelo de dados e criterios de aceitacao foram descritos em linguagem natural; depois a implementacao foi ajustada para cumprir a especificacao.
