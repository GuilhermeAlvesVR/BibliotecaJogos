using System.Collections.Generic;

namespace BibliotecaJogos.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        // Navegacao N:N: uma Tag pode estar em varios Jogos.
        public List<Jogo> Jogos { get; set; } = new();
    }
}
