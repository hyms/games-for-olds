using Godot;
using System;
using System.Collections.Generic;

/// <summary>
///     Fábrica de estilos "pixel art limpio" del proyecto.
///     Fuente única de valores: <c>docs/art_direction.md</c>
///     (plantilla <c>docs/word_search/wathersoup.jpg</c>).
///     Los contrastes se verifican en <c>tests/unit/PixelStylesTest.cs</c>.
///     Equivalente C# de <c>core/ui/PixelStyles.gd</c>.
/// </summary>
public static class PixelStyles
{
    // --- Textura de esquina --------------------------------------------------
    public const int Radius = 0; // Esquinas cuadradas: regla de oro pixel art (spec §2)

    // --- Paleta base ---------------------------------------------------------
    public static readonly Color Fondo = new("#EFEFEE"); // Fondo global limpio (lineamiento UI)
    public static readonly Color Arena = new("#A0BFC7"); // Borde fino de celdas y fondos suaves
    public static readonly Color Celda = new("#fffcfb"); // Relleno claro de celdas y texto sobre oscuros
    public static readonly Color CeldaIdle = new("#EBF2F5"); // Fondo de celda en reposo (spec §2.1)
    public static readonly Color Placa = new("#efefee"); // Interior de placas y modales
    public static readonly Color PlacaHeader = new("#a0bfc7"); // Panel de título (lineamiento UI)
    public static readonly Color Tinta = new("#477987"); // Letras y texto sobre claros
    public static readonly Color Borde = new("#477987"); // Borde visible de placas, celdas y botones
    public static readonly Color Sombra = new("#8a9aaa"); // Sombra dura decorativa
    public static readonly Color TextDark = new("#101010"); // Tinta de texto oscuro sobre claros
    public static readonly Color TextLight = new("#ffffff"); // Texto blanco puro sobre oscuros
    public static readonly Color BiselClaro = new("#ffffff"); // Luz del bisel 3D (arriba/izquierda)
    public static readonly Color BiselOscuro = new("#535456"); // Sombra del bisel 3D (abajo/derecha)

    // --- Medidas compartidas de botones --------------------------------------
    /// <summary>Lado mostrado de los iconos de botón (32 px de arte = 2× exacto).</summary>
    public const int ButtonIconSize = 16;

    // --- Acentos -------------------------------------------------------------
    public static readonly Color Turquesa = new("#099c92"); // Acento de celdas seleccionadas/encontradas (matriz global, conservada)
    public static readonly Color Cian = new("#08d0df"); // Acento secundario (matriz global, conservada)
    public static readonly Color Amarillo = new("#fdc109"); // Acento cálido (confeti/celebración)
    public static readonly Color CelebrationRed = new("#b45252"); // Coral cálido de confeti/celebración (token agregado v0.5.0)
    public static readonly Color CreamYellow = new("#ede19e"); // Banda de dificultad cálida (extensión aditiva v0.5.1)

    // --- Tokens locales de Word Search: madera/tierra (art_direction.md rev. 1.1) ---
    /// <summary>WS-Fondo: fondo madera de escena y relleno de botones HUD.</summary>
    public static readonly Color WSFondo = new("#a17a5b");
    /// <summary>WS-MaderaBorde: borde visible de botones, marco del tablero y del footer.</summary>
    public static readonly Color WSMaderaBorde = new("#3a2a1e");
    /// <summary>WS-Crema: placa de dificultad (banda de nivel).</summary>
    public static readonly Color WSCrema = new("#ede19e");
    /// <summary>WS-CremaBorde: borde de la placa de dificultad.</summary>
    public static readonly Color WSCremaBorde = new("#c4b476");
    /// <summary>WS-CremaSuave: %TimeBox y tarjeta del footer.</summary>
    public static readonly Color WSCremaSuave = new("#f1efe2");
    /// <summary>WS-Celda: relleno de celda en reposo.</summary>
    public static readonly Color WSCelda = new("#f1ede1");
    /// <summary>WS-Acento / WS-Encontrado: estado encontrado/selección (coral sólido).</summary>
    public static readonly Color WSAcento = new("#fd6d52");
    /// <summary>WS-EncontradoBorde: borde/contorno de celda encontrada.</summary>
    public static readonly Color WSEncontradoBorde = new("#8e3b33");
    /// <summary>WS-CeldaAviso: variante de celda en foco/hover/hint suave.</summary>
    public static readonly Color WSCeldaAviso = new("#b07259");
    /// <summary>WS-CianTitulo: título sobre fondo madera (requiere contorno WS-MaderaSombra).</summary>
    public static readonly Color WSCianTitulo = new("#65b9c4");
    /// <summary>WS-MaderaSombra: contorno/sombra dura del título y separadores.</summary>
    public static readonly Color WSMaderaSombra = new("#2e1d12");
    /// <summary>WS-Tachado: tachado + texto de palabra encontrada en el footer.</summary>
    public static readonly Color WSTachado = new("#9b4e48");

