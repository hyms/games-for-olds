using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
/// Tokens de color de un tema visual. Define la paleta completa que se aplica
/// globalmente a celdas, banners, botones y paneles (docs/art_direction.md §2.1).
/// Es una clase pura (no Resource) para poder crear temas en código sin depender
/// de archivos .tres.
/// </summary>
public sealed class ThemeData
{
    public string ThemeId { get; init; } = "classic";

    /// <summary>Fondo principal del juego.</summary>
    public Color BackgroundColor { get; init; } = Color.FromHtml("#FFFFFF");

    /// <summary>Color de relleno de banners de cabecera.</summary>
    public Color HeaderBannerColor { get; init; } = Color.FromHtml("#A0BFC7");

    /// <summary>Color de relleno de sub-banners secundarios.</summary>
    public Color SubheaderBannerColor { get; init; } = Color.FromHtml("#477987");

    /// <summary>Fondo de celdas en reposo (idle).</summary>
    public Color CellIdleBackgroundColor { get; init; } = Color.FromHtml("#EBF2F5");

    /// <summary>Borde de celdas en reposo.</summary>
    public Color CellIdleBorderColor { get; init; } = Color.FromHtml("#A0BFC7");

    /// <summary>Texto de celdas en reposo.</summary>
    public Color CellIdleTextColor { get; init; } = Color.FromHtml("#101010");

    /// <summary>Fondo de celdas cuando la palabra está completada.</summary>
    public Color CellFoundBackgroundColor { get; init; } = Color.FromHtml("#099C92");

    /// <summary>Texto de celdas cuando la palabra está completada (blanco puro).</summary>
    public Color CellFoundTextColor { get; init; } = Color.FromHtml("#FFFFFF");

    /// <summary>Texto del panel inferior (palabras pendientes/encontradas).</summary>
    public Color FooterTextColor { get; init; } = Color.FromHtml("#477987");

    /// <summary>Color del tachado de palabras encontradas en el footer.</summary>
    public Color FooterStrikethroughColor { get; init; } = Color.FromHtml("#099C92");
}
