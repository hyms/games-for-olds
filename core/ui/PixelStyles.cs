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
    public static readonly Color Placa = new("#efefee"); // Interior de placas y modales
    public static readonly Color PlacaHeader = new("#a0bfc7"); // Panel de título (lineamiento UI)
    public static readonly Color Tinta = new("#4e6077"); // Letras y texto sobre claros
    public static readonly Color Borde = new("#4e6077"); // Borde visible de placas, celdas y botones
    public static readonly Color Sombra = new("#8a9aaa"); // Sombra dura decorativa
    public static readonly Color TextDark = new("#101010"); // Tinta de texto oscuro sobre claros
    public static readonly Color TextLight = new("#ffffff"); // Texto blanco puro sobre oscuros

    // --- Acentos -------------------------------------------------------------
    public static readonly Color Amarillo = new("#fdc109");
    public static readonly Color Turquesa = new("#099c92"); // Acento de celdas seleccionadas/encontradas
    public static readonly Color Cian = new("#08d0df");

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

    /// <summary>Borde legible sobre un relleno: tinta si alcanza 3:1, si no tinta oscura.</summary>
    public static Color BorderColor(Color fill)
    {
        return ContrastRatio(Tinta, fill) >= 3.0f ? Tinta : TextDark;
    }

    /// <summary>Celda en reposo: fondo claro con borde fino azul grisáceo y sombra dura.</summary>
    public static StyleBoxFlat CellNormal()
    {
        return MakeBox(Celda, Arena, 2, Radius);
    }

    /// <summary>Celda seleccionada (trazo en curso): acento turquesa con letras blancas.</summary>
    public static StyleBoxFlat CellSelected()
    {
        return MakeBox(Turquesa, Tinta, 2, Radius, false);
    }

    /// <summary>Celda encontrada: acento turquesa con letras blancas y borde definido.</summary>
    public static StyleBoxFlat CellFound()
    {
        return MakeBox(Turquesa, EncontradaBorde, 3, Radius, false);
    }

    /// <summary>Celda resaltada por pista: fondo naranja retro, borde naranja oscuro (spec §4.2).</summary>
    public static StyleBoxFlat CellHint()
    {
        return MakeBox(PistaAcento, PistaBorde, 3, Radius, false);
    }

    /// <summary>Placa de encabezado o modal.</summary>
    public static StyleBoxFlat Plate()
    {
        return MakeBox(Placa, Borde, 2, Radius);
    }

    /// <summary>Panel del título principal: fondo azul claro, borde de pixel art y sombra dura.</summary>
    public static StyleBoxFlat TitlePanel()
    {
        var box = MakeBox(PlacaHeader, Tinta, 3, Radius);
        box.ContentMarginLeft = 12.0f;
        box.ContentMarginRight = 12.0f;
        box.ContentMarginTop = 8.0f;
        box.ContentMarginBottom = 8.0f;
        return box;
    }

    /// <summary>Panel de subtítulo secundario: fondo turquesa medio, borde píxel 2px y esquinas cuadradas.</summary>
    public static StyleBoxFlat SubtitlePanel()
    {
        var box = new StyleBoxFlat
        {
            BgColor = Color.FromHtml("#6C95A1"),
            BorderColor = Color.FromHtml("#A0BFC7"),
            AntiAliasing = false,
            CornerDetail = 1
        };
        box.SetBorderWidthAll(2);
        box.SetCornerRadiusAll(0);
        box.ContentMarginLeft = 12.0f;
        box.ContentMarginRight = 12.0f;
        box.ContentMarginTop = 2.0f;
        box.ContentMarginBottom = 2.0f;
        return box;
    }

    /// <summary>Panel inferior de palabras: placa ancha con marco de pixel art.</summary>
    public static StyleBoxFlat WordPanel()
    {
        var box = MakeBox(Placa, Tinta, 2, Radius);
        box.ContentMarginLeft = 10.0f;
        box.ContentMarginRight = 10.0f;
        box.ContentMarginTop = 6.0f;
        box.ContentMarginBottom = 6.0f;
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

    /// <summary>Botón de 56×56 con el color de familia indicado.</summary>
    public static StyleBoxFlat Button(Color? fill = null)
    {
        var background = fill ?? Arena;
        var box = MakeBox(background, BorderColor(background), 2, Radius);
        box.ContentMarginLeft = 16.0f;
        box.ContentMarginRight = 16.0f;
        box.ContentMarginTop = 12.0f;
        box.ContentMarginBottom = 12.0f;
        return box;
    }

    /// <summary>Estado presionado: sin sombra y borde más grueso (cambio de forma).</summary>
    public static StyleBoxFlat ButtonPressed(Color? fill = null)
    {
        var box = Button(fill);
        box.SetBorderWidthAll(3);
        box.ShadowSize = 0;
        return box;
    }

    /// <summary>
    ///     Estado foco: anillo exterior oscuro de 3 px dibujado fuera del botón
    ///     (sin relleno). Cambia la forma, no solo el color.
    /// </summary>
    public static StyleBoxFlat ButtonFocus()
    {
        StyleBoxFlat ring = new()
        {
            BgColor = Placa,
            BorderColor = Tinta,
            DrawCenter = false,
            AntiAliasing = false,
            CornerDetail = 2
        };
        ring.SetBorderWidthAll(3);
        ring.SetCornerRadiusAll(Radius + 3);
        ring.SetExpandMarginAll(3);
        return ring;
    }

    /// <summary>Estado deshabilitado: plano con borde, sin sombra.</summary>
    public static StyleBoxFlat ButtonDisabled()
    {
        return MakeBox(Arena, Borde, 2, Radius, false);
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