    // --- Estados del tablero -------------------------------------------------
    public static readonly Color Trazo = new("#2b56b8");

    public static readonly Color
        EncontradaBorde = new("#06706a"); // Acento turquesa oscurecido (contraste 3:1 sobre arena)

    public static readonly Color PistaBorde = new("#8a4600");
    public static readonly Color PistaAcento = new("#f9881e");

    /// <summary>Contraste WCAG 2.1 entre dos colores (1.0 – 21.0).</summary>
    public static float ContrastRatio(Color a, Color b)
    {
        var light = Mathf.Max(Luminance(a), Luminance(b));
        var dark = Mathf.Min(Luminance(a), Luminance(b));
        return (light + 0.05f) / (dark + 0.05f);
    }

    /// <summary>Fuente PixelOperator para celdas del tablero (spec §1.3).</summary>
    public static Font CellFont { get; private set; }

    /// <summary>Fuente VCR OSD Mono para HUD, cronómetro y acciones (spec §1.2).</summary>
    public static Font HudFont { get; private set; }

    /// <summary>Fuente Atkinson Hyperlegible para menús y textos accesibles.</summary>
    public static Font UiFont { get; private set; }

    /// <summary>Fuente PressStart2P para botones HUD con peso gráfico de display (art_direction §2.1).</summary>
    public static Font DisplayFont { get; private set; }

    static PixelStyles()
    {
        CellFont = LoadFont("res://assets/fonts/PixelOperator.ttf");
        HudFont = LoadFont("res://assets/fonts/VCR_OSD_Mono.ttf");
        UiFont = LoadFont("res://assets/fonts/AtkinsonHyperlegible-Regular.ttf");
        DisplayFont = LoadFont("res://assets/fonts/PressStart2P.ttf");
    }

    private static Font LoadFont(string path)
    {
        return ResourceLoader.Exists(path) ? GD.Load<Font>(path) : null;
    }

    private static readonly Dictionary<string, StyleBoxFlat> _styleCache = new();

    private static StyleBoxFlat Cached(string themeId, string key, Func<StyleBoxFlat> build)
    {
        var cacheKey = $"{themeId}:{key}";
        if (!_styleCache.TryGetValue(cacheKey, out var box))
        {
            box = build();
            _styleCache[cacheKey] = box;
        }
        return box;
    }

    /// <summary>Color de texto legible sobre un relleno: blanco, tinta o tinta oscura.</summary>
    public static Color TextColor(Color fill)
    {
        var best = TextLight;
        var bestRatio = ContrastRatio(best, fill);

        foreach (var candidate in new[] { Tinta, TextDark })
        {
            var ratio = ContrastRatio(candidate, fill);
            if (ratio > bestRatio)
            {
                best = candidate;
                bestRatio = ratio;
            }
        }

        return best;
    }

    /// <summary>Borde legible sobre un relleno: tinta si alcanza 3:1, si no un tono oscuro o claro según el fondo.</summary>
    public static Color BorderColor(Color fill)
    {
        if (ContrastRatio(Tinta, fill) >= 3.0f) return Tinta;
        return ContrastRatio(TextDark, fill) >= 3.0f ? TextDark : TextLight;
    }

