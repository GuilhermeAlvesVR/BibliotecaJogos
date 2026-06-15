using System.Globalization;
using System.Net;
using BibliotecaJogos.Data;
using BibliotecaJogos.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

using (var context = new BibliotecaContext())
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

app.MapGet("/", () => Html("Inicio", """
    <section class="hero">
        <div>
            <h1>Biblioteca de Jogos</h1>
            <p class="hero-text">Organize seus jogos, cadastre novas avaliacoes e acompanhe as notas da biblioteca.</p>
            <div class="actions">
                <a class="button" href="/jogos">Ver biblioteca</a>
                <a class="button ghost" href="/jogos/novo">Cadastrar jogo</a>
            </div>
        </div>
    </section>
    <section class="section-title">
        <h2>Acesso rapido</h2>
    </section>
    <section class="grid">
        <a class="card" href="/jogos"><span class="icon">🎮</span><strong>Jogos</strong><span>Veja todos os jogos cadastrados.</span></a>
        <a class="card" href="/jogos/novo"><span class="icon">＋</span><strong>Cadastrar jogo</strong><span>Adicione um novo jogo a biblioteca.</span></a>
        <a class="card" href="/avaliacoes/nova"><span class="icon">★</span><strong>Avaliar jogo</strong><span>Registre uma nota e um comentario.</span></a>
    </section>
    <section class="reports-bar">
        <span>Relatorios:</span>
        <a href="/consultas/jogos-avaliacoes">Jogos avaliados</a>
        <a href="/consultas/media-genero">Notas por genero</a>
        <a href="/consultas/generos-bem-avaliados">Destaques</a>
    </section>
    """));

app.MapGet("/jogos", () =>
{
    using var context = new BibliotecaContext();
    var jogos = context.Jogos
        .Include(j => j.Avaliacoes)
        .Include(j => j.Tags)
        .OrderBy(j => j.Titulo)
        .ToList();

    var linhas = string.Join("", jogos.Select(j => $"""
        <tr>
            <td>{j.Id}</td>
            <td>{JogoComCapa(j.Titulo)}</td>
            <td>{Escape(j.Genero)}</td>
            <td>{Escape(j.Plataforma)}</td>
            <td>{Escape(string.Join(", ", j.Tags.Select(t => t.Nome).OrderBy(t => t)))}</td>
            <td>{j.AnoLancamento}</td>
            <td>{j.Preco:C}</td>
            <td>{j.Avaliacoes.Count}</td>
        </tr>
        """));

    return Html("Jogos", $"""
        <h1>Jogos cadastrados</h1>
        <p class="page-intro">Catalogo completo dos jogos persistidos no banco SQLite.</p>
        <p><a class="button" href="/jogos/novo">Cadastrar novo jogo</a></p>
        <div class="table-wrap"><table>
            <thead><tr><th>ID</th><th>Titulo</th><th>Genero</th><th>Plataforma</th><th>Tags</th><th>Ano</th><th>Preco</th><th>Avaliacoes</th></tr></thead>
            <tbody>{linhas}</tbody>
        </table></div>
        """);
});

app.MapGet("/jogos/novo", () => Html("Cadastrar jogo", """
    <h1>Cadastrar jogo</h1>
    <form method="post" action="/jogos/novo" class="form">
        <label>Titulo <input name="titulo" required></label>
        <label>Genero <input name="genero" required></label>
        <label>Plataforma <input name="plataforma" required></label>
        <label>Tags <input name="tags" placeholder="Ex: multiplayer, indie, soulslike"></label>
        <label>Ano de lancamento <input name="ano" type="number" required min="1950" max="2100"></label>
        <label>Preco <input name="preco" type="number" step="0.01" required min="0"></label>
        <button type="submit">Salvar</button>
    </form>
    """));

