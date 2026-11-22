using ClasicosMenteActiva.core;
using Godot;

/// <summary>Contexto de uso de un botón; ajusta tipografía, tamaño y márgenes.</summary>
public enum PixelButtonVariant
{
    /// <summary>Barra superior del minijuego (Atrás, Pausa): VCR OSD Mono 16 px.</summary>
    Hud,

    /// <summary>Acciones de modales (Continuar, Pista, Reiniciar): VCR OSD Mono 18 px.</summary>
    Action,

    /// <summary>Menús principales y selector de dificultad: Atkinson 20 px para máxima legibilidad.</summary>
    Menu
}

/// <summary>Estado visual de un botón pixel art.</summary>
public enum PixelButtonState
{
    Normal,
    Hover,
    Pressed,
    Focus,
    Disabled
}

/// <summary>Alcance del botón: determina qué paleta de colores usar.</summary>
public enum ButtonScope
{
    Global,
    WordSearch
}

/// <summary>Posición del icono respecto al texto del botón.</summary>
public enum IconPosition
{
    Left,
    Right
}

/// <summary>Medidas tipográficas compartidas por todos los estados de una variante.</summary>
public readonly struct PixelButtonMetrics
{
    public Font Font { get; init; }
    public int FontSize { get; init; }
    public int MarginX { get; init; }
    public int MarginY { get; init; }
    public int HSeparation { get; init; }
    public Vector2 MinSize { get; init; }
    public HorizontalAlignment IconAlignment { get; init; }
    public int CornerRadius { get; init; }
}

/// <summary>Resultado inmutable para pintar un estado concreto de un botón.</summary>
public readonly struct PixelButtonVisual
{
    public PixelButtonMetrics Metrics { get; init; }
    public StyleBoxFlat Style { get; init; }
    public Color TextColor { get; init; }

    /// <summary>True si el bisel debe dibujarse elevado; false si debe hundirse.</summary>
    public bool Raised { get; init; }

    /// <summary>False solo en deshabilitado: sin relieve, botón plano.</summary>
    public bool DrawRelief { get; init; }

    /// <summary>Esquina del StyleBoxFlat (template de botón, no por estado).</summary>
    public int CornerRadius { get; init; }

    /// <summary>El icono se tiñe con el mismo color que el texto para mantener contraste.</summary>
    public Color IconColor => TextColor;
}

/// <summary>
///     Fábrica única de estilos de botón (factory method <see cref="For" />).
///     Centraliza la paleta, las medidas y los seis estados de <c>Button</c>, de modo
    ///     que HUD, modales, menús y selector de dificultad comparten las mismas reglas:
    ///     esquinas redondeadas 4 px (HUD Word Search: 8 px), borde de 2 px, bisel de 1 px, alfa 1.0 y desplazamiento de
    ///     2 px al pulsar (docs/art_direction.md §3 y §5.3).
/// </summary>
public static class PixelButtonStyle
{
    /// <summary>Píxeles que baja el contenido del botón en estado pulsado.</summary>
    public const int PressShift = 2;

    private static readonly PixelButtonVariant[] Variants =
    {
        PixelButtonVariant.Hud,
        PixelButtonVariant.Action,
        PixelButtonVariant.Menu
    };

    private static readonly PixelButtonState[] States =
    {
        PixelButtonState.Normal,
        PixelButtonState.Hover,
        PixelButtonState.Pressed,
        PixelButtonState.Focus,
        PixelButtonState.Disabled
    };

    /// <summary>Variantes disponibles; útil para tests y documentación.</summary>
    public static PixelButtonVariant[] AllVariants => Variants;

    /// <summary>Estados disponibles; útil para tests y documentación.</summary>
    public static PixelButtonState[] AllStates => States;