    /// <summary>Placa de encabezado, cronómetro o modal: crema suave con borde madera 2px y padding interno (Word Search, rev. 1.1). Devuelto desde caché flyweight por themeId; no mutar.</summary>
    public static StyleBoxFlat Plate(string themeId = "classic") => Cached(themeId, nameof(Plate), BuildPlate);

    private static StyleBoxFlat BuildPlate()
    {
        var box = MakeBox(WSCremaSuave, WSMaderaBorde, 2, Radius);
        box.ContentMarginLeft = 12.0f;
        box.ContentMarginRight = 12.0f;
        box.ContentMarginTop = 4.0f;
        box.ContentMarginBottom = 4.0f;
        return box;
    }

    /// <summary>Panel del título principal: fondo madera WS-Fondo con borde madera, borde pixel 2px (sin placa fría). Devuelto desde caché flyweight por themeId; no mutar.</summary>
    public static StyleBoxFlat TitlePanel(string themeId = "classic") => Cached(themeId, nameof(TitlePanel), BuildTitlePanel);

    private static StyleBoxFlat BuildTitlePanel()
    {
        var box = new StyleBoxFlat
        {
            BgColor = Opaque(PlacaHeader),
            BorderColor = Opaque(Borde),
            AntiAliasing = false,
            CornerDetail = 2
        };
        box.SetBorderWidthAll(2);
        // 1px top highlight: reduce top border width for "lit-from-above" effect
        box.BorderWidthTop = 1;
        // Hard retro drop shadow: 2px offset down-right in dark teal
        box.ShadowSize = 2;
        box.ShadowColor = Opaque(Tinta);
        box.ShadowOffset = new Vector2(2, 2);
        // Generous content margins for the taller 72px banner
        box.ContentMarginLeft = 16.0f;
        box.ContentMarginRight = 16.0f;
        box.ContentMarginTop = 10.0f;
        box.ContentMarginBottom = 10.0f;
        return box;
    }

    /// <summary>Panel del título de Word Search: fondo madera WS-Fondo sobre el lienzo de escena (sin placa fría). Devuelto desde caché flyweight por themeId; no mutar.</summary>
    public static StyleBoxFlat WordSearchTitlePanel(string themeId = "classic") => Cached(themeId, nameof(WordSearchTitlePanel), BuildWordSearchTitlePanel);

    private static StyleBoxFlat BuildWordSearchTitlePanel()
    {
        var box = MakeBox(WSFondo, WSMaderaBorde, 2, Radius);
        box.ContentMarginLeft = 16.0f;
        box.ContentMarginRight = 16.0f;
        box.ContentMarginTop = 10.0f;
        box.ContentMarginBottom = 10.0f;
        return box;
    }

    /// <summary>
    ///     Banda de dificultad: acento cálido <c>CreamYellow</c> (#EDE19E == WS-Crema) con borde pixel 2px,
    ///     según la referencia wathersoup. Texto oscuro <c>TextDark</c> encima (14.4:1).
    ///     Borde WS-CremaBorde (rev. 1.1) en lugar de Borde frío. Devuelto desde caché flyweight por themeId; no mutar.
    /// </summary>
    public static StyleBoxFlat SubtitlePanel(string themeId = "classic") => Cached(themeId, nameof(SubtitlePanel), BuildSubtitlePanel);

    private static StyleBoxFlat BuildSubtitlePanel()
    {
        var box = new StyleBoxFlat
        {
            BgColor = Opaque(CreamYellow),
            BorderColor = Opaque(WSCremaBorde),
            AntiAliasing = false,
            CornerDetail = 1
        };
        box.SetBorderWidthAll(2);
        box.SetCornerRadiusAll(0);
        box.ContentMarginLeft = 18.0f;
        box.ContentMarginRight = 18.0f;
        box.ContentMarginTop = 2.0f;
        box.ContentMarginBottom = 2.0f;
        return box;
    }

