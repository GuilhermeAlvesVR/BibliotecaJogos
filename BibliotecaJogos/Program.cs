using BibliotecaJogos.Data;
using BibliotecaJogos.Models;
using BibliotecaJogos.Repositories;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var context = new BibliotecaContext();
context.Database.EnsureCreated();

var repo = new JogoRepository(context);

if (!context.Jogos.Any())
{
    MostrarLoading("Inicializando banco de dados");
    SeedDados(repo);
}

MostrarIntro();

bool sair = false;
while (!sair)
{
    MostrarMenu();
    Console.Write("  Opção: ");
    var opcao = Console.ReadLine()?.Trim();

    switch (opcao)
    {
        case "1":
            MostrarLoading("Carregando jogos");
            var jogos = repo.ListarTodos();
            Console.Clear();
            Titulo("📋  TODOS OS JOGOS", ConsoleColor.Cyan);
            Console.WriteLine();
            foreach (var j in jogos)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write($"  [{j.Id:D2}] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{j.Titulo,-28} ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{j.Genero,-14} ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($"{j.Plataforma,-10} ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"{j.AnoLancamento}");
            }
            Console.ResetColor();
            Pausar();
            break;

        case "2":
            MostrarLoading("Buscando avaliações");
            Console.Clear();
            repo.ConsultaJogosComAvaliacoes();
            Pausar();
            break;

        case "3":
            MostrarLoading("Calculando médias");
            Console.Clear();
            repo.ConsultaMediaPorGenero();
            Pausar();
            break;

        case "4":
            Console.Clear();
            Titulo("🔍  FILTRO AVANÇADO", ConsoleColor.Yellow);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  Ano mínimo de lançamento (ex: 2020): ");
            Console.ForegroundColor = ConsoleColor.White;
            int ano = int.TryParse(Console.ReadLine(), out var a) ? a : 2020;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  Nota mínima do grupo (ex: 7.5): ");
            Console.ForegroundColor = ConsoleColor.White;
            double nota = double.TryParse(Console.ReadLine(), out var n) ? n : 7.5;
            MostrarLoading("Filtrando resultados");
            Console.Clear();
            repo.ConsultaGenerosBemAvaliadosRecentes(ano, nota);
            Pausar();
            break;

        case "5":
            CadastrarJogo(repo);
            break;

        case "6":
            AvaliarJogo(repo, context);
            break;

        case "0":
            sair = true;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n\n  Até logo, gamer! 🎮\n\n");
            Console.ResetColor();
            Thread.Sleep(1200);
            break;

        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  ⚠  Opção inválida. Tente novamente.");
            Console.ResetColor();
            Thread.Sleep(800);
            break;
    }
}

// ══════════════════════════════════════════════
//  VISUAL
// ══════════════════════════════════════════════

static void MostrarIntro()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(@"
  ██████╗ ██╗██████╗ ██╗     ██╗ ██████╗ ████████╗███████╗ ██████╗ █████╗ 
  ██╔══██╗██║██╔══██╗██║     ██║██╔═══██╗╚══██╔══╝██╔════╝██╔════╝██╔══██╗
  ██████╔╝██║██████╔╝██║     ██║██║   ██║   ██║   █████╗  ██║     ███████║
  ██╔══██╗██║██╔══██╗██║     ██║██║   ██║   ██║   ██╔══╝  ██║     ██╔══██║
  ██████╔╝██║██████╔╝███████╗██║╚██████╔╝   ██║   ███████╗╚██████╗██║  ██║
  ╚═════╝ ╚═╝╚═════╝ ╚══════╝╚═╝ ╚═════╝   ╚═╝   ╚══════╝ ╚═════╝╚═╝  ╚═╝");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("                         📚  DE  JOGOS\n");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  ─────────────────────────────────────────────────────────────────────");
    Console.ResetColor();
    Thread.Sleep(1500);
}

