using System;
using System.Collections.Generic;
using Godot;

/// <summary>Resultado de una selección del jugador (plan Fase 3).</summary>
public enum SelectionOutcome
{
    /// <summary>No es una palabra pendiente: se desvanece sin penalizar (C4).</summary>
    Invalid,

    /// <summary>Palabra pendiente encontrada.</summary>
    Found,

    /// <summary>Ya estaba encontrada: se ignora y no cuenta como fallo.</summary>
    Repeated,
}

/// <summary>
/// Resultado de validar una selección: qué pasó, qué palabra se leyó y qué
/// celdas hay que pintar (para marcar o para desvanecer).
/// </summary>
public readonly struct SelectionResult
{
    public SelectionResult(SelectionOutcome outcome, string word, IReadOnlyList<Vector2I> cells)
    {
        Outcome = outcome;
        Word = word ?? string.Empty;
        Cells = cells ?? Array.Empty<Vector2I>();
    }

    public SelectionOutcome Outcome { get; }

    public string Word { get; }

    public IReadOnlyList<Vector2I> Cells { get; }

    public bool IsFound => Outcome == SelectionOutcome.Found;

    public bool IsRepeated => Outcome == SelectionOutcome.Repeated;
}

/// <summary>
/// Estado de una partida de la Sopa de Letras (SRP · plan Fase 3): palabras
/// pendientes y encontradas, validación de selecciones y contador de fallos.
/// No conoce nodos ni píxeles, así que se prueba sin escenas y la vista
/// (<see cref="WordSearchMain"/>) solo pinta lo que devuelve <see cref="Select"/>.
/// </summary>
public sealed class WordSearchGame
{
    private readonly GenerationResult _result;
    private readonly HashSet<string> _pending = new();
    private readonly List<string> _order = new();
    private readonly List<string> _found = new();

    /// <summary>Crea la partida a partir del tablero generado.</summary>
    public WordSearchGame(GenerationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        _result = result;
        foreach (WordPlacement placement in result.Placements)
        {
            if (_pending.Add(placement.Word))
            {
                _order.Add(placement.Word);
            }
        }
    }

    public GenerationResult Result => _result;

    /// <summary>Palabras del tablero en el orden en que se colocaron.</summary>
    public IReadOnlyList<string> Order => _order;

    /// <summary>Palabras encontradas, en orden de descubrimiento.</summary>
    public IReadOnlyList<string> FoundWords => _found;

    /// <summary>Palabras que quedan por encontrar (consulta O(1) por palabra).</summary>
    public IReadOnlySet<string> PendingWords => _pending;

    public int FoundCount => _found.Count;

    public int TotalCount => _order.Count;

    /// <summary>Selecciones descartadas por no ser palabra (diagnóstico; C4 no penaliza).</summary>
    public int InvalidSelections { get; private set; }

    public bool IsComplete => TotalCount > 0 && _pending.Count == 0;

    /// <summary>¿Ya se encontró esta palabra?</summary>
    public bool IsFound(string word) => _found.Contains(word);

    /// <summary>
    /// Valida una selección recta. Solo las palabras pendientes cuentan como
    /// encontradas; las ya encontradas se ignoran sin sumar fallos.
    /// </summary>
    public SelectionResult Select(Vector2I start, Vector2I end)
    {
        List<Vector2I> path = WordPath.Build(start, end);
        string word = _result.ReadPath(start, end);

        if (path.Count < 2 || string.IsNullOrEmpty(word))
        {
            InvalidSelections++;
            return new SelectionResult(SelectionOutcome.Invalid, string.Empty, path);
        }

        if (IsFound(word))
        {
            return new SelectionResult(SelectionOutcome.Repeated, word, path);
        }

        if (!_pending.Remove(word))
        {
            InvalidSelections++;
            return new SelectionResult(SelectionOutcome.Invalid, word, path);
        }

        _found.Add(word);
        return new SelectionResult(SelectionOutcome.Found, word, path);
    }
}
