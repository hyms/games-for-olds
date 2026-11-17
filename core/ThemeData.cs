using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
///     Tokens de color de un tema visual. Define la paleta completa que se aplica
///     globalmente a celdas, banners, botones y paneles (docs/art_direction.md §2.1).
///     Es una clase pura (no Resource) para poder crear temas en código sin depender
///     de archivos .tres.
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

    /// <summary>
    ///     Color cálido de confeti/celebración (coral). Usado en el estallido de
    ///     partículas de victoria de la Sopa de Letras (docs/art_direction.md §4,
    ///     token agregado v0.5.0 de forma aditiva). No altera ningún token existente.
    /// </summary>
    public Color CelebrationRed { get; init; } = Color.FromHtml("#B45252");

    /// <summary>WS-Fondo: fondo madera de escena y relleno de botones HUD (Word Search rev. 1.1).</summary>
    public Color WSFondo { get; init; } = Color.FromHtml("#A17A5B");

    /// <summary>WS-MaderaBorde: borde visible de botones, marco del tablero y del footer.</summary>
    public Color WSMaderaBorde { get; init; } = Color.FromHtml("#3A2A1E");

    /// <summary>WS-Crema: placa de dificultad (banda de nivel).</summary>
    public Color WSCrema { get; init; } = Color.FromHtml("#EDE19E");

    /// <summary>WS-CremaBorde: borde de la placa de dificultad.</summary>
    public Color WSCremaBorde { get; init; } = Color.FromHtml("#C4B476");

    /// <summary>WS-CremaSuave: %TimeBox y tarjeta del footer.</summary>
    public Color WSCremaSuave { get; init; } = Color.FromHtml("#F1EFE2");

    /// <summary>WS-Celda: relleno de celda en reposo.</summary>
    public Color WSCelda { get; init; } = Color.FromHtml("#F1EDE1");

    /// <summary>WS-Acento / WS-Encontrado: estado encontrado/selección (coral sólido).</summary>
    public Color WSAcento { get; init; } = Color.FromHtml("#FD6D52");

    /// <summary>WS-EncontradoBorde: borde/contorno de celda encontrada.</summary>
    public Color WSEncontradoBorde { get; init; } = Color.FromHtml("#8E3B33");

    /// <summary>WS-CeldaAviso: variante de celda en foco/hover/hint suave.</summary>
    public Color WSCeldaAviso { get; init; } = Color.FromHtml("#B07259");

    /// <summary>WS-CianTitulo: título sobre fondo madera (requiere contorno WS-MaderaSombra).</summary>
    public Color WSCianTitulo { get; init; } = Color.FromHtml("#65B9C4");

    /// <summary>WS-MaderaSombra: contorno/sombra dura del título y separadores.</summary>
    public Color WSMaderaSombra { get; init; } = Color.FromHtml("#2E1D12");

    /// <summary>WS-Tachado: tachado + texto de palabra encontrada en el footer.</summary>
    public Color WSTachado { get; init; } = Color.FromHtml("#9B4E48");
}