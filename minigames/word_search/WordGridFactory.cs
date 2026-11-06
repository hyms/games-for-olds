using System;
using Godot;

namespace ClasicosMenteActiva.minigames.word_search;

/// <summary>
///     Geometría de la grilla calculada por <see cref="WordGridFactory" />
///     (spec §5.2): tamaño de celda y origen centrado para ubicar las celdas.
/// </summary>
public readonly struct WordGridLayout
{
    public readonly float CellSize;
    public readonly Vector2 Origin;

    public WordGridLayout(float cellSize, Vector2 origin)
    {
        CellSize = cellSize;
        Origin = origin;
    }
}

/// <summary>
///     Factory del tablero (patrón Factory · spec §5.2 · Single Responsibility).
///     Responsable exclusivo de instanciar y ubicar espacialmente las celdas
///     <see cref="WordCell" /> en la grilla según la dimensión del nivel. El
///     contenedor solo orquesta: este tipo decide geometría y colocación.
/// </summary>
public static class WordGridFactory
{
    /// <summary>Instancia la matriz de celdas según el resultado generado.</summary>
    public static WordCell[,] CreateCells(GenerationResult result, int fontSize)
    {
        ArgumentNullException.ThrowIfNull(result);

        var cells = new WordCell[result.Rows, result.Columns];
        for (var y = 0; y < result.Rows; y++)
        for (var x = 0; x < result.Columns; x++)
        {
            var coord = new Vector2I(x, y);
            cells[y, x] = WordCellFactory.Create(coord, result.GetLetter(coord), fontSize);
        }

        return cells;
    }

    /// <summary>Calcula la geometría de la grilla para el área disponible del contenedor.</summary>
    public static WordGridLayout ComputeLayout(int rows, int columns, Vector2 availableSize)
    {
        if (rows <= 0 || columns <= 0 || availableSize.X <= 0 || availableSize.Y <= 0)
            return new WordGridLayout(0, Vector2.Zero);

        var gap = WordSearchVisuals.CellGap;
        var stepX = (availableSize.X - gap * (columns - 1)) / columns;
        var stepY = (availableSize.Y - gap * (rows - 1)) / rows;
        var cellSize = Mathf.Max(WordSearchVisuals.MinCellSize, Mathf.Floor(Mathf.Min(stepX, stepY)));

        var totalWidth = cellSize * columns + gap * (columns - 1);
        var totalHeight = cellSize * rows + gap * (rows - 1);
        var origin = new Vector2(
            Mathf.Floor((availableSize.X - totalWidth) / 2.0f),
            Mathf.Floor((availableSize.Y - totalHeight) / 2.0f));

        return new WordGridLayout(cellSize, origin);
    }

    /// <summary>Ubica espacialmente cada celda (posición, tamaño y fuente) según el layout.</summary>
    public static void PositionCells(WordCell[,] cells, WordGridLayout layout, int fontSize)
    {
        if (cells == null || layout.CellSize <= 0) return;

        var rows = cells.GetLength(0);
        var columns = cells.GetLength(1);
        var step = layout.CellSize + WordSearchVisuals.CellGap;

        for (var y = 0; y < rows; y++)
        for (var x = 0; x < columns; x++)
        {
            var cell = cells[y, x];
            if (cell == null || !GodotObject.IsInstanceValid(cell)) continue;

            cell.Position = layout.Origin + new Vector2(x * step, y * step);
            cell.Size = new Vector2(layout.CellSize, layout.CellSize);
            cell.SetFontSize(fontSize);
        }
    }
}