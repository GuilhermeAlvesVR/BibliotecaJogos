using BibliotecaJogos.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaJogos.Data
{
    public static class DatabaseSetup
    {
        public static void Initialize(BibliotecaContext context)
        {
            context.Database.EnsureCreated();
            EnsureSchema(context);

            if (!context.Jogos.Any())
            {
                SeedDados(context);
            }
            else if (!context.Tags.Any())
            {
                SeedTags(context);
            }
        }

        public static List<Tag> ObterTags(BibliotecaContext context, string texto)
        {
            var nomes = texto
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(nome => !string.IsNullOrWhiteSpace(nome))
                .Select(nome => nome.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var tags = new List<Tag>();
            foreach (var nome in nomes)
            {
                var nomeNormalizado = nome.ToLowerInvariant();
                var tag = context.Tags.Local.FirstOrDefault(t => t.Nome.ToLower() == nomeNormalizado)
                    ?? context.Tags.FirstOrDefault(t => t.Nome.ToLower() == nomeNormalizado);

                if (tag is null)
                {
                    tag = new Tag { Nome = nome };
                    context.Tags.Add(tag);
                }

                tags.Add(tag);
            }

            return tags;
        }

        private static void EnsureSchema(BibliotecaContext context)
        {
            context.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS "Tags" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_Tags" PRIMARY KEY AUTOINCREMENT,
                    "Nome" TEXT NOT NULL
                );
                """);

            context.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS "JogoTag" (
                    "JogosId" INTEGER NOT NULL,
                    "TagsId" INTEGER NOT NULL,
                    CONSTRAINT "PK_JogoTag" PRIMARY KEY ("JogosId", "TagsId"),
                    CONSTRAINT "FK_JogoTag_Jogos_JogosId" FOREIGN KEY ("JogosId") REFERENCES "Jogos" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_JogoTag_Tags_TagsId" FOREIGN KEY ("TagsId") REFERENCES "Tags" ("Id") ON DELETE CASCADE
                );
                """);

            context.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS \"IX_JogoTag_TagsId\" ON \"JogoTag\" (\"TagsId\");");
        }

        private static void SeedDados(BibliotecaContext context)
        {
            var jogos = new List<Jogo>
            {
                new() { Titulo="Elden Ring",          Genero="RPG",       Plataforma="PC",     AnoLancamento=2022, Preco=249.90m },
                new() { Titulo="The Last of Us II",   Genero="Aventura",  Plataforma="PS5",    AnoLancamento=2020, Preco=199.90m },
                new() { Titulo="Hollow Knight",       Genero="RPG",       Plataforma="Switch", AnoLancamento=2017, Preco=49.90m  },
                new() { Titulo="Valorant",            Genero="FPS",       Plataforma="PC",     AnoLancamento=2020, Preco=0m      },
                new() { Titulo="Celeste",             Genero="Plataforma",Plataforma="PC",     AnoLancamento=2018, Preco=39.90m  },
                new() { Titulo="God of War Ragnarok", Genero="Aventura",  Plataforma="PS5",    AnoLancamento=2022, Preco=299.90m },
                new() { Titulo="Hades",               Genero="RPG",       Plataforma="PC",     AnoLancamento=2020, Preco=59.90m  },
                new() { Titulo="CS2",                 Genero="FPS",       Plataforma="PC",     AnoLancamento=2023, Preco=0m      },
            };

            context.Jogos.AddRange(jogos);
            context.SaveChanges();

            AtribuirTags(jogos[0], ObterTags(context, "soulslike, mundo aberto, rpg"));
            AtribuirTags(jogos[1], ObterTags(context, "historia, acao, exclusivo"));
            AtribuirTags(jogos[2], ObterTags(context, "indie, metroidvania, rpg"));
            AtribuirTags(jogos[3], ObterTags(context, "multiplayer, competitivo, fps"));
            AtribuirTags(jogos[4], ObterTags(context, "indie, plataforma, dificil"));
            AtribuirTags(jogos[5], ObterTags(context, "acao, historia, exclusivo"));
            AtribuirTags(jogos[6], ObterTags(context, "roguelike, indie, rpg"));
            AtribuirTags(jogos[7], ObterTags(context, "multiplayer, competitivo, fps"));
            context.SaveChanges();

            var avals = new List<Avaliacao>
            {
                new() { JogoId=jogos[0].Id, Autor="Alex",    Nota=9.5, Comentario="Obra-prima absoluta",       Data=DateTime.Now },
                new() { JogoId=jogos[0].Id, Autor="Lucas",   Nota=9.0, Comentario="Dificil, mas incrivel",     Data=DateTime.Now },
                new() { JogoId=jogos[0].Id, Autor="Mari",    Nota=8.5, Comentario="Mundo vasto e detalhado",   Data=DateTime.Now },
                new() { JogoId=jogos[1].Id, Autor="Pedro",   Nota=9.8, Comentario="Historia emocionante",      Data=DateTime.Now },
                new() { JogoId=jogos[1].Id, Autor="Joao",    Nota=7.0, Comentario="Gameplay travado as vezes", Data=DateTime.Now },
                new() { JogoId=jogos[2].Id, Autor="Carla",   Nota=9.2, Comentario="Melhor metroidvania",       Data=DateTime.Now },
                new() { JogoId=jogos[3].Id, Autor="Rafael",  Nota=8.0, Comentario="Competitivo viciante",      Data=DateTime.Now },
                new() { JogoId=jogos[3].Id, Autor="Bia",     Nota=6.5, Comentario="Cheio de hackers",          Data=DateTime.Now },
                new() { JogoId=jogos[4].Id, Autor="Thiago",  Nota=9.4, Comentario="Perfeito para indie",       Data=DateTime.Now },
                new() { JogoId=jogos[5].Id, Autor="Leticia", Nota=9.7, Comentario="Melhor PS5",                Data=DateTime.Now },
                new() { JogoId=jogos[6].Id, Autor="Felipe",  Nota=9.3, Comentario="Roguelike impecavel",       Data=DateTime.Now },
                new() { JogoId=jogos[7].Id, Autor="Ana",     Nota=7.5, Comentario="CS classico melhorado",     Data=DateTime.Now },
            };

            context.Avaliacoes.AddRange(avals);
            context.SaveChanges();
        }

        private static void SeedTags(BibliotecaContext context)
        {
            var jogos = context.Jogos.OrderBy(j => j.Id).ToList();
            if (jogos.Count == 0)
            {
                return;
            }

            foreach (var jogo in jogos)
            {
                var tags = ObterTags(context, jogo.Genero);
                AtribuirTags(jogo, tags);
            }

            context.SaveChanges();
        }

        private static void AtribuirTags(Jogo jogo, List<Tag> tags)
        {
            foreach (var tag in tags)
            {
                if (!jogo.Tags.Any(t => string.Equals(t.Nome, tag.Nome, StringComparison.OrdinalIgnoreCase)))
                {
                    jogo.Tags.Add(tag);
                }
            }
        }
    }
}