app.MapPost("/jogos/novo", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    using var context = new BibliotecaContext();

    var jogo = new Jogo
    {
        Titulo = form["titulo"].ToString(),
        Genero = form["genero"].ToString(),
        Plataforma = form["plataforma"].ToString(),
        AnoLancamento = ParseInt(form["ano"].ToString()),
        Preco = ParseDecimal(form["preco"].ToString())
    };

    jogo.Tags.AddRange(ObterTags(context, form["tags"].ToString()));
    context.Jogos.Add(jogo);

    context.SaveChanges();
    return Results.Redirect("/jogos");
});

app.MapGet("/avaliacoes/nova", () =>
{
    using var context = new BibliotecaContext();
    var jogos = context.Jogos.OrderBy(j => j.Titulo).ToList();
    var opcoes = string.Join("", jogos.Select(j => $"<option value=\"{j.Id}\">{Escape(j.Titulo)}</option>"));

    return Html("Nova avaliacao", $"""
        <h1>Avaliar jogo</h1>
        <form method="post" action="/avaliacoes/nova" class="form">
            <label>Jogo <select name="jogoId" required>{opcoes}</select></label>
            <label>Seu nome <input name="autor" required></label>
            <label>Nota de 0 a 10 <input name="nota" type="number" step="0.1" min="0" max="10" required></label>
            <label>Comentario <textarea name="comentario" required></textarea></label>
            <button type="submit">Registrar avaliacao</button>
        </form>
        """);
});

app.MapPost("/avaliacoes/nova", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    using var context = new BibliotecaContext();

    context.Avaliacoes.Add(new Avaliacao
    {
        JogoId = ParseInt(form["jogoId"].ToString()),
        Autor = form["autor"].ToString(),
        Nota = ParseDouble(form["nota"].ToString()),
        Comentario = form["comentario"].ToString(),
        Data = DateTime.Now
    });

    context.SaveChanges();
    return Results.Redirect("/consultas/jogos-avaliacoes");
});

app.MapGet("/consultas/jogos-avaliacoes", () =>
{
    using var context = new BibliotecaContext();

    var resultado = from j in context.Jogos
                    join a in context.Avaliacoes on j.Id equals a.JogoId
                    orderby j.Titulo, a.Nota descending
                    select new
                    {
                        Jogo = j.Titulo,
                        j.Plataforma,
                        Avaliador = a.Autor,
                        a.Nota,
                        a.Comentario
                    };

    var linhas = string.Join("", resultado.ToList().Select(r => $"""
        <tr>
            <td>{JogoComCapa(r.Jogo)}</td>
            <td>{Escape(r.Plataforma)}</td>
            <td>{Escape(r.Avaliador)}</td>
            <td>{r.Nota:F1}</td>
            <td>{Escape(r.Comentario)}</td>
        </tr>
        """));

    return Html("Jogos avaliados", $"""
        <h1>Jogos avaliados</h1>
        <p class="page-intro">Relatorio usando dados das duas classes: <code>Jogo</code> e <code>Avaliacao</code>.</p>
        <div class="table-wrap"><table>
            <thead><tr><th>Jogo</th><th>Plataforma</th><th>Avaliador</th><th>Nota</th><th>Comentario</th></tr></thead>
            <tbody>{linhas}</tbody>
        </table></div>
        """);
});

app.MapGet("/consultas/media-genero", () =>
{
    using var context = new BibliotecaContext();

    var resultado = from j in context.Jogos
                    join a in context.Avaliacoes on j.Id equals a.JogoId
                    group a by j.Genero into grp
                    orderby grp.Average(a => a.Nota) descending
                    select new
                    {
                        Genero = grp.Key,
                        TotalJogos = grp.Select(a => a.JogoId).Distinct().Count(),
                        TotalAvaliacoes = grp.Count(),
                        MediaNota = grp.Average(a => a.Nota),
                        MaiorNota = grp.Max(a => a.Nota),
                        MenorNota = grp.Min(a => a.Nota)
                    };

    var linhas = string.Join("", resultado.ToList().Select(r => $"""
        <tr>
            <td>{Escape(r.Genero)}</td>
            <td>{r.TotalJogos}</td>
            <td>{r.TotalAvaliacoes}</td>
            <td>{r.MediaNota:F2}</td>
            <td>{r.MaiorNota:F1}</td>
            <td>{r.MenorNota:F1}</td>
        </tr>
        """));

    return Html("Notas por genero", $"""
        <h1>Notas por genero</h1>
        <p class="page-intro">Relatorio com funcoes de grupo: <code>Count</code>, <code>Average</code>, <code>Max</code> e <code>Min</code>.</p>
        <div class="table-wrap"><table>
            <thead><tr><th>Genero</th><th>Jogos</th><th>Avaliacoes</th><th>Media</th><th>Maior</th><th>Menor</th></tr></thead>
            <tbody>{linhas}</tbody>
        </table></div>
        """);
});

