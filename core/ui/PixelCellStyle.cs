using ClasicosMenteActiva.core;
using Godot;

/// <summary>Resultado inmutable para pintar una celda en un estado concreto.</summary>
public readonly struct PixelCellVisual
{
    public StyleBoxFlat Style { get; init; }
    public Color TextColor { get; init; }

    /// <summary>True si el bisel debe dibujarse elevado; false si debe hundirse.</summary>
    public bool Raised { get; init; }

    /// <summary>False solo si la celda no debe mostrar relieve.</summary>
    public bool DrawRelief { get; init; }
}

/// <summary>
///     Fábrica de estilos de celda (factory method <see cref="For" />). Reposo y
///     pista usan relieve elevado; pulsada, seleccionada y encontrada usan relieve
///     hundido, tal como un botón físico presionado. Comparte el bisel de
///     <see cref="PixelReliefPainter" /> con los botones para mantener una sola
///     gramática visual (docs/art_direction.md §3).
/// </summary>
public static class PixelCellStyle
{
    /// <summary>Estilo de celda para el estado indicado y el tema activo.</summary>
    public static PixelCellVisual For(PixelCellState state, ThemeData theme)
    {
        theme ??= new ThemeData();
        var (fill, border, text, raised) = state switch
        {
            PixelCellState.Pressed => (
                PixelStyles.Celda.Darkened(0.08f),
                theme.CellIdleBorderColor,
                PixelStyles.TextColor(PixelStyles.Celda.Darkened(0.08f)),
                false),
            PixelCellState.Selected => (
                theme.CellFoundBackgroundColor, // WS-Acento (coral, rev. 1.1)
                PixelStyles.WSEncontradoBorde,
                PixelStyles.TextDark, // Texto oscuro sobre coral
                false),
            PixelCellState.Found => (
                theme.CellFoundBackgroundColor, // WS-Acento (coral, rev. 1.1)
                PixelStyles.WSEncontradoBorde,
                PixelStyles.TextDark, // Texto oscuro sobre coral
                false),
            PixelCellState.Hint => (
                PixelStyles.PistaAcento,
                PixelStyles.PistaBorde,
                PixelStyles.TextColor(PixelStyles.PistaAcento),
                true),
            _ => (
                theme.CellIdleBackgroundColor, // WS-Celda (rev. 1.1)
                PixelStyles.WSMaderaBorde,
                PixelStyles.TextColor(theme.CellIdleBackgroundColor),
                true)
        };

        var style = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = border,
            AntiAliasing = false,
            CornerDetail = 0,
            DrawCenter = true
        };
        style.SetBorderWidthAll(PixelReliefPainter.BorderWidth);
        style.SetCornerRadiusAll(PixelStyles.Radius);

        return new PixelCellVisual
        {
            Style = style,
            TextColor = text,
            Raised = raised,
            DrawRelief = true
        };
    }
}
