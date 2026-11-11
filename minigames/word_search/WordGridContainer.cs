using System;
using System.Collections.Generic;
using System.Linq;
using ClasicosMenteActiva.minigames.word_search;
using Godot;

/// <summary>
///     Tablero de la Sopa de Letras (patrón Composite · spec §6 · plan Fase 3 §3.1).
///     Crea y coloca las <see cref="WordCell" />, gestiona la selección por arrastre
///     (con bloqueo de dirección y ajuste a la línea dominante) y emite
///     <c>word_selected(inicio, fin)</c> al soltar.
/// </summary>
public partial class WordGridContainer : Control
{
    /// <summary>Se emite al soltar una selección recta de 2 o más celdas.</summary>
    [Signal]
    public delegate void WordSelectedEventHandler(Vector2I start, Vector2I end);

    private readonly BoardInputHandler _input = new();
    private readonly List<Vector2I> _invalidCells = new();
    private readonly List<Vector2I> _preview = new();

    private readonly DragSelection _selection = new();

    private WordCell[,] _cells;
    private float _gap;
    private Timer _hintTimer;
    private string _hintWord = string.Empty;
    private Tween _invalidTween;
    private char[,] _letters;
    private Vector2 _origin;
    private WordTraceOverlay _trace;

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public float CellSize { get; private set; }

