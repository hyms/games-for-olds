using Godot;

/// <summary>
/// Valores de presentación de la Sopa de Letras en un único sitio: geometría
/// del tablero, tipografía de celda, tiempos de feedback y medidas del
/// selector. La lógica de juego no decide píxeles ni duraciones (SRP · plan
/// Fase 3). Los colores y estilos salen de <see cref="PixelStyles"/>.
/// </summary>
public static class WordSearchVisuals
{
    // --- Tablero -------------------------------------------------------------
    /// <summary>Separación entre celdas (docs/art_direction.md §3).</summary>
    public const float CellGap = 6.0f;

    /// <summary>Tamaño mínimo de celda antes del zoom de la Fase 4.</summary>
    public const int MinCellSize = 10;

    /// <summary>Tamaño de letra mientras el contenedor aún no tiene tamaño.</summary>
    public const int DefaultCellFontSize = 20;

    /// <summary>Proporción del lado de celda que ocupa la letra.</summary>
    public const float CellFontRatio = 0.52f;

    /// <summary>Límites del tamaño de letra de una celda en píxeles.</summary>
    public const int MinCellFontSize = 10;

    public const int MaxCellFontSize = 24;

    // --- Feedback ------------------------------------------------------------
    /// <summary>Duración del desvanecido de una selección inválida (spec §3).</summary>
    public const float InvalidFadeSeconds = 0.5f;

    /// <summary>Transparencia mínima del desvanecido (vuelve a 1 al terminar).</summary>
    public const float InvalidFadeAlpha = 0.25f;

    /// <summary>Grosor del trazo de arrastre en píxeles.</summary>
    public const float TraceWidth = 6.0f;

    /// <summary>Opacidad de la línea del trazo.</summary>
    public const float TraceAlpha = 0.7f;

    /// <summary>Radio y opacidad de los nodos de los extremos del trazo.</summary>
    public const float TraceDotRadius = 5.0f;

    public const float TraceDotAlpha = 0.9f;

    // --- Selector de dificultad ---------------------------------------------
    /// <summary>Alto mínimo de cada botón de nivel (objetivo táctil, spec §3).</summary>
    public const int LevelButtonMinHeight = 76;

    /// <summary>Tamaño de letra de los botones de nivel.</summary>
    public const int LevelButtonFontSize = 20;
}
