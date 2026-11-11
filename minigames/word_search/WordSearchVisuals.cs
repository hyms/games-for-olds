/// <summary>
///     Valores de presentación de la Sopa de Letras en un único sitio: geometría
///     del tablero, tipografía de celda, tiempos de feedback y medidas del
///     selector. La lógica de juego no decide píxeles ni duraciones (SRP · plan
///     Fase 3). Los colores y estilos salen de <see cref="PixelStyles" />.
/// </summary>
public static class WordSearchVisuals
{
    // --- Tablero -------------------------------------------------------------
    /// <summary>
    ///     Lado de celda y separación FIJOS por tamaño de tablero, tomados de la
    ///     plantilla retro (<c>docs/word_search/wathersoup.jpg</c>). Son el máximo
    ///     por dificultad: <see cref="WordGridFactory" /> solo los reduce lo justo
    ///     para que la grilla entre entera en el área disponible, de modo que no
    ///     exista recorte vertical ni horizontal en ningún viewport.
    /// </summary>
    /// <param name="rows">Filas del tablero.</param>
    /// <param name="columns">Columnas del tablero.</param>
    /// <returns>Tamaño de celda y separación en píxeles enteros.</returns>
    public static (int CellSize, int Gap) BoardMetricsFor(int rows, int columns)
    {
        var side = System.Math.Max(rows, columns);
        return side switch
        {
            <= 8 => (36, 8),   // Fácil    · 8 × 8
            <= 10 => (30, 6),  // Medio    · 10 × 10
            <= 12 => (24, 6),  // Difícil  · 12 × 12
            _ => (20, 4),      // Maestro  · 15 × 15
        };
    }

    /// <summary>Luz del bisel elevado de celda: 1 px arriba/izquierda (plantilla retro).</summary>
    public const int ReliefHighlightWidth = 1;

    /// <summary>Sombra dura del bisel elevado de celda: 2 px abajo/derecha (plantilla retro).</summary>
    public const int ReliefShadowWidth = 2;

    /// <summary>
    ///     Piso de seguridad del lado de celda. Con <see cref="BoardMetricsFor" />
    ///     ningún nivel del juego lo alcanza en 360 × 640; existe para que un
    ///     viewport excepcionalmente estrecho no produzca celdas de 0 px.
    /// </summary>
    public const int MinCellSize = 16;

    /// <summary>Tamaño de letra mientras el contenedor aún no tiene tamaño.</summary>
    public const int DefaultCellFontSize = 20;

    /// <summary>Proporción del lado de celda que ocupa la letra (cap-height ≈ celda).</summary>
    public const float CellFontRatio = 0.92f;

    /// <summary>Límites del tamaño de letra de una celda en píxeles.</summary>
    public const int MinCellFontSize = 10;

    /// <summary>Máximo tamaño de fuente por celda (soporta escalado a grandes grillas).</summary>
    public const int MaxCellFontSize = 48;

    // --- Feedback ------------------------------------------------------------
    /// <summary>Duración del resaltado de pista (spec §4.2, 1.5 s).</summary>
    public const float HintFlashSeconds = 1.5f;

    /// <summary>Duración del destello de una selección inválida (spec §3).</summary>
    public const float InvalidFlashSeconds = 0.5f;

    /// <summary>Grosor del trazo de arrastre en píxeles.</summary>
    public const float TraceWidth = 6.0f;

    /// <summary>Opacidad de la línea del trazo (opacidad absoluta, regla de oro pixel art).</summary>
    public const float TraceAlpha = 1.0f;

    /// <summary>Radio y opacidad de los nodos de los extremos del trazo.</summary>
    public const float TraceDotRadius = 5.0f;

    public const float TraceDotAlpha = 1.0f;

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