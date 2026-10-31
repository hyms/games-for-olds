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
    /// <summary>Separación entre celdas (docs/art_direction.md §3, 41:6 escala).</summary>
    public const float CellGap = 6.0f;

    /// <summary>Tamaño mínimo de celda antes del zoom (≥24 para visibilidad).</summary>
    public const int MinCellSize = 24;

    /// <summary>Tamaño de letra mentre el contenedor aún no tiene tamaño.</summary>
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

    // --- Zoom (Fase 4) -------------------------------------------------------
    /// <summary>Factor zoom mínimo sin distorsión.</summary>
    public const float MinZoom = 1.0f;

    /// <summary>Factor zoom máximo con dos dedos.</summary>
    public const float MaxZoom = 2.5f;

    /// <summary>Incremento de zoom al pinchar (factor por pincel).</summary>
    public const float ZoomStep = 1.2f;

    /// <summary>Margen lateral escalado (docs/art_direction.md §3, 19px base).</summary>
    public const float BoardMargin = 19.0f;

    /// <summary>Factor que escala el margen lateral por nivel dificultad.</summary>
    public const int MarginScalePerLevel = 1;

    // --- Selector de dificultad ---------------------------------------------
    /// <summary>Alto mínimo de cada botón de nivel (objetivo táctil, spec §3).</summary>
    public const int LevelButtonMinHeight = 76;

    /// <summary>Tamaño de letra de los botones de nivel.</summary>
    public const int LevelButtonFontSize = 20;
}
