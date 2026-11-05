using System;
using System.Linq;
using System.Collections.Generic;
using ClasicosMenteActiva.minigames.word_search;
using Godot;

/// <summary>
/// Tablero de la Sopa de Letras (patrón Composite · spec §6 · plan Fase 3 §3.1).
/// Crea y coloca las <see cref="WordCell"/>, gestiona la selección por arrastre
/// (con bloqueo de dirección y ajuste a la línea dominante) y emite
/// <c>word_selected(inicio, fin)</c> al soltar.
/// </summary>
public partial class WordGridContainer : Control
{
    /// <summary>Se emite al soltar una selección recta de 2 o más celdas.</summary>
    [Signal]
    public delegate void WordSelectedEventHandler(Vector2I start, Vector2I end);

    private readonly DragSelection _selection = new();
    private readonly BoardInputHandler _input = new();
    private readonly List<Vector2I> _preview = new();
    private readonly List<Vector2I> _invalidCells = new();

    private WordCell[,] _cells;
    private char[,] _letters;
    private WordTraceOverlay _trace;
    private Tween _invalidTween;
    private Timer _hintTimer;
    private string _hintWord = string.Empty;
    private float _cellSize;
    private Vector2 _origin;

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public float CellSize => _cellSize;

    public bool IsSelecting => _selection.IsActive;

    /// <summary>Último resultado de generación (para leer palabras).</summary>
    public GenerationResult Result { get; private set; }