app.MapGet("/consultas/generos-bem-avaliados", (int? anoMinimo, double? notaMinima) =>
{
    var ano = anoMinimo ?? 2020;
    var nota = notaMinima ?? 7.5;

    using var context = new BibliotecaContext();

    var resultado = from j in context.Jogos
                    join a in context.Avaliacoes on j.Id equals a.JogoId
                    where j.AnoLancamento >= ano
                    group a by j.Genero into grp
                    where grp.Average(a => a.Nota) >= nota
                    orderby grp.Average(a => a.Nota) descending
                    select new
                    {
                        Genero = grp.Key,
                        Avaliacoes = grp.Count(),
                        Media = grp.Average(a => a.Nota)
                    };

    var linhas = string.Join("", resultado.ToList().Select(r => $"""
        <tr>
            <td>{Escape(r.Genero)}</td>
            <td>{r.Avaliacoes}</td>
            <td>{r.Media:F2}</td>
        </tr>
        """));

    if (linhas.Length == 0)
    {
        linhas = "<tr><td colspan=\"3\">Nenhum genero encontrado com esses criterios.</td></tr>";
    }

    return Html("Generos em destaque", $"""
        <h1>Generos em destaque</h1>
        <p class="page-intro">Relatorio com <code>where</code> principal antes do agrupamento e filtro do grupo apos o <code>group</code>, funcionando como <code>having</code>.</p>
        <form method="get" action="/consultas/generos-bem-avaliados" class="inline-form">
            <label>Ano minimo <input name="anoMinimo" type="number" value="{ano}"></label>
            <label>Nota minima <input name="notaMinima" type="number" step="0.1" value="{nota.ToString(CultureInfo.InvariantCulture)}"></label>
            <button type="submit">Filtrar</button>
        </form>
        <div class="table-wrap"><table>
            <thead><tr><th>Genero</th><th>Avaliacoes</th><th>Media</th></tr></thead>
            <tbody>{linhas}</tbody>
        </table></div>
        """);
});

app.Run();