    /// <summary>Marco del tablero: fondo madera WS-Fondo con borde WS-MaderaBorde de 3 px, plano (sin sombra). Devuelto desde caché flyweight por themeId; no mutar.</summary>
    public static StyleBoxFlat BoardFrame(string themeId = "classic") => Cached(themeId, nameof(BoardFrame), BuildBoardFrame);

    private static StyleBoxFlat BuildBoardFrame()
    {
        var box = MakeBox(WSFondo, WSMaderaBorde, 3, Radius, false);
        box.ContentMarginLeft = 10.0f;
        box.ContentMarginRight = 10.0f;
        box.ContentMarginTop = 10.0f;
        box.ContentMarginBottom = 10.0f;
        return box;
    }

    /// <summary>Panel inferior de palabras: crema suave WS-CremaSuave con marco madera WS-MaderaBorde de 3px. Devuelto desde caché flyweight por themeId; no mutar.</summary>
    public static StyleBoxFlat WordPanel(string themeId = "classic") => Cached(themeId, nameof(WordPanel), BuildWordPanel);

    private static StyleBoxFlat BuildWordPanel()
    {
        var box = MakeBox(WSCremaSuave, WSMaderaBorde, 3, Radius);
        box.ContentMarginLeft = 16.0f;
        box.ContentMarginRight = 16.0f;
        box.ContentMarginTop = 12.0f;
        box.ContentMarginBottom = 12.0f;
        return box;
    }

    /// <summary>Fondo del modal: panel sólido que bloquea el tablero (sin alfa reducido, spec §2). Devuelto desde caché flyweight por themeId; no mutar.</summary>
    public static StyleBoxFlat ModalBackdrop(string themeId = "classic") => Cached(themeId, nameof(ModalBackdrop), () => MakeBox(TextDark, Borde, 0, 0, false));

    /// <summary>Tarjeta interior de un modal. Devuelto desde caché flyweight por themeId; no mutar.</summary>
    public static StyleBoxFlat ModalCard(string themeId = "classic") => Cached(themeId, nameof(ModalCard), BuildModalCard);

    private static StyleBoxFlat BuildModalCard()
    {
        var box = MakeBox(Placa, Tinta, 2, Radius);
        box.ContentMarginLeft = 18.0f;
        box.ContentMarginRight = 18.0f;
        box.ContentMarginTop = 16.0f;
        box.ContentMarginBottom = 16.0f;
        return box;
    }

    /// <summary>Píldora de categoría. Devuelto desde caché flyweight por themeId; no mutar.</summary>
    public static StyleBoxFlat Pill(string themeId = "classic") => Cached(themeId, nameof(Pill), () => MakeBox(Arena, BorderColor(Arena), 2, 999, false));

    private static StyleBoxFlat MakeBox(Color bg, Color border, int borderWidth, int radius, bool shadow = true)
    {
        StyleBoxFlat box = new()
        {
            BgColor = Opaque(bg),
            BorderColor = Opaque(border),
            AntiAliasing = false,
            CornerDetail = 2
        };
        box.SetBorderWidthAll(borderWidth);
        box.SetCornerRadiusAll(radius);
        if (shadow)
        {
            box.ShadowSize = 2;
            box.ShadowColor = Opaque(Sombra);
            box.ShadowOffset = new Vector2(2, 2);
        }

        return box;
    }

    /// <summary>Fuerza opacidad total (alpha = 1.0): regla pixel art sin transparencias.</summary>
    public static Color Opaque(Color color) => new(color.R, color.G, color.B, 1f);

    private static float Luminance(Color color)
    {
        return 0.2126f * Channel(color.R) + 0.7152f * Channel(color.G) + 0.0722f * Channel(color.B);
    }

    private static float Channel(float value)
    {
        return value <= 0.03928f ? value / 12.92f : Mathf.Pow((value + 0.055f) / 1.055f, 2.4f);
    }
}