    public override void _Ready()
    {
        // MouseFilter.Stop captura todos los eventos táctiles dentro del área del
        // contenedor. Es necesario en Android para que el arrastre (InputEventScreenTouch
        // + InputEventScreenDrag) se entregue de forma confiable y no se pierda entre
        // nodos hijos.
        MouseFilter = MouseFilterEnum.Stop;
        EnsureTrace();
        EnsureHintTimer();
        UpdateLayout();

        // Conectar traductor de input (Composite: el contenedor orquesta, el handler
        // solo traduce eventos a acciones).
        _input.SelectionBegan += OnInputBegan;
        _input.SelectionUpdated += OnInputUpdated;
        _input.SelectionEnded += OnInputEnded;
        _input.SelectionCanceled += OnInputCanceled;

    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            UpdateLayout();
        }
    }

    /// <summary>Monta el tablero con el resultado generado (la reemplaza si ya había una).</summary>
    public void Setup(GenerationResult result)
    {
        if (result == null)
        {
            return;
        }

        CancelSelection();
        KillInvalidTween();
        EnsureTrace();

        Result = result;
        Rows = result.Rows;
        Columns = result.Columns;
        _letters = result.Grid;

        // Asegurar que el contenedor capture todo el input táctil después de
        // reconstruir el tablero (Setup se llama dinámicamente por cada partida).
        MouseFilter = MouseFilterEnum.Stop;

        foreach (Node child in GetChildren())
        {
            // El trazo y el temporizador de pistas sobreviven a cada partida.
            if (child == _trace || child == _hintTimer)
            {
                continue;
            }

            RemoveChild(child);
            child.QueueFree();
        }

        _cells = new WordCell[Rows, Columns];
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                var cell = WordCellFactory.Create(new Vector2I(x, y), result.GetLetter(new Vector2I(x, y)), WordSearchVisuals.DefaultCellFontSize);
                AddChild(cell);
                _cells[y, x] = cell;
            }
        }

        UpdateLayout();
    }

    /// <summary>Celda en una coordenada; null si está fuera del tablero.</summary>
    public WordCell GetCell(Vector2I coord) => IsValidCell(coord) ? _cells[coord.Y, coord.X] : null;

    public bool IsValidCell(Vector2I coord)
    {
        return _cells != null
            && coord.X >= 0
            && coord.Y >= 0
            && coord.X < Columns
            && coord.Y < Rows;
    }

    /// <summary>Estado visual actual de una celda.</summary>
    public WordCellState GetCellState(Vector2I coord)
    {
        var cell = GetCell(coord);
        return cell != null ? cell.WordState : WordCellState.Normal;
    }

    /// <summary>Centro de una celda en coordenadas locales del contenedor.</summary>
    public Vector2 GetCellCenter(Vector2I coord)
    {
        float step = _cellSize + WordSearchVisuals.CellGap;
        return _origin + new Vector2((coord.X * step) + (_cellSize / 2.0f), (coord.Y * step) + (_cellSize / 2.0f));
    }

    /// <summary>Rectángulo de una celda en coordenadas locales.</summary>
    public Rect2 GetCellRect(Vector2I coord)
    {
        float step = _cellSize + WordSearchVisuals.CellGap;
        return new Rect2(_origin + new Vector2(coord.X * step, coord.Y * step), new Vector2(_cellSize, _cellSize));
    }

    /// <summary>Celda bajo un punto local; false si el punto queda fuera del tablero.</summary>
    public bool TryGetCell(Vector2 localPosition, out Vector2I coord)
    {
        coord = Vector2I.Zero;
        if (_letters == null || _cellSize <= 0)
        {
            return false;
        }

        Vector2 point = localPosition - _origin;
        if (point.X < 0 || point.Y < 0)
        {
            return false;
        }

        float step = _cellSize + WordSearchVisuals.CellGap;
        int column = Mathf.FloorToInt(point.X / step);
        int row = Mathf.FloorToInt(point.Y / step);
        if (column < 0 || row < 0 || column >= Columns || row >= Rows)
        {
            return false;
        }

        coord = new Vector2I(column, row);
        return true;
    }

    // --- Selección por arrastre ---------------------------------------------

    /// <summary>Inicia la selección en el punto local indicado.</summary>
    public void BeginSelection(Vector2 localPosition)
    {
        if (TryGetCell(localPosition, out Vector2I coord))
        {
            BeginSelectionAt(coord);
        }
    }

    /// <summary>Inicia la selección en una celda concreta.</summary>
    public void BeginSelectionAt(Vector2I coord)
    {
        if (!IsValidCell(coord))
        {
            return;
        }

        // Si una selección previa quedó atrapada (p. ej. se perdió el Touch.Release
        // en Android), cancelarla antes de iniciar una nueva.
        if (_selection.IsActive)
        {
            CancelSelection();
        }

        KillInvalidTween();
        ClearPreview();
        _selection.Begin(coord);
        ShowPreview();
    }

    /// <summary>Actualiza la selección con el punto local bajo el dedo.</summary>
    public void UpdateSelection(Vector2 localPosition)
    {
        if (!_selection.IsActive)
        {
            return;
        }

        if (!TryGetCell(localPosition, out Vector2I coord))
        {
            CancelSelection();
            return;
        }

        UpdateSelectionAt(coord);
    }

    /// <summary>Actualiza la selección con la celda bajo el dedo.</summary>
    public void UpdateSelectionAt(Vector2I coord)
    {
        if (!_selection.IsActive)
        {
            return;
        }

        _selection.Update(coord, Rows, Columns);
        ShowPreview();
    }

    /// <summary>Termina la selección y emite <c>word_selected</c> si es válida.</summary>
    public void EndSelection()
    {
        if (!_selection.IsActive)
        {
            return;
        }

        bool valid = _selection.TryComplete(out Vector2I start, out Vector2I end);
        ClearPreview();
        if (valid)
        {
            EmitSignal(SignalName.WordSelected, start, end);
        }
    }

    /// <summary>Termina la selección; si el dedo suelta fuera del tablero, la cancela.</summary>
    public void EndSelection(Vector2 localPosition)
    {
        if (!_selection.IsActive)
        {
            return;
        }

        if (!TryGetCell(localPosition, out _))
        {
            CancelSelection();
            return;
        }

        EndSelection();
    }

    /// <summary>Cancela la selección actual sin emitir señal.</summary>
    public void CancelSelection()
    {
        _selection.Cancel();
        ClearPreview();
    }

    /// <summary>Desvanece una selección inválida durante <paramref name="seconds"/> sin penalizar.</summary>
    public void FlashInvalid(IReadOnlyList<Vector2I> cells, float seconds = WordSearchVisuals.InvalidFadeSeconds)
    {
        if (cells == null || cells.Count == 0)
        {
            return;
        }

        KillInvalidTween();
        _invalidCells.Clear();
        foreach (Vector2I coord in cells)
        {
            WordCell cell = GetCell(coord);
            if (cell == null)
            {
                continue;
            }

            _invalidCells.Add(coord);
            cell.SetState(WordCellState.Selected);
        }

        if (_invalidCells.Count == 0)
        {
            return;
        }

        _invalidTween = CreateTween();
        _invalidTween.SetParallel(true);
        foreach (Vector2I coord in _invalidCells)
        {
            _invalidTween.TweenProperty(GetCell(coord), "modulate:a", WordSearchVisuals.InvalidFadeAlpha, seconds);
        }

        _invalidTween.Chain().TweenCallback(Callable.From(RestoreInvalid));
    }

    /// <summary>Marca las celdas de una palabra encontrada (la última encontrada prevalece).</summary>
    public void MarkFound(IReadOnlyList<Vector2I> cells, string word)
    {
        if (cells == null)
        {
            return;
        }

        foreach (Vector2I coord in cells)
        {
            GetCell(coord)?.MarkFound(word);
        }
    }

    /// <summary>Resalta temporalmente la palabra indicada (pista, 3 s).</summary>
    public void FlashHint(string word)
    {
        WordPlacement placement = FindPlacement(word);
        if (placement == null)
        {
            return;
        }

        EnsureHintTimer();
        _hintWord = placement.Word;
        foreach (Vector2I coord in placement.Cells)
        {
            GetCell(coord)?.SetState(WordCellState.Hint);
        }

        _hintTimer.Stop();
        _hintTimer.Start();
    }

    private void EnsureHintTimer()
    {
        if (_hintTimer != null && GodotObject.IsInstanceValid(_hintTimer))
        {
            return;
        }

        _hintTimer = new Timer { Name = "HintTimer", OneShot = true, WaitTime = 3.0 };
        _hintTimer.Timeout += RestoreHint;
        AddChild(_hintTimer);
    }

    private void RestoreHint()
    {
        if (string.IsNullOrEmpty(_hintWord))
        {
            return;
        }

        WordPlacement placement = FindPlacement(_hintWord);
        _hintWord = string.Empty;
        if (placement == null)
        {
            return;
        }

        foreach (Vector2I coord in placement.Cells)
        {
            GetCell(coord)?.RestoreBaseState();
        }
    }

    private WordPlacement FindPlacement(string word)
    {
        return Result?.Placements.FirstOrDefault(
            p => p.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
    }

    // --- Entrada -------------------------------------------------------------

    public override void _GuiInput(InputEvent @event)
    {
        if (_input.Process(@event))
        {
            AcceptEvent();
        }
    }

    private void OnInputBegan(Vector2 localPosition)
    {
        BeginSelection(localPosition);
    }

    private void OnInputUpdated(Vector2 localPosition)
    {
        UpdateSelection(localPosition);
    }

    private void OnInputEnded(Vector2 localPosition)
    {
        // Touch.Release sin selección activa: limpiar estados visuales
        // residuales por si el estado quedó atrapado en Android.
        if (!_selection.IsActive)
        {
            ClearPreview();
        }
        else
        {
            EndSelection(localPosition);
        }
    }

    private void OnInputCanceled()
    {
        CancelSelection();
    }

    // --- Interno -------------------------------------------------------------

    private void EnsureTrace()
    {
        if (_trace != null && GodotObject.IsInstanceValid(_trace))
        {
            return;
        }

        _trace = new WordTraceOverlay { Name = "Trace" };
        AddChild(_trace);
        _trace.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    private void UpdateLayout()
    {
        if (_cells == null || Rows == 0 || Columns == 0 || Size.X <= 0 || Size.Y <= 0)
        {
            return;
        }

        float stepX = (Size.X - (WordSearchVisuals.CellGap * (Columns - 1))) / Columns;
        float stepY = (Size.Y - (WordSearchVisuals.CellGap * (Rows - 1))) / Rows;
        _cellSize = Mathf.Max(WordSearchVisuals.MinCellSize, Mathf.Floor(Mathf.Min(stepX, stepY)));

        float totalWidth = (_cellSize * Columns) + (WordSearchVisuals.CellGap * (Columns - 1));
        float totalHeight = (_cellSize * Rows) + (WordSearchVisuals.CellGap * (Rows - 1));
        _origin = new Vector2(
            Mathf.Floor((Size.X - totalWidth) / 2.0f),
            Mathf.Floor((Size.Y - totalHeight) / 2.0f));

        int fontSize = Mathf.Clamp(
            Mathf.RoundToInt(_cellSize * WordSearchVisuals.CellFontRatio),
            WordSearchVisuals.MinCellFontSize,
            WordSearchVisuals.MaxCellFontSize);
        float step = _cellSize + WordSearchVisuals.CellGap;
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                _cells[y, x].Position = _origin + new Vector2(x * step, y * step);
                _cells[y, x].Size = new Vector2(_cellSize, _cellSize);
                _cells[y, x].SetFontSize(fontSize);
            }
        }
    }

    private void ShowPreview()
    {
        ClearPreview();
        if (!_selection.IsActive)
        {
            return;
        }

        List<Vector2I> path = WordPath.Build(_selection.Start, _selection.End);
        if (path.Count == 0)
        {
            path.Add(_selection.Start);
        }

        foreach (Vector2I coord in path)
        {
            WordCell cell = GetCell(coord);
            if (cell == null)
            {
                continue;
            }

            _preview.Add(coord);
            cell.SetState(WordCellState.Selected);
        }

        if (_selection.IsDirectionLocked && _selection.End != _selection.Start)
        {
            _trace.ShowTrace(GetCellCenter(_selection.Start), GetCellCenter(_selection.End));
        }
        else
        {
            _trace.HideTrace();
        }
    }

    private void ClearPreview()
    {
        foreach (Vector2I coord in _preview)
        {
            GetCell(coord)?.RestoreBaseState();
        }

        _preview.Clear();
        _trace?.HideTrace();
    }

    private void RestoreInvalid()
    {
        foreach (Vector2I coord in _invalidCells)
        {
            WordCell cell = GetCell(coord);
            if (cell == null || !GodotObject.IsInstanceValid(cell))
            {
                continue;
            }

            cell.RestoreBaseState();
            cell.Modulate = Colors.White;
        }

        _invalidCells.Clear();
        _invalidTween = null;
    }

    private void KillInvalidTween()
    {
        if (_invalidTween != null && _invalidTween.IsValid())
        {
            _invalidTween.Kill();
        }

        _invalidTween = null;
        if (_invalidCells.Count > 0)
        {
            RestoreInvalid();
        }
    }
}