static void MostrarMenu()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n  ╔══════════════════════════════════════╗");
    Console.WriteLine("  ║       📚  BIBLIOTECA DE JOGOS        ║");
    Console.WriteLine("  ╠══════════════════════════════════════╣");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("  ║  1  · 🎮  Listar todos os jogos      ║");
    Console.WriteLine("  ║  2  · 🔗  Jogos com avaliações       ║");
    Console.WriteLine("  ║  3  · 📊  Média por gênero           ║");
    Console.WriteLine("  ║  4  · 🔍  Filtro avançado            ║");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("  ║  5  · ➕  Cadastrar jogo             ║");
    Console.WriteLine("  ║  6  · ⭐  Avaliar jogo               ║");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("  ║  0  · 🚪  Sair                       ║");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("  ╚══════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

static void Titulo(string texto, ConsoleColor cor)
{
    Console.ForegroundColor = cor;
    Console.WriteLine($"\n  ╔{'═'.ToString().PadRight(texto.Length + 2, '═')}╗");
    Console.WriteLine($"  ║ {texto} ║");
    Console.WriteLine($"  ╚{'═'.ToString().PadRight(texto.Length + 2, '═')}╝");
    Console.ResetColor();
}

static void MostrarLoading(string mensagem)
{
    Console.WriteLine();
    string[] frames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
    for (int i = 0; i < 20; i++)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"\r  {frames[i % frames.Length]}  {mensagem}...");
        Console.ResetColor();
        Thread.Sleep(60);
    }
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write($"\r  ✔  {mensagem}     \n");
    Console.ResetColor();
    Thread.Sleep(200);
}

static void BarraEstrelas(double nota)
{
    int cheias = (int)Math.Round(nota / 10.0 * 10);
    Console.ForegroundColor = ConsoleColor.Yellow;
    for (int i = 0; i < 10; i++)
    {
        Console.Write(i < cheias ? "█" : "░");
        Thread.Sleep(40);
    }
    Console.ResetColor();
}

static void CorDaNota(double nota)
{
    if (nota >= 8.0) Console.ForegroundColor = ConsoleColor.Green;
    else if (nota >= 6.0) Console.ForegroundColor = ConsoleColor.Yellow;
    else Console.ForegroundColor = ConsoleColor.Red;
}

static void Pausar()
{
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("\n  Pressione qualquer tecla para voltar...");
    Console.ResetColor();
    Console.ReadKey(true);
}

// ══════════════════════════════════════════════
//  CADASTRO
// ══════════════════════════════════════════════

static void CadastrarJogo(JogoRepository repo)
{
    Console.Clear();
    Titulo("➕  CADASTRAR JOGO", ConsoleColor.Green);
    Console.WriteLine();

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Título: ");
    Console.ForegroundColor = ConsoleColor.White;
    var titulo = Console.ReadLine() ?? "";

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Gênero (ex: RPG, FPS, Aventura): ");
    Console.ForegroundColor = ConsoleColor.White;
    var genero = Console.ReadLine() ?? "";

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Plataforma (ex: PC, PS5, Switch): ");
    Console.ForegroundColor = ConsoleColor.White;
    var plataforma = Console.ReadLine() ?? "";

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Ano de lançamento: ");
    Console.ForegroundColor = ConsoleColor.White;
    int.TryParse(Console.ReadLine(), out int anoJ);

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Preço (ex: 199.90): ");
    Console.ForegroundColor = ConsoleColor.White;
    decimal.TryParse(Console.ReadLine(), out decimal preco);

    MostrarLoading("Salvando jogo");

    repo.AdicionarJogo(new Jogo
    {
        Titulo = titulo,
        Genero = genero,
        Plataforma = plataforma,
        AnoLancamento = anoJ,
        Preco = preco
    });

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n  ✔  \"{titulo}\" cadastrado com sucesso!");
    Console.ResetColor();
    Pausar();
}

// ══════════════════════════════════════════════
//  AVALIAÇÃO DINÂMICA
// ══════════════════════════════════════════════

