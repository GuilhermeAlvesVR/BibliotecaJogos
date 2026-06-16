using System.Net;
using Microsoft.AspNetCore.Http;

namespace BibliotecaJogos.Views
{
    public static class HtmlPage
    {
        public static IResult Html(string titulo, string conteudo)
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
                        .game-title { color: #ffffff; font-weight: 850; }

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
                        label { color: #dbeafe; font-weight: 800; }

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

        public static string Escape(string valor)
        {
            return WebUtility.HtmlEncode(valor);
        }

        public static string JogoComCapa(string titulo)
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

        private static string CapaDoJogo(string titulo)
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

        private static string Iniciais(string titulo)
        {
            var partes = titulo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var iniciais = string.Concat(partes.Take(2).Select(p => char.ToUpperInvariant(p[0])));
            return Escape(string.IsNullOrWhiteSpace(iniciais) ? "?" : iniciais);
        }
    }
}
