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

/// <summary>Medidas tipográficas compartidas por todos los estados de una variante.</summary>
public readonly struct PixelButtonMetrics
{
    public Font Font { get; init; }
    public int FontSize { get; init; }
    public int MarginX { get; init; }
    public int MarginY { get; init; }
    public int HSeparation { get; init; }
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

    /// <summary>El icono se tiñe con el mismo color que el texto para mantener contraste.</summary>
    public Color IconColor => TextColor;
}

/// <summary>
///     Fábrica única de estilos de botón (factory method <see cref="For" />).
///     Centraliza la paleta, las medidas y los seis estados de <c>Button</c>, de modo
///     que HUD, modales, menús y selector de dificultad comparten las mismas reglas:
///     esquinas rectas, borde de 2 px, bisel de 1 px, alfa 1.0 y desplazamiento de
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
                Font = PixelStyles.HudFont,
                FontSize = 16,
                MarginX = 10,
                MarginY = 10,
                HSeparation = 6
            },
            PixelButtonVariant.Action => new PixelButtonMetrics
            {
                Font = PixelStyles.HudFont,
                FontSize = 18,
                MarginX = 14,
                MarginY = 12,
                HSeparation = 8
            },
            _ => new PixelButtonMetrics
            {
                Font = PixelStyles.UiFont,
                FontSize = 20,
                MarginX = 16,
                MarginY = 12,
                HSeparation = 8
            }
        };
    }

    /// <summary>
    ///     Construye el estilo de un estado. El estado <see cref="PixelButtonState.Pressed" />
    ///     conserva la altura total (top + 2 px, bottom − 2 px) y el resto de estados
    ///     comparten márgenes para que el botón no cambie de tamaño al interactuar.
    /// </summary>
    public static PixelButtonVisual For(PixelButtonVariant variant, PixelButtonState state, ThemeData theme)
    {
        theme ??= new ThemeData();
        var metrics = MetricsFor(variant);
        var fill = FillFor(state, theme);
        var border = PixelStyles.BorderColor(fill);
        var isPressed = state == PixelButtonState.Pressed;

        var style = new StyleBoxFlat
        {
            BgColor = state == PixelButtonState.Focus ? new Color(0, 0, 0, 0) : fill,
            BorderColor = state == PixelButtonState.Focus
                ? PixelStyles.TextColor(theme.BackgroundColor)
                : border,
            AntiAliasing = false,
            CornerDetail = 0,
            DrawCenter = state != PixelButtonState.Focus
        };
        style.SetBorderWidthAll(PixelReliefPainter.BorderWidth);
        style.SetCornerRadiusAll(PixelStyles.Radius);
        style.ContentMarginLeft = metrics.MarginX;
        style.ContentMarginRight = metrics.MarginX;
        style.ContentMarginTop = metrics.MarginY + (isPressed ? PressShift : 0);
        style.ContentMarginBottom = metrics.MarginY - (isPressed ? PressShift : 0);

        if (state == PixelButtonState.Normal)
        {
            style.ShadowSize = 2;
            style.ShadowColor = PixelStyles.Sombra;
            style.ShadowOffset = new Vector2(2, 2);
        }

        if (state == PixelButtonState.Focus)
        {
            // El foco se pinta encima del estilo base: solo un anillo exterior.
            style.SetExpandMarginAll(2);
        }

        return new PixelButtonVisual
        {
            Metrics = metrics,
            Style = style,
            TextColor = TextFor(state, theme),
            Raised = state is PixelButtonState.Normal or PixelButtonState.Focus,
            DrawRelief = state != PixelButtonState.Disabled
        };
    }

    private static Color FillFor(PixelButtonState state, ThemeData theme)
    {
        return state switch
        {
            PixelButtonState.Hover => theme.SubheaderBannerColor,
            PixelButtonState.Pressed => theme.CellFoundBackgroundColor.Darkened(0.3f),
            PixelButtonState.Disabled => PixelStyles.Arena,
            _ => theme.HeaderBannerColor
        };
    }

    private static Color TextFor(PixelButtonState state, ThemeData theme)
    {
        return state switch
        {
            // El foco se dibuja sobre el relleno normal.
            PixelButtonState.Focus => PixelStyles.TextColor(theme.HeaderBannerColor),
            PixelButtonState.Disabled => PixelStyles.TextColor(PixelStyles.Arena),
            _ => PixelStyles.TextColor(FillFor(state, theme))
        };
    }
}