    /// <summary>Medidas tipográficas de una variante.</summary>
    public static PixelButtonMetrics MetricsFor(PixelButtonVariant variant)
    {
        return variant switch
        {
            PixelButtonVariant.Hud => new PixelButtonMetrics
            {
                Font = PixelStyles.DisplayFont,
                FontSize = 10,
                MarginX = 10,
                MarginY = 10,
                HSeparation = 6,
                MinSize = new Vector2(88, 50),
                IconAlignment = HorizontalAlignment.Center,
                CornerRadius = 8
            },
            PixelButtonVariant.Action => new PixelButtonMetrics
            {
                Font = PixelStyles.DisplayFont,
                FontSize = 11,
                MarginX = 14,
                MarginY = 12,
                HSeparation = 8,
                MinSize = new Vector2(0, 56),
                IconAlignment = HorizontalAlignment.Center,
                CornerRadius = 4
            },
            _ => new PixelButtonMetrics
            {
                Font = PixelStyles.UiFont,
                FontSize = 20,
                MarginX = 16,
                MarginY = 12,
                HSeparation = 8,
                MinSize = new Vector2(0, 56),
                IconAlignment = HorizontalAlignment.Center,
                CornerRadius = 4
            }
        };
    }

    /// <summary>
    ///     Construye el estilo de un estado. El estado <see cref="PixelButtonState.Pressed" />
    ///     conserva la altura total (top + 2 px, bottom − 2 px) y el resto de estados
    ///     comparten márgenes para que el botón no cambie de tamaño al interactuar.
    /// </summary>
    public static PixelButtonVisual For(PixelButtonVariant variant, PixelButtonState state, ThemeData theme, ButtonScope scope = ButtonScope.Global)
    {
        theme ??= new ThemeData();
        var metrics = MetricsFor(variant);
        var fill = FillFor(state, theme, scope);
        var border = BorderFor(state, theme, fill, scope);
        var isPressed = state == PixelButtonState.Pressed;

        var style = new StyleBoxFlat
        {
            BgColor = state == PixelButtonState.Focus
                ? new Color(0, 0, 0, 0)
                : PixelStyles.Opaque(fill),
            BorderColor = PixelStyles.Opaque(state == PixelButtonState.Focus
                ? PixelStyles.TextColor(theme.BackgroundColor)
                : border),
            AntiAliasing = false,
            CornerDetail = 1,
            DrawCenter = state != PixelButtonState.Focus
        };
        style.SetBorderWidthAll(PixelReliefPainter.BorderWidth);
        style.SetCornerRadiusAll(CornerRadiusFor(variant, scope));
        style.ContentMarginLeft = metrics.MarginX;
        style.ContentMarginRight = metrics.MarginX;
        style.ContentMarginTop = metrics.MarginY + (isPressed ? PressShift : 0);
        style.ContentMarginBottom = metrics.MarginY - (isPressed ? PressShift : 0);
        style.CornerDetail = CornerDetailFor(scope);

        style.ContentMarginTop = metrics.MarginY + (isPressed ? PressShift : 0);
        style.ContentMarginBottom = metrics.MarginY - (isPressed ? PressShift : 0);
        style.CornerDetail = CornerDetailFor(scope);

        if (state == PixelButtonState.Focus)
        {
            // El foco se pinta como anillo de 2 px pegado al borde (sin expand).
        }

        return new PixelButtonVisual
        {
            Metrics = metrics,
            Style = style,
            TextColor = TextFor(state, theme, scope),
            Raised = state is PixelButtonState.Normal or PixelButtonState.Focus,
            DrawRelief = state != PixelButtonState.Disabled,
            CornerRadius = CornerRadiusFor(variant, scope)
        };
    }

    /// <summary>Esquina del StyleBoxFlat: Word Search redondea a 8 px; el resto usa la métrica de la variante.</summary>
    private static int CornerRadiusFor(PixelButtonVariant variant, ButtonScope scope)
    {
        return scope == ButtonScope.WordSearch ? 8 : metricsCornerRadius(variant);
    }