static void AvaliarJogo(JogoRepository repo, BibliotecaContext ctx)
{
    Console.Clear();
    Titulo("⭐  AVALIAR JOGO", ConsoleColor.Yellow);
    Console.WriteLine();

    var jogos = repo.ListarTodos();
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("  Jogos disponíveis:\n");
    foreach (var j in jogos)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"  [{j.Id:D2}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(j.Titulo);
    }

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  ID do jogo: ");
    Console.ForegroundColor = ConsoleColor.White;
    int.TryParse(Console.ReadLine(), out int id);

    var jogoEscolhido = ctx.Jogos.Find(id);
    if (jogoEscolhido == null)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ⚠  Jogo não encontrado.");
        Console.ResetColor();
        Pausar();
        return;
    }

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Seu nome: ");
    Console.ForegroundColor = ConsoleColor.White;
    var autor = Console.ReadLine() ?? "Anônimo";

    // Nota com validação visual
    double nota = 0;
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  Nota (0 a 10): ");
        Console.ForegroundColor = ConsoleColor.White;
        if (double.TryParse(Console.ReadLine(), out nota) && nota >= 0 && nota <= 10) break;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ⚠  Digite um número entre 0 e 10.");
    }

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write("  Comentário: ");
    Console.ForegroundColor = ConsoleColor.White;
    var comentario = Console.ReadLine() ?? "";

    MostrarLoading("Enviando avaliação");

    repo.AdicionarAvaliacao(new Avaliacao
    {
        JogoId = id,
        Autor = autor,
        Nota = nota,
        Comentario = comentario,
        Data = DateTime.Now
    });

    // Card de confirmação animado
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("\n  ╔════════════════════════════════════════╗");
    Console.WriteLine("  ║       🎮  AVALIAÇÃO REGISTRADA!        ║");
    Console.WriteLine("  ╠════════════════════════════════════════╣");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"  ║  Jogo   : {jogoEscolhido.Titulo,-30}║");
    Console.WriteLine($"  ║  Por    : {autor,-30}║");
    Console.Write("  ║  Nota   : ");
    BarraEstrelas(nota);
    CorDaNota(nota);
    Console.WriteLine($"  {nota:F1}/10  ║");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("  ╚════════════════════════════════════════╝");
    Console.ResetColor();
    Pausar();
}

// ══════════════════════════════════════════════
//  SEED
// ══════════════════════════════════════════════

static void SeedDados(JogoRepository repo)
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
    foreach (var j in jogos) repo.AdicionarJogo(j);

    var avals = new List<Avaliacao>
    {
        new() { JogoId=1, Autor="Alex",    Nota=9.5, Comentario="Obra-prima absoluta",       Data=DateTime.Now },
        new() { JogoId=1, Autor="Lucas",   Nota=9.0, Comentario="Difícil, mas incrível",     Data=DateTime.Now },
        new() { JogoId=1, Autor="Mari",    Nota=8.5, Comentario="Mundo vasto e detalhado",   Data=DateTime.Now },
        new() { JogoId=2, Autor="Pedro",   Nota=9.8, Comentario="História emocionante",      Data=DateTime.Now },
        new() { JogoId=2, Autor="João",    Nota=7.0, Comentario="Gameplay travado às vezes", Data=DateTime.Now },
        new() { JogoId=3, Autor="Carla",   Nota=9.2, Comentario="Melhor metroidvania",       Data=DateTime.Now },
        new() { JogoId=4, Autor="Rafael",  Nota=8.0, Comentario="Competitivo viciante",      Data=DateTime.Now },
        new() { JogoId=4, Autor="Bia",     Nota=6.5, Comentario="Cheio de hackers",          Data=DateTime.Now },
        new() { JogoId=5, Autor="Thiago",  Nota=9.4, Comentario="Perfeito para indie",       Data=DateTime.Now },
        new() { JogoId=6, Autor="Leticia", Nota=9.7, Comentario="Melhor PS5",                Data=DateTime.Now },
        new() { JogoId=7, Autor="Felipe",  Nota=9.3, Comentario="Roguelike impecável",       Data=DateTime.Now },
        new() { JogoId=8, Autor="Ana",     Nota=7.5, Comentario="CS clássico melhorado",     Data=DateTime.Now },
    };
    foreach (var a in avals) repo.AdicionarAvaliacao(a);
}