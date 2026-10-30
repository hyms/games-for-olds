using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Godot;

/// <summary>
/// Pantalla de la Sopa de Letras (patrón Template Method · spec §6 · plan
/// Fase 3). Monta el tablero generado, valida la selección por arrastre y
/// mantiene el contador y la lista de palabras.
/// Provisional: el HUD completo, la pausa y el modal de victoria llegan en las
/// fases 5 y 8; el footer de palabras se sustituye por el de la Fase 5.
/// </summary>
public partial class WordSearchMain : Control
{
    /// <summary>Ruta de esta escena (la usa el selector de dificultad).</summary>
    public const string ScenePath = "res://minigames/word_search/WordSearchMain.tscn";

    private readonly HashSet<string> _pending = new();
    private readonly List<string> _foundOrder = new();
    private readonly List<string> _order = new();

    private SceneManager _sceneManager;
    private BigButton _btnBack;
    private Label _counter;
    private Label _difficultyLabel;
    private Label _victoryLabel;
    private RichTextLabel _wordList;
    private Control _boardArea;

    public DifficultyConfig Config { get; private set; }

    public GenerationResult Result { get; private set; }

    public WordGridContainer Board { get; private set; }

    /// <summary>Palabras encontradas en esta partida.</summary>
    public IReadOnlyList<string> FoundWords => _foundOrder;

    /// <summary>Palabras que quedan por encontrar.</summary>
    public IReadOnlyCollection<string> PendingWords => _pending;

    public int WordsFound => _foundOrder.Count;

    public int TotalWords => _order.Count;

    /// <summary>Selecciones descartadas (diagnóstico y pruebas; no penalizan, C4).</summary>
    public int InvalidSelections { get; private set; }

    public bool IsComplete => Result != null && _pending.Count == 0;

    public override void _Ready()
    {
        _sceneManager = GetNode<SceneManager>($"/root/{Constants.AutoloadSceneManager}");
        _btnBack = GetNode<BigButton>("%BtnBack");
        _counter = GetNode<Label>("%WordsCounter");
        _difficultyLabel = GetNode<Label>("%DifficultyLabel");
        _victoryLabel = GetNode<Label>("%VictoryLabel");
        _wordList = GetNode<RichTextLabel>("%WordList");
        _boardArea = GetNode<Control>("%BoardArea");

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
            _difficultyLabel.Text = "No se pudo cargar la dificultad.";
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
        Result = result;
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

        _victoryLabel.Visible = false;
        Board.Setup(result.Grid);
        UpdateHud();
    }

    private void OnWordSelected(Vector2I start, Vector2I end)
    {
        if (Result == null)
        {
            return;
        }

        List<Vector2I> path = WordPath.Build(start, end);
        string word = Result.ReadPath(start, end);
        if (path.Count < 2 || string.IsNullOrEmpty(word))
        {
            InvalidSelections++;
            Board.FlashInvalid(path);
            return;
        }

        if (_pending.Remove(word))
        {
            _foundOrder.Add(word);
            Board.MarkFound(path, word);
            UpdateHud();
            if (_pending.Count == 0)
            {
                _victoryLabel.Visible = true;
                GD.Print($"[WordSearch] completada en {Config.DisplayName}");
            }

            return;
        }

        // No es una palabra pendiente (no existe o ya encontrada): se desvanece
        // sin penalizar y sin tocar el contador (IT-WS-02, C4).
        InvalidSelections++;
        Board.FlashInvalid(path);
    }

    private void UpdateHud()
    {
        _counter.Text = $"{_foundOrder.Count}/{TotalWords}";
        _difficultyLabel.Text = Config?.SummaryLabel ?? string.Empty;

        var text = new StringBuilder();
        foreach (string word in _order)
        {
            if (_pending.Contains(word))
            {
                text.AppendLine(word);
            }
        }

        foreach (string word in _foundOrder)
        {
            text.AppendLine($"[color=#{PixelStyles.EncontradaBorde.ToHtml(false)}][s]{word}[/s][/color]");
        }

        _wordList.Text = text.ToString().TrimEnd();
    }

    private void OnBackPressed()
    {
        _sceneManager.GoTo(DifficultySelector.ScenePath);
    }
}
