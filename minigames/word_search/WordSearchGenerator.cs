using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Palabra colocada en el tablero: texto normalizado, celda de inicio,
/// dirección unitaria y ruta completa de celdas (extremos incluidos).
/// </summary>
public sealed class WordPlacement
{
    public WordPlacement(string word, Vector2I start, Vector2I direction, IReadOnlyList<Vector2I> cells)
    {
        Word = word;
        Start = start;
        Direction = direction;
        Cells = cells;
    }

    public string Word { get; }

    public Vector2I Start { get; }

    public Vector2I Direction { get; }

    public Vector2I End => Cells[^1];

    public IReadOnlyList<Vector2I> Cells { get; }
}

/// <summary>
/// Resultado de generar un tablero (plan Fase 2 §2.2). Siempre contiene una
/// matriz rellena: si no se alcanza el mínimo, <see cref="MeetsMinimum"/> es
/// <c>false</c> y el juego decide qué hacer (fallo controlado, sin tablero vacío).
/// </summary>
public sealed class GenerationResult
{
    public GenerationResult(
        char[,] grid,
        IReadOnlyList<WordPlacement> placements,
        IReadOnlyList<string> requestedWords,
        int minimumRequired,
        int attempts)
    {
        Grid = grid;
        Placements = placements;
        RequestedWords = requestedWords;
        MinimumRequired = minimumRequired;
        Attempts = attempts;
    }

    /// <summary>Letras del tablero; todas las celdas están rellenas.</summary>
    public char[,] Grid { get; }

    public IReadOnlyList<WordPlacement> Placements { get; }

    /// <summary>Palabras que se pidieron colocar (en el orden del intento aceptado).</summary>
    public IReadOnlyList<string> RequestedWords { get; }

    /// <summary>Mínimo aceptable para dar el tablero por válido.</summary>
    public int MinimumRequired { get; }

    /// <summary>Reintentos que necesitó el generador.</summary>
    public int Attempts { get; }

    public int Rows => Grid.GetLength(0);

    public int Columns => Grid.GetLength(1);

    public int PlacedCount => Placements.Count;

    public bool MeetsMinimum => PlacedCount >= MinimumRequired;

    /// <summary>Palabras que no cupo colocar en el intento aceptado.</summary>
    public List<string> UnplacedWords
    {
        get
        {
            var placed = new HashSet<string>();
            foreach (WordPlacement placement in Placements)
            {
                placed.Add(placement.Word);
            }

            var unplaced = new List<string>();
            foreach (string word in RequestedWords)
            {
                if (!placed.Contains(word))
                {
                    unplaced.Add(word);
                }
            }

            return unplaced;
        }
    }

    /// <summary>Letra de una celda; espacio si la coordenada queda fuera.</summary>
    public char GetLetter(Vector2I coord)
    {
        if (coord.X < 0 || coord.Y < 0 || coord.X >= Columns || coord.Y >= Rows)
        {
            return ' ';
        }

        return Grid[coord.Y, coord.X];
    }

    /// <summary>
    /// Lee la palabra que forma una selección recta. Devuelve cadena vacía si
    /// la selección no está alineada o tiene una sola celda.
    /// </summary>
    public string ReadPath(Vector2I start, Vector2I end)
    {
        List<Vector2I> path = WordPath.Build(start, end);
        if (path.Count < 2)
        {
            return string.Empty;
        }

        var letters = new System.Text.StringBuilder(path.Count);
        foreach (Vector2I coord in path)
        {
            letters.Append(GetLetter(coord));
        }

        return letters.ToString();
    }
}

/// <summary>
/// Generador de tableros de la Sopa de Letras (patrón Strategy · spec §6 ·
/// plan Fase 2 §2.2). Recibe la <see cref="DifficultyConfig"/> y la lista de
/// palabras, y no conoce la interfaz: se puede probar sin escenas.
/// </summary>
public sealed class WordSearchGenerator
{
    /// <summary>Reintentos completos antes de aceptar el 80 % (plan Fase 2).</summary>
    public const int MaxAttempts = 30;