static IResult Html(string titulo, string conteudo)
{
    var pagina = $$"""
        <!doctype html>
        <html lang="pt-br">
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{{Escape(titulo)}} - Biblioteca de Jogos</title>
            <style>
                :root {
                    color-scheme: dark;
                    font-family: Inter, Segoe UI, Arial, sans-serif;
                    background: #0a0f1d;
                    color: #edf4ff;
                }

                * { box-sizing: border-box; }

                body {
                    margin: 0;
                    min-height: 100vh;
                    background:
                        radial-gradient(circle at 18% 12%, rgba(67, 119, 255, .28), transparent 28rem),
                        radial-gradient(circle at 88% 8%, rgba(255, 184, 77, .14), transparent 24rem),
                        linear-gradient(180deg, #0a0f1d, #111827 54%, #0a0f1d);
                }

                header {
                    position: sticky;
                    top: 0;
                    z-index: 2;
                    background: rgba(10, 15, 29, .78);
                    backdrop-filter: blur(18px);
                    border-bottom: 1px solid rgba(148, 163, 184, .18);
                    padding: 16px 28px;
                    display: flex;
                    gap: 22px;
                    align-items: center;
                    justify-content: space-between;
                    flex-wrap: wrap;
                }

                header > a {
                    color: #ffffff;
                    text-decoration: none;
                    font-weight: 900;
                    letter-spacing: -.02em;
                    font-size: 1.08rem;
                }

                nav {
                    display: flex;
                    gap: 8px;
                    flex-wrap: wrap;
                }

                nav a {
                    color: #b8c7e6;
                    text-decoration: none;
                    font-weight: 700;
                    font-size: .9rem;
                    padding: 8px 10px;
                    border-radius: 999px;
                }

                nav a:hover {
                    color: #ffffff;
                    background: rgba(79, 140, 255, .18);
                }

                main {
                    max-width: 1180px;
                    margin: 0 auto;
                    padding: 38px 20px 70px;
                }

                h1, h2 {
                    margin: 0;
                    color: #ffffff;
                    letter-spacing: -.04em;
                }

                h1 { font-size: clamp(2.2rem, 6vw, 4.8rem); line-height: .95; }
                h2 { font-size: clamp(1.45rem, 3vw, 2.2rem); }
                p { color: #b8c7e6; }

                .hero {
                    display: grid;
                    grid-template-columns: 1fr;
                    gap: 24px;
                    align-items: stretch;
                    background: linear-gradient(135deg, rgba(23, 36, 68, .94), rgba(13, 20, 36, .9));
                    border: 1px solid rgba(148, 163, 184, .22);
                    border-radius: 30px;
                    padding: clamp(26px, 5vw, 54px);
                    box-shadow: 0 28px 70px rgba(0, 0, 0, .32);
                    overflow: hidden;
                    position: relative;
                }

                .hero::after {
                    content: "";
                    position: absolute;
                    width: 280px;
                    height: 280px;
                    right: -80px;
                    bottom: -130px;
                    background: radial-gradient(circle, rgba(79, 140, 255, .35), transparent 68%);
                }

                .hero > * { position: relative; z-index: 1; }
                .hero-text { max-width: 660px; font-size: 1.12rem; line-height: 1.7; }
                .tag { color: #93c5fd; text-transform: uppercase; letter-spacing: .12em; font-size: .78rem; font-weight: 900; }

                .actions {
                    display: flex;
                    gap: 12px;
                    flex-wrap: wrap;
                    margin-top: 26px;
                }

                .button, button {
                    background: linear-gradient(135deg, #4f8cff, #7c3aed);
                    color: white;
                    border: 0;
                    border-radius: 999px;
                    padding: 11px 17px;
                    text-decoration: none;
                    font-weight: 900;
                    cursor: pointer;
                    box-shadow: 0 12px 24px rgba(79, 140, 255, .25);
                }

                .button.ghost {
                    background: rgba(255, 255, 255, .08);
                    border: 1px solid rgba(255, 255, 255, .18);
                    box-shadow: none;
                }

                .section-title { margin: 34px 0 14px; }
                .section-title h2 { margin-top: 6px; }
                .page-intro { margin-top: 6px; margin-bottom: 22px; }

                .grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(245px, 1fr));
                    gap: 16px;
                    margin-top: 18px;
                }

                .card {
                    min-height: 178px;
                    background: rgba(17, 27, 49, .78);
                    border: 1px solid rgba(148, 163, 184, .18);
                    border-radius: 22px;
                    padding: 22px;
                    color: #e5edf7;
                    text-decoration: none;
                    display: grid;
                    align-content: start;
                    gap: 10px;
                    transition: transform .18s ease, border-color .18s ease, background .18s ease;
                }

                .card:hover {
                    transform: translateY(-4px);
                    border-color: rgba(79, 140, 255, .62);
                    background: rgba(25, 39, 70, .9);
                }

                .card strong { font-size: 1.12rem; color: #ffffff; }
                .card span:not(.icon) { color: #b8c7e6; line-height: 1.55; }
                .card.accent { border-color: rgba(251, 191, 36, .28); }

                .icon {
                    width: 42px;
                    height: 42px;
                    display: inline-grid;
                    place-items: center;
                    border-radius: 14px;
                    background: rgba(79, 140, 255, .16);
                    color: #bfdbfe;
                    font-weight: 950;
                }

                .card.accent .icon {
                    background: rgba(251, 191, 36, .16);
                    color: #fde68a;
                }

                .reports-bar {
                    display: flex;
                    gap: 10px;
                    align-items: center;
                    flex-wrap: wrap;
                    margin-top: 18px;
                    padding: 14px 16px;
                    border: 1px solid rgba(148, 163, 184, .16);
                    border-radius: 16px;
                    background: rgba(17, 27, 49, .48);
                    color: #b8c7e6;
                }

                .reports-bar span { font-weight: 900; color: #ffffff; }

                .reports-bar a {
                    color: #bfdbfe;
                    text-decoration: none;
                    font-weight: 800;
                    padding: 7px 10px;
                    border-radius: 999px;
                    background: rgba(79, 140, 255, .12);
                }

                .reports-bar a:hover { background: rgba(79, 140, 255, .22); }

                .game-cell {
                    display: flex;
                    align-items: center;
                    gap: 12px;
                    min-width: 240px;
                }

                .cover {
                    width: 96px;
                    height: 54px;
                    flex: 0 0 96px;
                    border-radius: 12px;
                    object-fit: contain;
                    background: #050a16;
                    border: 1px solid rgba(255, 255, 255, .16);
                    box-shadow: 0 12px 22px rgba(0, 0, 0, .28);
                }

                .cover-placeholder {
                    display: grid;
                    place-items: center;
                    background: linear-gradient(135deg, #1d4ed8, #7c3aed);
                    color: white;
                    font-weight: 950;
                    font-size: .95rem;
                    letter-spacing: -.04em;
                }

                .cover-fallback { display: none; }

                .game-title {
                    color: #ffffff;
                    font-weight: 850;
                }

                .table-wrap {
                    overflow-x: auto;
                    border: 1px solid rgba(148, 163, 184, .18);
                    border-radius: 18px;
                    box-shadow: 0 18px 48px rgba(0, 0, 0, .2);
                }

                table {
                    width: 100%;
                    border-collapse: collapse;
                    background: rgba(15, 23, 42, .82);
                    min-width: 720px;
                }

                th, td {
                    padding: 14px 16px;
                    border-bottom: 1px solid rgba(148, 163, 184, .14);
                    text-align: left;
                }

                th {
                    background: rgba(30, 41, 59, .92);
                    color: #ffffff;
                    font-size: .82rem;
                    text-transform: uppercase;
                    letter-spacing: .06em;
                }

                tr:hover td { background: rgba(79, 140, 255, .07); }

                input, select, textarea {
                    width: 100%;
                    margin-top: 8px;
                    padding: 12px 13px;
                    border-radius: 13px;
                    border: 1px solid rgba(148, 163, 184, .24);
                    background: rgba(5, 10, 22, .76);
                    color: white;
                    outline: none;
                }

                input:focus, select:focus, textarea:focus {
                    border-color: #60a5fa;
                    box-shadow: 0 0 0 4px rgba(96, 165, 250, .12);
                }

                textarea { min-height: 120px; resize: vertical; }

                label {
                    color: #dbeafe;
                    font-weight: 800;
                }

                .form {
                    display: grid;
                    gap: 17px;
                    max-width: 620px;
                    padding: 24px;
                    background: rgba(17, 27, 49, .78);
                    border: 1px solid rgba(148, 163, 184, .18);
                    border-radius: 22px;
                }

                .inline-form {
                    display: flex;
                    gap: 14px;
                    align-items: end;
                    flex-wrap: wrap;
                    margin: 20px 0;
                    padding: 16px;
                    background: rgba(17, 27, 49, .62);
                    border: 1px solid rgba(148, 163, 184, .16);
                    border-radius: 18px;
                }

                .inline-form label { min-width: 190px; }
                code { color: #93c5fd; background: rgba(147, 197, 253, .1); padding: 2px 6px; border-radius: 7px; }

                @media (max-width: 760px) {
                    header { align-items: flex-start; }
                    nav { gap: 4px; }
                    nav a { font-size: .84rem; padding: 7px 8px; }
                    main { padding-top: 24px; }
                    .hero { grid-template-columns: 1fr; border-radius: 22px; }
                    .hero-panel { min-height: 150px; }
                    .panel-number { font-size: 3.5rem; }
                    .form { padding: 18px; }
                }
            </style>
        </head>
        <body>
            <header>
                <a href="/">Biblioteca de Jogos</a>
                <nav>
                    <a href="/jogos">Jogos</a>
                    <a href="/jogos/novo">Cadastrar</a>
                    <a href="/avaliacoes/nova">Avaliar</a>
                    <a href="/consultas/jogos-avaliacoes">Jogos avaliados</a>
                    <a href="/consultas/media-genero">Notas por genero</a>
                    <a href="/consultas/generos-bem-avaliados">Destaques</a>
                </nav>
            </header>
            <main>{{conteudo}}</main>
        </body>
        </html>
        """;

    return Results.Content(pagina, "text/html; charset=utf-8");
}