    /// <summary>Corneado de detalle: Word Search 2 px, el resto 1 px.</summary>
    private static int CornerDetailFor(ButtonScope scope)
    {
        return scope == ButtonScope.WordSearch ? 2 : 1;
    }

    private static int metricsCornerRadius(PixelButtonVariant variant) => variant switch
    {
        PixelButtonVariant.Hud => 8,
        PixelButtonVariant.Action => 4,
        _ => 4
    };

    private static Color FillFor(PixelButtonState state, ThemeData theme, ButtonScope scope)
    {
        if (scope == ButtonScope.WordSearch)
        {
            return state switch
            {
                PixelButtonState.Hover => theme.WSCremaSuave,
                PixelButtonState.Pressed => theme.WSMaderaBorde.Darkened(0.2f),
                PixelButtonState.Disabled => theme.WSCrema,
                _ => theme.WSFondo
            };
        }

        return state switch
        {
            PixelButtonState.Hover => theme.SubheaderBannerColor,
            PixelButtonState.Pressed => theme.CellFoundBackgroundColor.Darkened(0.3f),
            PixelButtonState.Disabled => PixelStyles.Arena,
            _ => theme.HeaderBannerColor
        };
    }

    private static Color BorderFor(PixelButtonState state, ThemeData theme, Color fill, ButtonScope scope)
    {
        if (state == PixelButtonState.Focus)
        {
            return PixelStyles.TextColor(theme.BackgroundColor);
        }

        if (scope == ButtonScope.WordSearch)
        {
            return theme.WSMaderaBorde;
        }

        return PixelStyles.BorderColor(fill);
    }

    private static Color TextFor(PixelButtonState state, ThemeData theme, ButtonScope scope)
    {
        return state switch
        {
            PixelButtonState.Focus => PixelStyles.TextColor(theme.HeaderBannerColor),
            PixelButtonState.Disabled => PixelStyles.TextColor(PixelStyles.Arena),
            _ => PixelStyles.TextColor(FillFor(state, theme, scope))
        };
    }

    /// <summary>
    ///     Estilo atenuado para tarjetas secundarias (no activas) del carrusel:
    ///     relleno del estado base oscurecido, sin transparencia, colores de texto
    ///   y borde derivados del relleno para mantener contraste. Los estados
    ///     (normal/hover/pressed/focus/disabled) conservan su semántica de relieve.
    /// </summary>
    public static PixelButtonVisual MutedFor(PixelButtonVariant variant, PixelButtonState state, ThemeData theme, ButtonScope scope = ButtonScope.Global)
    {
        theme ??= new ThemeData();
        var metrics = MetricsFor(variant);
        var fill = PixelStyles.Opaque(FillFor(state, theme, scope).Darkened(0.35f));
        var isPressed = state == PixelButtonState.Pressed;

        var style = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = PixelStyles.Opaque(PixelStyles.BorderColor(fill)),
            AntiAliasing = false,
            CornerDetail = 1,
            DrawCenter = state != PixelButtonState.Focus,
        };
        style.SetBorderWidthAll(PixelReliefPainter.BorderWidth);
        style.SetCornerRadiusAll(CornerRadiusFor(variant, scope));
        style.ContentMarginLeft = metrics.MarginX;
        style.ContentMarginRight = metrics.MarginX;
        style.ContentMarginTop = metrics.MarginY + (isPressed ? PressShift : 0);
        style.ContentMarginBottom = metrics.MarginY - (isPressed ? PressShift : 0);

        return new PixelButtonVisual
        {
            Metrics = metrics,
            Style = style,
            TextColor = PixelStyles.TextColor(fill),
            Raised = state is PixelButtonState.Normal or PixelButtonState.Focus,
            DrawRelief = state != PixelButtonState.Disabled,
            CornerRadius = CornerRadiusFor(variant, scope),
        };
    }
}
