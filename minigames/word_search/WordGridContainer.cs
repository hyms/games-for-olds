using System;
using System.Collections.Generic;
using Godot;

namespace ClasicosMenteActiva.minigames.word_search;

/// <summary>
///     Tablero de la Sopa de Letras (spec §6 · plan Fase 3 §3.1).
///     Crea y coloca las <see cref="WordCell" />, gestiona la selección por arrastre
///     (con bloqueo de dirección y ajuste a la línea dominante) y emite
///     <c>word_selected(inicio, fin)</c> al soltar.
///     El diseño de la grilla usa métricas fijas de <see cref="WordSearchVisuals" />
///     sin capas de fábrica ni estructuras intermediarias (YAGNI · V1 baseline).
/// </summary>
public partial class WordGridContainer : Control
{
    /// <summary>Se emite al soltar una selección recta de 2 o más celdas.</summary>
    [Signal]
    public delegate void WordSelectedEventHandler(Vector2I start, Vector2I end);

    /// <summary>Se emite cuando cambia el factor de zoom (valor nuevo, 1.0–2.5).</summary>
    [Signal]
    public delegate void ZoomChangedEventHandler(float zoom);

    private readonly List<Vector2I> _invalidCells = new();
    private readonly List<Vector2I> _preview = new();
    private readonly DragSelection _selection = new();

    private WordTraceOverlay _trace;
    private WordCell[,] _cells;
    private float _gap;
    private Timer _hintTimer;
    private string _hintWord = string.Empty;
    private Tween _invalidTween;
    private char[,] _letters;
    private Vector2 _origin;
    private bool _isPressed;

    // --- Zoom (Fase 4) -------------------------------------------------------
    private float _zoom = WordSearchVisuals.MinZoom;

    public int Rows { get; private set; }

    public int Columns { get; private set; }

    public float CellSize { get; private set; }

    /// <summary>Separación entre celdas; fija por dificultad (ver BoardMetricsFor).</summary>
    public float CellGap => _gap;

    public bool IsSelecting => _selection.IsActive;

    /// <summary>Factor de zoom actual del tablero (1.0–2.5).</summary>
    public float Zoom => _zoom;