static string Escape(string valor)
{
    return WebUtility.HtmlEncode(valor);
}

static string JogoComCapa(string titulo)
{
    var capa = CapaDoJogo(titulo);
    var tituloSeguro = Escape(titulo);

    if (!string.IsNullOrWhiteSpace(capa))
    {
        return $"""
            <div class="game-cell">
                <img class="cover" src="{capa}" alt="Capa de {tituloSeguro}" loading="lazy" onerror="this.style.display='none';this.nextElementSibling.style.display='grid';">
                <span class="cover cover-placeholder cover-fallback">{Iniciais(titulo)}</span>
                <span class="game-title">{tituloSeguro}</span>
            </div>
            """;
    }

    return $"""
        <div class="game-cell">
            <span class="cover cover-placeholder">{Iniciais(titulo)}</span>
            <span class="game-title">{tituloSeguro}</span>
        </div>
        """;
}

static string CapaDoJogo(string titulo)
{
    return titulo.Trim().ToLowerInvariant() switch
    {
        "elden ring" => "https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/1245620/header.jpg",
        "the last of us ii" => "https://image.api.playstation.com/vulcan/ap/rnd/202312/0117/315718bce7eed62e3cf3fb02d61b81ff1782d6b6cf850fa4.png",
        "hollow knight" => "https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/367520/header.jpg",
        "celeste" => "https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/504230/header.jpg",
        "god of war ragnarok" => "https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/2322010/header.jpg",
        "hades" => "https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/1145360/header.jpg",
        "cs2" => "https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/730/header.jpg",
        _ => ""
    };
}

