using System;
using System.Collections.Generic;
using System.Text;

using BibliotecaJogos.Data;
using BibliotecaJogos.Models;
using Microsoft.EntityFrameworkCore;




namespace BibliotecaJogos.Repositories
{
    public class JogoRepository
    {
        private readonly BibliotecaContext _ctx;

        public JogoRepository(BibliotecaContext ctx)
        {
            _ctx = ctx;
        }

        // ─────────────────────────────────────────────────────────
        // CONSULTA 1 — Join entre Jogo e Avaliacao
        // Requisito: "consulta usando dados das duas classes"
        // ─────────────────────────────────────────────────────────
        public void ConsultaJogosComAvaliacoes()
        {
            Console.WriteLine("\n══ CONSULTA 1 — Jogos com suas Avaliações ══\n");

            var resultado = from j in _ctx.Jogos
                            join a in _ctx.Avaliacoes on j.Id equals a.JogoId
                            orderby j.Titulo, a.Nota descending
                            select new
                            {
                                Jogo = j.Titulo,
                                Plataforma = j.Plataforma,
                                Avaliador = a.Autor,
                                Nota = a.Nota,
                                Comentario = a.Comentario
                            };

            foreach (var r in resultado)
            {
                Console.WriteLine($"🎮 {r.Jogo,-30} [{r.Plataforma}]");
                Console.WriteLine($"   👤 {r.Avaliador,-20} ⭐ {r.Nota:F1}  \"{r.Comentario}\"");
                Console.WriteLine();
            }
        }

        // ─────────────────────────────────────────────────────────
        // CONSULTA 2 — GroupBy com funções de grupo
        // Requisito: "consulta usando funções de grupo"
        // ─────────────────────────────────────────────────────────
        public void ConsultaMediaPorGenero()
        {
            Console.WriteLine("\n══ CONSULTA 2 — Média de Notas por Gênero ══\n");

            var resultado = from j in _ctx.Jogos
                            join a in _ctx.Avaliacoes on j.Id equals a.JogoId
                            group a by j.Genero into grp
                            orderby grp.Average(a => a.Nota) descending
                            select new
                            {
                                Genero = grp.Key,
                                TotalJogos = grp.Select(a => a.JogoId).Distinct().Count(),
                                TotalAval = grp.Count(),
                                MediaNota = grp.Average(a => a.Nota),
                                MaiorNota = grp.Max(a => a.Nota),
                                MenorNota = grp.Min(a => a.Nota)
                            };

            Console.WriteLine($"{"Gênero",-16} {"Jogos",6} {"Aval.",6} {"Média",6} {"Máx.",6} {"Mín.",6}");
            Console.WriteLine(new string('─', 52));

            foreach (var r in resultado)
            {
                Console.WriteLine(
                    $"{r.Genero,-16} {r.TotalJogos,6} {r.TotalAval,6} " +
                    $"{r.MediaNota,6:F2} {r.MaiorNota,6:F1} {r.MenorNota,6:F1}");
            }
        }

        // ─────────────────────────────────────────────────────────
        // CONSULTA 3 — Where (filtro principal) + Where no grupo (having)
        // Requisito: "filtro principal (where) e filtro do grupo (having)"
        // ─────────────────────────────────────────────────────────
        public void ConsultaGenerosBemAvaliadosRecentes(int anoMinimo, double notaMinima)
        {
            Console.WriteLine($"\n══ CONSULTA 3 — Gêneros populares (jogos ≥ {anoMinimo}, média ≥ {notaMinima}) ══\n");

            var resultado = from j in _ctx.Jogos
                            join a in _ctx.Avaliacoes on j.Id equals a.JogoId
                            // WHERE principal: filtra somente jogos a partir do ano mínimo
                            where j.AnoLancamento >= anoMinimo
                            group a by j.Genero into grp
                            // HAVING: mantém somente grupos com média acima do limite
                            where grp.Average(a => a.Nota) >= notaMinima
                            orderby grp.Average(a => a.Nota) descending
                            select new
                            {
                                Genero = grp.Key,
                                Avaliacoes = grp.Count(),
                                Media = grp.Average(a => a.Nota)
                            };

            if (!resultado.Any())
            {
                Console.WriteLine("Nenhum gênero encontrado com esses critérios.");
                return;
            }

            foreach (var r in resultado)
            {
                Console.WriteLine($"✅ {r.Genero,-16} — {r.Avaliacoes} avaliações — média {r.Media:F2}");
            }
        }

        // ─────────────────────────────────────────────────────────
        // CRUD auxiliar
        // ─────────────────────────────────────────────────────────
        public void AdicionarJogo(Jogo jogo)
        {
            _ctx.Jogos.Add(jogo);
            _ctx.SaveChanges();
            Console.WriteLine($"✔ Jogo \"{jogo.Titulo}\" cadastrado com Id={jogo.Id}");
        }

        public void AdicionarAvaliacao(Avaliacao avaliacao)
        {
            _ctx.Avaliacoes.Add(avaliacao);
            _ctx.SaveChanges();
            Console.WriteLine($"✔ Avaliação de \"{avaliacao.Autor}\" registrada.");
        }

        public List<Jogo> ListarTodos()
        {
            return _ctx.Jogos.Include(j => j.Avaliacoes).ToList();
        }
    }
}