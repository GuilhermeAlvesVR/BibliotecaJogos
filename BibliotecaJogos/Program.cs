using System.Globalization;
using BibliotecaJogos.Data;
using BibliotecaJogos.Models;
using Microsoft.EntityFrameworkCore;
using static BibliotecaJogos.Data.DatabaseSetup;
using static BibliotecaJogos.Views.HtmlPage;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

using (var context = new BibliotecaContext())
{
    Initialize(context);
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