    /// <summary>Rondas de colocación por intento (la segunda recoloca los fallos).</summary>
    private const int PlacementPasses = 2;

    /// <summary>Intentos que necesitó la última generación (diagnóstico y tests).</summary>
    public int LastAttempts { get; private set; }

    /// <summary>
    /// Genera un tablero. <paramref name="seed"/> ≥ 0 fija la semilla (tests y
    /// depuración); -1 usa una semilla aleatoria.
    /// </summary>
    public GenerationResult Generate(DifficultyConfig config, IEnumerable<string> words, long seed = -1)
    {
        ArgumentNullException.ThrowIfNull(config);

        int size = Mathf.Clamp(config.GridSize, DifficultyConfig.MinGridSize, DifficultyConfig.MaxGridSize);
        List<string> pool = PrepareWords(words, size);
        int target = Mathf.Min(Mathf.Max(1, config.TargetWords), Mathf.Max(1, pool.Count));
        int minimum = Mathf.Min(target, config.EffectiveMinimum);
        List<Vector2I> directions = UsableDirections(config.Directions);

        RandomNumberGenerator rng = Utils.CreateRng(seed);

        GenerationResult best = null;
        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            LastAttempts = attempt;
            List<string> shuffled = new(pool);
            Utils.Shuffle(shuffled, rng);
            if (shuffled.Count > target)
            {
                shuffled.RemoveRange(target, shuffled.Count - target);
            }

            GenerationResult current = Build(size, shuffled, directions, minimum, attempt, rng);
            if (best == null || current.PlacedCount > best.PlacedCount)
            {
                best = current;
            }

            if (best.PlacedCount >= target)
            {
                break;
            }
        }

