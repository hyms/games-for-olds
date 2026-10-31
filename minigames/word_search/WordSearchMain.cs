using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Godot;

/// <summary>
/// Pantalla de la Sopa de Letras (plan Fase 3, regresada a la segunda opción):
/// `WordSearchMain` mantiene el estado y las reglas de la partida; `WordSearchHud`
/// solo formatea el footer y maqueta las etiquetas. `WordSearchMain` orquesta
/// el ciclo de vida y traduce la entrada del tablero en reglas de juego.
/// Provisional: la pausa y el modal de victoria llegan en las fases 5 y 8.
/// </summary>
public partial class WordSearchMain : Control
{
    /// <summary>Ruta de esta escena (la usa el selector de dificultad).</summary>
    public const string ScenePath = "res://minigames/word_search/WordSearchMain.tscn";

    private SceneManager _sceneManager;
    private BigButton _btnBack;
    private Control _boardArea;
    private WordSearchHud _hud;

    public DifficultyConfig Config { get; private set; }

    /// <summary>Palabras pendientes (orden del tablero).</summary>
    private readonly HashSet<string> _pending = new();

    /// <summary>Palabras encontradas, en orden de descubrimiento.</summary>
    private readonly List<string> _foundOrder = new();

    /// <summary>Palabras del tablero, en orden de colocación.</summary>
    private readonly List<string> _order = new();

    public WordGridContainer Board { get; private set; }

    public IReadOnlyList<string> FoundWords => _foundOrder;

    public int WordsFound => _foundOrder.Count;

    public int TotalWords => _order.Count;

    /// <summary>Selecciones descartadas (diagnóstico y pruebas; no penalizan, C4).</summary>
    public int InvalidSelections { get; private set; }

    public bool IsComplete => _order.Count > 0 && _pending.Count == 0;

    public override void _Ready()
    {
        _sceneManager = GetNode<SceneManager>($"/root/{Constants.AutoloadSceneManager}");
        _btnBack = GetNode<BigButton>("%BtnBack");
        _boardArea = GetNode<Control>("%BoardArea");

        _hud = new WordSearchHud(
            GetNode<Label>("%WordsCounter"),
            GetNode<Label>("%DifficultyLabel"),
            GetNode<RichTextLabel>("%WordList"),
            GetNode<Label>("%VictoryLabel"));

        _btnBack.Pressed += OnBackPressed;

        if (_boardArea.GetParent() is PanelContainer boardSlot)
        {
            boardSlot.AddThemeStyleboxOverride("panel", PixelStyles.Plate());
        }

        Board = new WordGridContainer { Name = "Board" };
        Board.WordSelected += OnWordSelected;
        _boardArea.AddChild(Board);
        Board.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        StartFromSession();
    }

    /// <summary>Carga el nivel elegido, sortea las palabras y genera el tablero.</summary>
    public void StartFromSession()
    {
        DifficultyConfig config = WordSearchSession.ResolveDifficulty();
        if (config == null)
        {
            GD.PushError("[WordSearchMain] no hay ninguna dificultad cargada");
            _hud.Bind(null);
            return;
        }

        WordSelectionResult selection = WordSelection.Pick(config, WordBank.LoadAll(), WordSearchSession.Bag);
        var stopwatch = Stopwatch.StartNew();
        GenerationResult result = new WordSearchGenerator().Generate(config, selection.Words);
        stopwatch.Stop();

        GD.Print($"[WordSearch] {config.Id} {config.GridSize}×{config.GridSize}: "
            + $"{result.PlacedCount}/{config.TargetWords} palabras, "
            + $"{result.Attempts} intento(s), {stopwatch.Elapsed.TotalMilliseconds:0.0} ms "
            + $"(grupos: {string.Join(", ", selection.CategoryIds)})");

        if (!result.MeetsMinimum)
        {
            GD.PushWarning($"[WordSearch] tablero por debajo del mínimo: "
                + $"{result.PlacedCount} de {result.MinimumRequired}");
        }

        StartGame(config, result);
    }

    /// <summary>Arranca una partida con un tablero ya generado (también lo usan los tests).</summary>
    public void StartGame(DifficultyConfig config, GenerationResult result)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(result);

        Config = config;

        _pending.Clear();
        _foundOrder.Clear();
        _order.Clear();
        InvalidSelections = 0;

        foreach (WordPlacement placement in result.Placements)
        {
            if (_pending.Add(placement.Word))
            {
                _order.Add(placement.Word);
            }
        }

        Board.Setup(result);
        _hud.Bind(config);
        RenderHud();
    }

    private void OnWordSelected(Vector2I start, Vector2I end)
    {
        if (_order.Count == 0)
        {
            return;
        }

        List<Vector2I> path = WordPath.Build(start, end);
        string word = ResultReader(start, end);
        GD.Print($"[DEBUG] OnWordSelected: start={start} end={end} pathCount={path.Count} word='{word}' pendingContains={_pending.Contains(word)} ResultGrid={Board.Result?.Grid.GetLength(0)}x{Board.Result?.Grid.GetLength(1)}");
        if (path.Count < 2 || string.IsNullOrEmpty(word))
        {
            InvalidSelections++;
            Board.FlashInvalid(path);
            return;
        }

        if (_foundOrder.Contains(word))
        {
            Board.FlashInvalid(path);
            return;
        }

        if (!_pending.Remove(word))
        {
            InvalidSelections++;
            Board.FlashInvalid(path);
            return;
        }

        _foundOrder.Add(word);
        Board.MarkFound(path, word);
        RenderHud();
        if (_pending.Count == 0)
        {
            GD.Print($"[WordSearch] completada en {Config.DisplayName}");
        }
    }

    /// <summary>Lee la palabra entre dos celdas usando el tablero montado.</summary>
    private string ResultReader(Vector2I start, Vector2I end)
    {
        return Board.Result?.ReadPath(start, end) ?? string.Empty;
    }

    private void RenderHud()
    {
        _hud.Render(WordsFound, TotalWords, _pending, FoundWords, IsComplete);
    }

    private void OnBackPressed()
    {
        _sceneManager.GoTo(DifficultySelector.ScenePath);
    }
}