    /// <summary>Separación entre celdas del layout actual (la fija <see cref="WordGridFactory" />).</summary>
    public float CellGap => _gap;

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
        if (what == NotificationResized) UpdateLayout();
    }

    /// <summary>Monta el tablero con el resultado generado (la reemplaza si ya había una).</summary>
    public void Setup(GenerationResult result)
    {
        if (result == null) return;

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

        foreach (var child in GetChildren())
        {
            // El trazo y el temporizador de pistas sobreviven a cada partida.
            if (child == _trace || child == _hintTimer) continue;

            RemoveChild(child);
            child.QueueFree();
        }

        _cells = WordGridFactory.CreateCells(result, WordSearchVisuals.DefaultCellFontSize);
        for (var y = 0; y < Rows; y++)
        for (var x = 0; x < Columns; x++)
            if (_cells[y, x] != null) AddChild(_cells[y, x]);

        UpdateLayout();
    }

    /// <summary>Celda en una coordenada; null si está fuera del tablero.</summary>
    public WordCell GetCell(Vector2I coord)
    {
        return IsValidCell(coord) ? _cells[coord.Y, coord.X] : null;
    }

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
        var step = CellSize + _gap;
        return _origin + new Vector2(coord.X * step + CellSize / 2.0f, coord.Y * step + CellSize / 2.0f);
    }

    /// <summary>Rectángulo de una celda en coordenadas locales.</summary>
    public Rect2 GetCellRect(Vector2I coord)
    {
        var step = CellSize + _gap;
        return new Rect2(_origin + new Vector2(coord.X * step, coord.Y * step), new Vector2(CellSize, CellSize));
    }

    /// <summary>Celda bajo un punto local; false si el punto queda fuera del tablero.</summary>
    public bool TryGetCell(Vector2 localPosition, out Vector2I coord)
    {
        coord = Vector2I.Zero;
        if (_letters == null || CellSize <= 0) return false;

        var point = localPosition - _origin;
        if (point.X < 0 || point.Y < 0) return false;

        var step = CellSize + _gap;
        var column = Mathf.FloorToInt(point.X / step);
        var row = Mathf.FloorToInt(point.Y / step);
        if (column < 0 || row < 0 || column >= Columns || row >= Rows) return false;

        coord = new Vector2I(column, row);
        return true;
    }

    // --- Selección por arrastre ---------------------------------------------

    /// <summary>Inicia la selección en el punto local indicado.</summary>
    public void BeginSelection(Vector2 localPosition)
    {
        if (TryGetCell(localPosition, out var coord)) BeginSelectionAt(coord);
    }

    /// <summary>Inicia la selección en una celda concreta.</summary>
    public void BeginSelectionAt(Vector2I coord)
    {
        if (!IsValidCell(coord)) return;

        // Si una selección previa quedó atrapada (p. ej. se perdió el Touch.Release
        // en Android), cancelarla antes de iniciar una nueva.
        if (_selection.IsActive) CancelSelection();

        KillInvalidTween();
        ClearPreview();
        _selection.Begin(coord);
        ShowPreview();
    }

    /// <summary>Actualiza la selección con el punto local bajo el dedo.</summary>
    public void UpdateSelection(Vector2 localPosition)
    {
        if (!_selection.IsActive) return;

        if (!TryGetCell(localPosition, out var coord))
        {
            CancelSelection();
            return;
        }

        UpdateSelectionAt(coord);
    }

    /// <summary>Actualiza la selección con la celda bajo el dedo.</summary>
    public void UpdateSelectionAt(Vector2I coord)
    {
        if (!_selection.IsActive) return;

        _selection.Update(coord, Rows, Columns);
        ShowPreview();
    }

    /// <summary>Termina la selección y emite <c>word_selected</c> si es válida.</summary>
    public void EndSelection()
    {
        if (!_selection.IsActive) return;

        var valid = _selection.TryComplete(out var start, out var end);
        ClearPreview();
        if (valid) EmitSignal(SignalName.WordSelected, start, end);
    }

    /// <summary>Termina la selección; si el dedo suelta fuera del tablero, la cancela.</summary>
    public void EndSelection(Vector2 localPosition)
    {
        if (!_selection.IsActive) return;

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

    /// <summary>Destella una selección inválida durante <paramref name="seconds" /> sin penalizar, respetando opacidad absoluta.</summary>
    public void FlashInvalid(IReadOnlyList<Vector2I> cells, float seconds = WordSearchVisuals.InvalidFlashSeconds)
    {
        if (cells == null || cells.Count == 0) return;

        KillInvalidTween();
        _invalidCells.Clear();
        foreach (var coord in cells)
        {
            var cell = GetCell(coord);
            if (cell == null) continue;

            _invalidCells.Add(coord);
            cell.SetState(WordCellState.Selected);
        }

        if (_invalidCells.Count == 0) return;

        _invalidTween = CreateTween();
        _invalidTween.TweenInterval(seconds);
        _invalidTween.TweenCallback(Callable.From(RestoreInvalid));
    }

    /// <summary>Marca las celdas de una palabra encontrada (la última encontrada prevalece).</summary>
    public void MarkFound(IReadOnlyList<Vector2I> cells, string word)
    {
        if (cells == null) return;

        foreach (var coord in cells) GetCell(coord)?.MarkFound(word);
    }

    /// <summary>Resalta temporalmente la palabra indicada (pista, 1.5 s según spec §4.2).</summary>
    public void FlashHint(string word)
    {
        var placement = FindPlacement(word);
        if (placement == null) return;

        EnsureHintTimer();
        _hintWord = placement.Word;
        foreach (var coord in placement.Cells) GetCell(coord)?.SetState(WordCellState.Hint);

        _hintTimer.Stop();
        _hintTimer.Start();
    }

    private void EnsureHintTimer()
    {
        if (_hintTimer != null && IsInstanceValid(_hintTimer)) return;

        _hintTimer = new Timer { Name = "HintTimer", OneShot = true, WaitTime = WordSearchVisuals.HintFlashSeconds };
        _hintTimer.Timeout += RestoreHint;
        AddChild(_hintTimer);
    }

    private void RestoreHint()
    {
        if (string.IsNullOrEmpty(_hintWord)) return;

        var placement = FindPlacement(_hintWord);
        _hintWord = string.Empty;
        if (placement == null) return;

        foreach (var coord in placement.Cells) GetCell(coord)?.RestoreBaseState();
    }

    private WordPlacement FindPlacement(string word)
    {
        return Result?.Placements.FirstOrDefault(p => p.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
    }

    // --- Entrada -------------------------------------------------------------

    public override void _GuiInput(InputEvent @event)
    {
        if (_input.Process(@event)) AcceptEvent();
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
            ClearPreview();
        else
            EndSelection(localPosition);
    }

    private void OnInputCanceled()
    {
        CancelSelection();
    }

    // --- Interno -------------------------------------------------------------

    private void EnsureTrace()
    {
        if (_trace != null && IsInstanceValid(_trace)) return;

        _trace = new WordTraceOverlay { Name = "Trace" };
        AddChild(_trace);
        _trace.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    private void UpdateLayout()
    {
        if (_cells == null || Rows == 0 || Columns == 0 || Size.X <= 0 || Size.Y <= 0) return;

        var layout = WordGridFactory.ComputeLayout(Rows, Columns, Size);
        CellSize = layout.CellSize;
        if (CellSize <= 0) return;

        _gap = layout.Gap;
        _origin = layout.Origin;

        var fontSize = Mathf.Clamp(
            Mathf.RoundToInt(CellSize * WordSearchVisuals.CellFontRatio),
            WordSearchVisuals.MinCellFontSize,
            WordSearchVisuals.MaxCellFontSize);

        WordGridFactory.PositionCells(_cells, layout, fontSize);
    }

    private void ShowPreview()
    {
        ClearPreview();
        if (!_selection.IsActive) return;

        var path = WordPath.Build(_selection.Start, _selection.End);
        if (path.Count == 0) path.Add(_selection.Start);

        foreach (var coord in path)
        {
            var cell = GetCell(coord);
            if (cell == null) continue;

            _preview.Add(coord);
            cell.SetState(WordCellState.Selected);
        }

        if (_selection.IsDirectionLocked && _selection.End != _selection.Start)
            _trace.ShowTrace(GetCellCenter(_selection.Start), GetCellCenter(_selection.End));
        else
            _trace.HideTrace();
    }

    private void ClearPreview()
    {
        foreach (var coord in _preview) GetCell(coord)?.RestoreBaseState();

        _preview.Clear();
        _trace?.HideTrace();
    }

    private void RestoreInvalid()
    {
        foreach (var coord in _invalidCells)
        {
            var cell = GetCell(coord);
            if (cell == null || !IsInstanceValid(cell)) continue;

            cell.RestoreBaseState();
        }

        _invalidCells.Clear();
        _invalidTween = null;
    }

    private void KillInvalidTween()
    {
        if (_invalidTween != null && _invalidTween.IsValid()) _invalidTween.Kill();

        _invalidTween = null;
        if (_invalidCells.Count > 0) RestoreInvalid();
    }
}