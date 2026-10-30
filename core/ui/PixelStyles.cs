using Godot;

/// <summary>
/// Fábrica de estilos "pixel art limpio" del proyecto.
/// Fuente única de valores: <c>docs/art_direction.md</c>
/// (plantilla <c>docs/word_search/wathersoup.jpg</c>).
/// Los contrastes se verifican en <c>tests/unit/PixelStylesTest.cs</c>.
/// Equivalente C# de <c>core/ui/PixelStyles.gd</c>.
/// </summary>
public static class PixelStyles
{
    // --- Paleta base ---------------------------------------------------------
    public static readonly Color Arena = new("#b5ad96"); // Fondo general de pantallas
    public static readonly Color Celda = new("#fbf3da"); // Relleno de celdas del tablero
    public static readonly Color Placa = new("#ead9bb"); // Interior de placas y modales
    public static readonly Color Tinta = new("#101010"); // Letras y texto sobre claros
    public static readonly Color Borde = new("#535456"); // Borde visible de placas, celdas y botones
    public static readonly Color Sombra = new("#8f8874"); // Sombra dura decorativa (no delimita)

    // --- Acentos -------------------------------------------------------------
    public static readonly Color Purpura = new("#9944c6");
    public static readonly Color PurpuraPresionado = new("#8a3cb4");
    public static readonly Color Amarillo = new("#fdc109");
    public static readonly Color Turquesa = new("#099c92");
    public static readonly Color Cian = new("#08d0df");

    // --- Estados del tablero -------------------------------------------------
    public static readonly Color CeldaSeleccion = new("#6095fb");
    public static readonly Color Trazo = new("#2b56b8");
    public static readonly Color EncontradaBorde = new("#04544d");
    public static readonly Color PistaBorde = new("#8a4600");
    public static readonly Color PistaAcento = new("#f9881e");

    // --- Textura de esquina --------------------------------------------------
    public const int Radius = 5; // Radio de esquina (plantilla 8 px ÷ 1,594)

    /// <summary>Contraste WCAG 2.1 entre dos colores (1.0 – 21.0).</summary>
    public static float ContrastRatio(Color a, Color b)
    {
        float light = Mathf.Max(Luminance(a), Luminance(b));
        float dark = Mathf.Min(Luminance(a), Luminance(b));
        return (light + 0.05f) / (dark + 0.05f);
    }

    /// <summary>Color de texto legible sobre un relleno: blanco o tinta.</summary>
    public static Color TextColor(Color fill)
    {
        return ContrastRatio(Colors.White, fill) >= ContrastRatio(Tinta, fill)
            ? Colors.White
            : Tinta;
    }

    /// <summary>Celda en reposo: crema con borde oscuro y sombra dura.</summary>
    public static StyleBoxFlat CellNormal() => MakeBox(Celda, Borde, 2, Radius);

    /// <summary>Celda seleccionada (trazo en curso).</summary>
    public static StyleBoxFlat CellSelected() => MakeBox(CeldaSeleccion, Borde, 2, Radius);

    /// <summary>Celda encontrada: relleno turquesa oscuro con letras crema en negrita.</summary>
    public static StyleBoxFlat CellFound() => MakeBox(EncontradaBorde, Celda, 3, Radius, false);

    /// <summary>Celda resaltada por pista: borde naranja oscuro con pulso.</summary>
    public static StyleBoxFlat CellHint() => MakeBox(Celda, PistaBorde, 3, Radius, false);

    /// <summary>Placa de encabezado o modal.</summary>
    public static StyleBoxFlat Plate() => MakeBox(Placa, Borde, 2, Radius);

    /// <summary>Píldora de categoría.</summary>
    public static StyleBoxFlat Pill() => MakeBox(Purpura, Tinta, 2, 999, false);

    /// <summary>Botón de 56×56 con el color de familia indicado.</summary>
    public static StyleBoxFlat Button(Color? fill = null)
    {
        StyleBoxFlat box = MakeBox(fill ?? Purpura, Tinta, 2, Radius);
        box.ContentMarginLeft = 16.0f;
        box.ContentMarginRight = 16.0f;
        box.ContentMarginTop = 12.0f;
        box.ContentMarginBottom = 12.0f;
        return box;
    }

    /// <summary>Estado presionado: sin sombra y borde más grueso (cambio de forma).</summary>
    public static StyleBoxFlat ButtonPressed(Color? fill = null)
    {
        StyleBoxFlat box = Button(fill);
        box.SetBorderWidthAll(3);
        box.ShadowSize = 0;
        box.ShadowColor = new Color(0, 0, 0, 0);
        return box;
    }

    /// <summary>
    /// Estado foco: anillo exterior oscuro de 3 px dibujado fuera del botón
    /// (sin relleno). Cambia la forma, no solo el color.
    /// </summary>
    public static StyleBoxFlat ButtonFocus()
    {
        StyleBoxFlat ring = new()
        {
            BgColor = new Color(0, 0, 0, 0),
            BorderColor = Tinta,
            DrawCenter = false,
            AntiAliasing = false,
            CornerDetail = 2,
        };
        ring.SetBorderWidthAll(3);
        ring.SetCornerRadiusAll(Radius + 3);
        ring.SetExpandMarginAll(3);
        return ring;
    }

    /// <summary>Estado deshabilitado: plano con borde, sin sombra.</summary>
    public static StyleBoxFlat ButtonDisabled() => MakeBox(Arena, Borde, 2, Radius, false);

    private static StyleBoxFlat MakeBox(Color bg, Color border, int borderWidth, int radius, bool shadow = true)
    {
        StyleBoxFlat box = new()
        {
            BgColor = bg,
            BorderColor = border,
            AntiAliasing = false,
            CornerDetail = 2,
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
        return (0.2126f * Channel(color.R)) + (0.7152f * Channel(color.G)) + (0.0722f * Channel(color.B));
    }

    private static float Channel(float value)
    {
        return value <= 0.03928f ? value / 12.92f : Mathf.Pow((value + 0.055f) / 1.055f, 2.4f);
    }
}