    /// <summary>Último resultado de generación (para leer palabras).</summary>
    public GenerationResult Result { get; private set; }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        EnsureTrace();
        EnsureHintTimer();
        UpdateLayout();
    }

    private void EnsureTrace()
    {
        // Trace overlay no se destruye entre partidas; vive en el contenedor.
        var existing = FindChild("Trace", true, false);
        if (existing != null) return;
        _trace = new WordTraceOverlay { Name = "Trace" };
        AddChild(_trace);
        _trace.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    private void EnsureHintTimer()
    {
        if (_hintTimer != null && IsInstanceValid(_hintTimer)) return;

        _hintTimer = new Timer { Name = "HintTimer", OneShot = true, WaitTime = WordSearchVisuals.HintFlashSeconds };
        _hintTimer.Timeout += RestoreHint;
        AddChild(_hintTimer);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized) UpdateLayout();
    }

    /// <summary>
    ///     Recalcula la geometría de la grilla y vuelve a aplicar el zoom actual.
    ///     Usado al cambiar el tamaño del contenedor (responsivo) y tras Setup().
    /// </summary>
    private void UpdateLayout()
    {
        ComputeGridLayout();
        PivotOffset = PivotOffset.Center;
        Scale = new Vector2(_zoom, _zoom);
    }

    /// <summary>
    ///     Aplica factor de zoom al tablero (centrado en pivot). El zoom amplía el
    ///     área lógica sin alterar hit-testing: las coordenadas de entrada llegan en
    ///     espacio local (no escalado), así que TryGetCell / GetCellCenter no cambian.
    /// </summary>
    public void SetZoom(float factor)
    {
        var target = Mathf.Clamp(factor, WordSearchVisuals.MinZoom, WordSearchVisuals.MaxZoom);
        if (Mathf.IsEqualApprox(_zoom, target)) return;

        _zoom = target;
        UpdateLayout();
        EmitSignal(SignalName.ZoomChanged, _zoom);
    }

    /// <summary>Restablece el zoom a 1.0 (vista normal).</summary>
    public void ResetZoom() => SetZoom(WordSearchVisuals.MinZoom);

    /// <summary>Incrementa el zoom en un paso (hasta el máximo).</summary>
    public void ZoomIn() => SetZoom(_zoom + WordSearchVisuals.ZoomStep);

    /// <summary>Reduce el zoom en un paso (hasta el mínimo).</summary>
    public void ZoomOut() => SetZoom(_zoom - WordSearchVisuals.ZoomStep);

    /// <summary>
    ///     Monta el tablero con el resultado generado (lo reemplaza si ya había una).
    ///     Fíjala geometría usando BoardMetricsFor y coloca celdas directamente;
    ///     no usa WordGridFactory ni WordGridLayout (YAGNI).
    /// </summary>
    public void Setup(GenerationResult result)
    {
        if (result == null) return;

        CancelSelection();
        KillInvalidTween();

        Result = result;
        Rows = result.Rows;
        Columns = result.Columns;
        _letters = result.Grid;

        MouseFilter = MouseFilterEnum.Stop;

        // Reconstruir la grilla: crear celdas y posicionarlas con métricas directas.
        _cells = new WordCell[Rows, Columns];
        for (var y = 0; y < Rows; y++)
        {
            for (var x = 0; x < Columns; x++)
            {
                var coord = new Vector2I(x, y);
                var cell = new WordCell();
                cell.Setup(coord, result.GetLetter(coord), WordSearchVisuals.DefaultCellFontSize);
                _cells[y, x] = cell;
                AddChild(cell);
            }
        }

        // Geometría de la grilla: calcular tamaño de celda y separación desde
        // BoardMetricsFor y centrar la grilla en el contenedor disponible.
        ComputeGridLayout();

        // Cada partida empieza sin zoom (spec §4: reposición al iniciar/reiniciar).
        _zoom = WordSearchVisuals.MinZoom;
        Scale = Vector2.One;
        PivotOffset = PivotOffset.Center;

        UpdateLayout();
    }

    private void ComputeGridLayout()
    {
        // Usar directamente BoardMetricsFor para la baseline V1 (30×30 celdas, 6 px gap).
        var (preferredCellSize, gap) = WordSearchVisuals.BoardMetricsFor(Rows, Columns);

        // Ajustar al tamaño disponible: reducir solo lo justo para que quepa el tablero entero.
        var totalWidth = preferredCellSize * Columns + gap * (Columns - 1);
        var totalHeight = preferredCellSize * Rows + gap * (Rows - 1);
        var originX = Mathf.Floor((Size.X - totalWidth) / 2.0f);
        var originY = Mathf.Floor((Size.Y - totalHeight) / 2.0f);
        _origin = new Vector2(originX, originY);
        _gap = gap;
        CellSize = preferredCellSize;

        // Aplicar tamaño y posición a cada celda.
        for (var y = 0; y < Rows; y++)
        {
            for (var x = 0; x < Columns; x++)
            {
                var cell = _cells[y, x];
                if (cell == null) continue;
                var cellStep = CellSize + _gap;
                cell.Position = _origin + new Vector2(x * cellStep, y * cellStep);
                cell.Size = new Vector2(CellSize, CellSize);

                // Tamaño de fuente proporcional al ancho de la celda.
                var fontSize = Mathf.Clamp(
                    Mathf.RoundToInt(CellSize * WordSearchVisuals.CellFontRatio),
                    WordSearchVisuals.MinCellFontSize,
                    WordSearchVisuals.MaxCellFontSize);
                cell.SetFontSize(fontSize);
            }
        }
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

    /// <summary>Destella una selección inválida durante <paramref name="seconds" /> sin penalizar.</summary>
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

    private WordPlacement FindPlacement(string word)
    {
        return Result?.Placements.FirstOrDefault(p => p.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
    }

    // --- Interno -------------------------------------------------------------

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

    private void ClearPreview()
    {
        foreach (var coord in _preview) GetCell(coord)?.RestoreBaseState();

        _preview.Clear();
        _trace?.HideTrace();
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

    // --- Entrada directa (sin BoardInputHandler — YAGNI) ---------------------

    public override void _GuiInput(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventScreenTouch touch:
                if (touch.Pressed)
                {
                    _isPressed = true;
                    BeginSelection(touch.Position);
                    AcceptEvent();
                    return;
                }

                if (_isPressed)
                {
                    _isPressed = false;
                    EndSelection(touch.Position);
                    AcceptEvent();
                    return;
                }

                // Release sin pressed previo: estado atrapado, limpiar.
                CancelSelection();
                AcceptEvent();
                return;

            case InputEventScreenDrag drag:
                if (!_isPressed) return;
                UpdateSelection(drag.Position);
                AcceptEvent();
                return;

            case InputEventMouseButton { ButtonIndex: MouseButton.Left } button:
                if (button.Pressed)
                {
                    _isPressed = true;
                    BeginSelection(button.Position);
                    AcceptEvent();
                    return;
                }

                if (_isPressed)
                {
                    _isPressed = false;
                    EndSelection(button.Position);
                    AcceptEvent();
                    return;
                }

                CancelSelection();
                AcceptEvent();
                return;

            case InputEventMouseMotion motion when _isPressed:
                UpdateSelection(motion.Position);
                AcceptEvent();
                return;
        }
    }
}