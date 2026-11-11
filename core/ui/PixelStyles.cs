using Godot;

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
    public static readonly Color Fondo = new("#ffffff"); // Fondo global limpio (lineamiento UI)
    public static readonly Color Arena = new("#b0c2cc"); // Borde fino de celdas y fondos suaves
    public static readonly Color Celda = new("#fffcfb"); // Relleno claro de celdas y texto sobre oscuros
    public static readonly Color CeldaIdle = new("#EBF2F5"); // Fondo de celda en reposo (spec §2.1)
    public static readonly Color Placa = new("#efefee"); // Interior de placas y modales
    public static readonly Color PlacaHeader = new("#a0bfc7"); // Panel de título (lineamiento UI)
    public static readonly Color Tinta = new("#4e6077"); // Letras y texto sobre claros
    public static readonly Color Borde = new("#4e6077"); // Borde visible de placas, celdas y botones
    public static readonly Color Sombra = new("#8a9aaa"); // Sombra dura decorativa
    public static readonly Color TextDark = new("#101010"); // Tinta de texto oscuro sobre claros
    public static readonly Color TextLight = new("#ffffff"); // Texto blanco puro sobre oscuros
    public static readonly Color BiselClaro = new("#ffffff"); // Luz del bisel 3D (arriba/izquierda)
    public static readonly Color BiselOscuro = new("#535456"); // Sombra del bisel 3D (abajo/derecha)

    // --- Medidas compartidas de botones --------------------------------------
    /// <summary>Lado mostrado de los iconos de botón (32 px de arte = 2× exacto).</summary>
    public const int ButtonIconSize = 16;

    // --- Colores de cabecera específicos (wathersoup.jpg) ---------------------
    public static readonly Color TitleFill = new("#6C929D");
    public static readonly Color TitleBorder = new("#68838B");
    public static readonly Color TitleOutline = new("#2B5666");
    public static readonly Color TitleTopHighlight = new("#B0C2C6");
    public static readonly Color TitleReliefMid = new("#94B3B8");
    public static readonly Color TitleReliefLight = new("#D3ECF0");
public static readonly Color Turquesa = new("#099c92"); // Acento de celdas seleccionadas/encontradas
    public static readonly Color Cian = new("#08d0df");
    public static readonly Color Amarillo = new("#fdc109"); // Acento cálido (confeti/celebración)
    public static readonly Color CelebrationRed = new("#b45252"); // Coral cálido de confeti/celebración (token agregado v0.5.0)

    // --- Estados del tablero -------------------------------------------------
    public static readonly Color CeldaSeleccion = new("#6095fb");
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

    static PixelStyles()
    {
        CellFont = LoadFont("res://assets/fonts/PixelOperator.ttf");
        HudFont = LoadFont("res://assets/fonts/VCR_OSD_Mono.ttf");
        UiFont = LoadFont("res://assets/fonts/AtkinsonHyperlegible-Regular.ttf");
    }

    private static Font LoadFont(string path)
    {
        return ResourceLoader.Exists(path) ? GD.Load<Font>(path) : null;
    }

    /// <summary>Color de texto legible sobre un relleno: blanco, tinta o tinta oscura.</summary>
    public static Color TextColor(Color fill)
    {
        var best = Colors.White;
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

    /// <summary>Placa de encabezado o modal.</summary>
    public static StyleBoxFlat Plate()
    {
        return MakeBox(Placa, Borde, 2, Radius);
    }

    /// <summary>Panel del título principal: banner teal medio (#6C929D, plantilla wathersoup.jpg), borde pixel 2px.</summary>
    public static StyleBoxFlat TitlePanel()
    {
        var box = new StyleBoxFlat
        {
            BgColor = TitleFill,
            BorderColor = TitleBorder,
            AntiAliasing = false,
            CornerDetail = 2
        };
        box.SetBorderWidthAll(2);
        // 1px top highlight: reduce top border width for "lit-from-above" effect
        box.BorderWidthTop = 1;
        // Hard retro drop shadow: 2px offset down-right in dark teal
        box.ShadowSize = 2;
        box.ShadowColor = Tinta;
        box.ShadowOffset = new Vector2(2, 2);
        // Generous content margins for the taller 72px banner
        box.ContentMarginLeft = 16.0f;
        box.ContentMarginRight = 16.0f;
        box.ContentMarginTop = 10.0f;
        box.ContentMarginBottom = 10.0f;
        return box;
    }

    /// <summary>Banda de subtítulo: fondo claro #D2E1E8 bajo el banner (plantilla wathersoup.jpg) con borde pixel 2px.</summary>
    public static StyleBoxFlat SubtitlePanel()
    {
        var box = new StyleBoxFlat
        {
            BgColor = Color.FromHtml("#B8D4DC"),
            BorderColor = TitleBorder,
            AntiAliasing = false,
            CornerDetail = 1
        };
        box.SetBorderWidthAll(2);
        box.SetCornerRadiusAll(0);
        box.ContentMarginLeft = 14.0f;
        box.ContentMarginRight = 14.0f;
        box.ContentMarginTop = 2.0f;
        box.ContentMarginBottom = 2.0f;
        return box;
    }

    /// <summary>Panel inferior de palabras: placa ancha con marco retro de pixel art de 2px.</summary>
    public static StyleBoxFlat WordPanel()
    {
        var box = MakeBox(Placa, Tinta, 2, Radius);
        box.ContentMarginLeft = 12.0f;
        box.ContentMarginRight = 12.0f;
        box.ContentMarginTop = 8.0f;
        box.ContentMarginBottom = 8.0f;
        return box;
    }

    /// <summary>Fondo del modal: panel sólido que bloquea el tablero (sin alfa reducido, spec §2).</summary>
    public static StyleBoxFlat ModalBackdrop()
    {
        return MakeBox(new Color(0.04f, 0.05f, 0.07f), Tinta, 0, 0, false);
    }

    /// <summary>Tarjeta interior de un modal.</summary>
    public static StyleBoxFlat ModalCard()
    {
        var box = MakeBox(Placa, Tinta, 2, Radius);
        box.ContentMarginLeft = 18.0f;
        box.ContentMarginRight = 18.0f;
        box.ContentMarginTop = 16.0f;
        box.ContentMarginBottom = 16.0f;
        return box;
    }

    /// <summary>Píldora de categoría.</summary>
    public static StyleBoxFlat Pill()
    {
        return MakeBox(Arena, BorderColor(Arena), 2, 999, false);
    }

    private static StyleBoxFlat MakeBox(Color bg, Color border, int borderWidth, int radius, bool shadow = true)
    {
        StyleBoxFlat box = new()
        {
            BgColor = bg,
            BorderColor = border,
            AntiAliasing = false,
            CornerDetail = 2
        };
        box.SetBorderWidthAll(borderWidth);
        box.SetCornerRadiusAll(radius);
        if (shadow)
        {
            box.ShadowSize = 2;
            box.ShadowColor = Sombra;
            box.ShadowOffset = new Vector2(2, 2);
        }

        return box;
    }

    private static float Luminance(Color color)
    {
        return 0.2126f * Channel(color.R) + 0.7152f * Channel(color.G) + 0.0722f * Channel(color.B);
    }

    private static float Channel(float value)
    {
        return value <= 0.03928f ? value / 12.92f : Mathf.Pow((value + 0.055f) / 1.055f, 2.4f);
    }
}