using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotecaJogos.Models
{
    public class Jogo
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public string Plataforma { get; set; } = string.Empty;
        public int AnoLancamento { get; set; }
        public decimal Preco { get; set; }

        // Navegação — um Jogo tem muitas Avaliações
        public List<Avaliacao> Avaliacoes { get; set; } = new();
    }
}