        return best ?? Build(size, new List<string>(), directions, minimum, 1, rng);
    }

    /// <summary>Crea una matriz vacía (celdas <c>'\0'</c>).</summary>
    public static char[,] CreateEmptyGrid(int rows, int columns) => new char[rows, columns];

    /// <summary>
    /// Normaliza, filtra por longitud y elimina duplicados (UT-WS-04).
    /// Acepta 4–<paramref name="maxLength"/> letras A–Z.
    /// </summary>
    public static List<string> PrepareWords(IEnumerable<string> words, int maxLength)
    {
        var prepared = new List<string>();
        var seen = new HashSet<string>();
        if (words == null)
        {
            return prepared;
        }

        foreach (string raw in words)
        {
            string normalized = WordSearchTheme.NormalizeWord(raw);
            if (normalized.Length < WordSearchTheme.MinBankWordLength || normalized.Length > maxLength)
            {
                continue;
            }

            if (seen.Add(normalized))
            {
                prepared.Add(normalized);
            }
        }

        return prepared;
    }

    /// <summary>
    /// Coloca una palabra concreta si cabe (permite solapar solo con la misma
    /// letra). Se expone para pruebas y para la pista de la Fase 6.
    /// </summary>
    public static bool TryPlaceWord(string word, Vector2I start, Vector2I direction, char[,] grid)
    {
        if (grid == null || !WordPath.IsUnitDirection(direction))
        {
            return false;
        }

        string normalized = WordSearchTheme.NormalizeWord(word);
        if (normalized.Length < WordSearchTheme.MinBankWordLength || !Fits(normalized, start, direction, grid))
        {
            return false;
        }

        WriteWord(normalized, start, direction, grid);
        return true;
    }

    /// <summary>Cuenta las letras que coinciden con celdas ya ocupadas (requiere que la palabra quepa).</summary>
    private static int OverlapCount(string word, Vector2I start, Vector2I direction, char[,] grid)
    {
        int overlaps = 0;
        for (int index = 0; index < word.Length; index++)
        {
            Vector2I coord = start + (direction * index);
            if (grid[coord.Y, coord.X] == word[index])
            {
                overlaps++;
            }
        }

        return overlaps;
    }

    private static GenerationResult Build(
        int size,
        List<string> words,
        List<Vector2I> directions,
        int minimum,
        int attempt,
        RandomNumberGenerator rng)
    {
        char[,] grid = CreateEmptyGrid(size, size);
        var placements = new List<WordPlacement>();
        List<string> pending = new(words);

        for (int pass = 0; pass < PlacementPasses && pending.Count > 0; pass++)
        {
            var stillPending = new List<string>();
            foreach (string word in pending)
            {
                if (TryFindPlacement(word, grid, directions, rng, out WordPlacement placement))
                {
                    placements.Add(placement);
                }
                else
                {
                    stillPending.Add(word);
                }
            }

            pending = stillPending;
        }

        FillEmptyCells(grid, rng);
        return new GenerationResult(grid, placements, words, minimum, attempt);
    }

    private static bool TryFindPlacement(
        string word,
        char[,] grid,
        List<Vector2I> directions,
        RandomNumberGenerator rng,
        out WordPlacement placement)
    {
        int size = grid.GetLength(0);
        var overlapping = new List<(Vector2I Start, Vector2I Direction)>();
        var free = new List<(Vector2I Start, Vector2I Direction)>();

        foreach (Vector2I direction in directions)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var start = new Vector2I(x, y);
                    if (!Fits(word, start, direction, grid))
                    {
                        continue;
                    }

                    if (OverlapCount(word, start, direction, grid) > 0)
                    {
                        overlapping.Add((start, direction));
                    }
                    else
                    {
                        free.Add((start, direction));
                    }
                }
            }
        }

        List<(Vector2I Start, Vector2I Direction)> candidates = overlapping.Count > 0 ? overlapping : free;
        if (candidates.Count == 0)
        {
            placement = null;
            return false;
        }

        (Vector2I chosenStart, Vector2I chosenDirection) = candidates[rng.RandiRange(0, candidates.Count - 1)];
        List<Vector2I> cells = WordPath.Build(chosenStart, chosenDirection, word.Length);
        WriteWord(word, chosenStart, chosenDirection, grid);
        placement = new WordPlacement(word, chosenStart, chosenDirection, cells);
        return true;
    }

    private static bool Fits(string word, Vector2I start, Vector2I direction, char[,] grid)
    {
        int rows = grid.GetLength(0);
        int columns = grid.GetLength(1);
        for (int index = 0; index < word.Length; index++)
        {
            Vector2I coord = start + (direction * index);
            if (coord.X < 0 || coord.Y < 0 || coord.X >= columns || coord.Y >= rows)
            {
                return false;
            }

            char current = grid[coord.Y, coord.X];
            if (current != '\0' && current != word[index])
            {
                return false;
            }
        }

        return true;
    }

    private static void WriteWord(string word, Vector2I start, Vector2I direction, char[,] grid)
    {
        for (int index = 0; index < word.Length; index++)
        {
            Vector2I coord = start + (direction * index);
            grid[coord.Y, coord.X] = word[index];
        }
    }

    private static void FillEmptyCells(char[,] grid, RandomNumberGenerator rng)
    {
        int rows = grid.GetLength(0);
        int columns = grid.GetLength(1);
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (grid[y, x] == '\0')
                {
                    grid[y, x] = (char)('A' + rng.RandiRange(0, 25));
                }
            }
        }
    }

    private static List<Vector2I> UsableDirections(Godot.Collections.Array<Vector2I> configured)
    {
        var directions = new List<Vector2I>();
        if (configured != null)
        {
            foreach (Vector2I direction in configured)
            {
                if (WordPath.IsUnitDirection(direction) && !directions.Contains(direction))
                {
                    directions.Add(direction);
                }
            }
        }

        if (directions.Count == 0)
        {
            GD.PushWarning("[WordSearchGenerator] configuración sin direcciones válidas: se usan las 2 de avance");
            return new List<Vector2I>(WordPath.OrthogonalForward);
        }

        return directions;
    }

}