static string Iniciais(string titulo)
{
    var partes = titulo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var iniciais = string.Concat(partes.Take(2).Select(p => char.ToUpperInvariant(p[0])));
    return Escape(string.IsNullOrWhiteSpace(iniciais) ? "?" : iniciais);
}

static int ParseInt(string valor)
{
    return int.TryParse(valor, out var numero) ? numero : 0;
}

static double ParseDouble(string valor)
{
    return double.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var numero)
        || double.TryParse(valor, NumberStyles.Number, CultureInfo.CurrentCulture, out numero)
        ? numero
        : 0;
}

static decimal ParseDecimal(string valor)
{
    return decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out var numero)
        || decimal.TryParse(valor, NumberStyles.Number, CultureInfo.CurrentCulture, out numero)
        ? numero
        : 0;
}

static void EnsureSchema(BibliotecaContext context)
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

static List<Tag> ObterTags(BibliotecaContext context, string texto)
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

static void SeedDados(BibliotecaContext context)
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

static void SeedTags(BibliotecaContext context)
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

static void AtribuirTags(Jogo jogo, List<Tag> tags)
{
    foreach (var tag in tags)
    {
        if (!jogo.Tags.Any(t => string.Equals(t.Nome, tag.Nome, StringComparison.OrdinalIgnoreCase)))
        {
            jogo.Tags.Add(tag);
        }
    }
}
