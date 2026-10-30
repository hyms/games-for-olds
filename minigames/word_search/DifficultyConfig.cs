using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Datos de un nivel de dificultad de la Sopa de Letras (spec §4.1 y §4.2 ·
/// plan Fase 2 §2.1). Toda la lógica del generador se apoya en estos datos:
/// no hay condicionales del tipo «si es el nivel 3» en el código.
/// Un archivo <c>.tres</c> por nivel en <c>resources/difficulties/</c>.
/// </summary>
[GlobalClass]
public partial class DifficultyConfig : Resource
{
    /// <summary>Tablero mínimo permitido (el más pequeño de la spec es 8×8).</summary>
    public const int MinGridSize = 5;

    /// <summary>Tablero máximo permitido (nivel Maestro, decisión C3).</summary>
    public const int MaxGridSize = 15;

    /// <summary>Identificador estable del nivel (= nombre del archivo .tres).</summary>
    [Export]
    public string Id { get; set; } = string.Empty;

    /// <summary>Nombre visible para el jugador (Fácil, Medio, Difícil, Maestro).</summary>
    [Export]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Lado del tablero cuadrado (8, 10, 12 o 15).</summary>
    [Export(PropertyHint.Range, "5,15")]
    public int GridSize { get; set; } = 8;

    /// <summary>Palabras que se intentan colocar (objetivo de la partida).</summary>
    [Export(PropertyHint.Range, "3,30")]
    public int TargetWords { get; set; } = 5;

    /// <summary>Mínimo aceptable tras los reintentos (80 % del objetivo).</summary>
    [Export(PropertyHint.Range, "1,30")]
    public int MinimumWords { get; set; } = 4;

    /// <summary>Vectores unitarios permitidos al colocar palabras (8 direcciones como máximo).</summary>
    [Export]
    public Godot.Collections.Array<Vector2I> Directions { get; set; } = new()
    {
        new Vector2I(1, 0),
        new Vector2I(0, 1),
    };

    /// <summary>Grupos temáticos que se mezclan en cada partida (C3: 1/1/2/4).</summary>
    [Export(PropertyHint.Range, "1,10")]
    public int CategoryCount { get; set; } = 1;

    /// <summary>Tiempo de referencia en segundos para el puntaje (C7).</summary>
    [Export(PropertyHint.Range, "60,3600")]
    public int ParSeconds { get; set; } = 240;

    /// <summary>Id del nivel que hay que superar para desbloquear este (vacío = abierto).</summary>
    [Export]
    public string PreviousLevelId { get; set; } = string.Empty;

    /// <summary>Mínimo efectivo: nunca pide más palabras de las solicitadas.</summary>
    public int EffectiveMinimum => Mathf.Clamp(MinimumWords, 1, Mathf.Max(1, TargetWords));

    /// <summary>Etiqueta corta del tablero («8 × 8») para el selector.</summary>
    public string BoardLabel => $"{GridSize} × {GridSize}";

    /// <summary>Etiqueta completa del nivel («Fácil · 8 × 8 · 5 palabras»).</summary>
    public string SummaryLabel => $"{DisplayName} · {BoardLabel} · {TargetWords} palabras";

    /// <summary>¿La configuración permite colocar palabras en esta dirección?</summary>
    public bool AllowsDirection(Vector2I direction) => Directions.Contains(direction);

    /// <summary>
    /// Comprueba la coherencia de los datos (spec §4.2). Devuelve la lista de
    /// errores; vacío = configuración válida.
    /// </summary>
    public string[] Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Id))
        {
            errors.Add("falta id");
        }

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            errors.Add("falta display_name");
        }

        if (GridSize is < MinGridSize or > MaxGridSize)
        {
            errors.Add($"tablero {GridSize} fuera de {MinGridSize}–{MaxGridSize}");
        }

        if (TargetWords < 1 || TargetWords > GridSize * GridSize)
        {
            errors.Add($"objetivo {TargetWords} imposible en {GridSize}×{GridSize}");
        }

        int minimumRequired = Mathf.CeilToInt(TargetWords * 0.8f);
        if (MinimumWords < minimumRequired || MinimumWords > TargetWords)
        {
            errors.Add($"mínimo {MinimumWords} fuera de {minimumRequired}–{TargetWords} (80 %)");
        }

        if (Directions.Count == 0)
        {
            errors.Add("sin direcciones");
        }

        foreach (Vector2I direction in Directions)
        {
            if (!WordPath.IsUnitDirection(direction))
            {
                errors.Add($"dirección no unitaria: {direction}");
            }
        }

        if (CategoryCount < 1 || CategoryCount > 10)
        {
            errors.Add($"category_count {CategoryCount} fuera de 1–10");
        }

        if (ParSeconds <= 0)
        {
            errors.Add("par_seconds debe ser positivo");
        }

        return errors.ToArray();
    }
}
