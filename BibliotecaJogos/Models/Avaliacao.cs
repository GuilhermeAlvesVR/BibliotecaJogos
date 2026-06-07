using System;
using System.Collections.Generic;
using System.Text;


namespace BibliotecaJogos.Models
{
    public class Avaliacao
    {
        public int Id { get; set; }
        public string Autor { get; set; } = string.Empty;
        public double Nota { get; set; }       // 0.0 a 10.0
        public string Comentario { get; set; } = string.Empty;
        public DateTime Data { get; set; }

        // Chave estrangeira
        public int JogoId { get; set; }
        public Jogo Jogo { get; set; } = null!;
    }
}