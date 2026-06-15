# BibliotecaJogos

Aplicacao web em C#/.NET para gerenciar uma biblioteca de jogos com avaliacoes.

## Como executar

```bash
dotnet run --project BibliotecaJogos
```

Depois acesse a URL exibida no terminal, por exemplo `http://localhost:5000`.

## Requisitos atendidos

- Persistencia de dados em banco de dados: usa SQLite com Entity Framework Core em `BibliotecaJogos/Data/BibliotecaContext.cs`.
- Tres classes relacionadas: `Jogo`, `Avaliacao` e `Tag` em `BibliotecaJogos/Models`.
- Relacionamento 1:N: um `Jogo` possui varias `Avaliacoes`, configurado em `OnModelCreating`.
- Relacionamento N:N: um `Jogo` possui varias `Tags` e uma `Tag` pertence a varios `Jogos`, configurado pela tabela `JogoTag`.
- Consulta LINQ usando dados das duas classes: rota `/consultas/jogos-avaliacoes`.
- Consulta LINQ usando funcoes de grupo: rota `/consultas/media-genero` com `Count`, `Average`, `Max` e `Min`.
- Consulta LINQ com filtro principal e filtro de grupo: rota `/consultas/generos-bem-avaliados` com `where` antes do `group` e outro `where` depois do agrupamento.
- Tecnica de uso de IA: especificacao criada no formato do GitHub Spec Kit em `specs/001-biblioteca-jogos/spec.md`.

## Paginas principais

- `/` pagina inicial.
- `/jogos` lista jogos cadastrados.
- `/jogos/novo` cadastra novo jogo.
- `/avaliacoes/nova` registra avaliacao.
- `/consultas/jogos-avaliacoes` mostra a consulta com dados das duas classes.
- `/consultas/media-genero` mostra a consulta com agrupamento por genero.
- `/consultas/generos-bem-avaliados` mostra a consulta com filtro principal e filtro de grupo.
