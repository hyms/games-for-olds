using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

/// <summary>
/// Pantalla de la Sopa de Letras (patrón Template Method · spec §6 · plan
/// Fase 3). Solo orquesta: monta el tablero, delega el estado de la partida en
/// <see cref="WordSearchGame"/> y la presentación en <see cref="WordSearchHud"/>.
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

    /// <summary>Estado de la partida (palabras, validación y fallos).</summary>
    public WordSearchGame Game { get; private set; }

    public WordGridContainer Board { get; private set; }

    // Atajos de solo lectura para el HUD y las pruebas de integración.
    public IReadOnlyList<string> FoundWords => Game?.FoundWords ?? Array.Empty<string>();

    public int WordsFound => Game?.FoundCount ?? 0;

    public int TotalWords => Game?.TotalCount ?? 0;

    public int InvalidSelections => Game?.InvalidSelections ?? 0;

    public bool IsComplete => Game?.IsComplete ?? false;

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
        Game = new WordSearchGame(result);

        Board.Setup(result.Grid);
        _hud.Bind(config);
        _hud.Render(Game);
    }

    private void OnWordSelected(Vector2I start, Vector2I end)
    {
        if (Game == null)
        {
            return;
        }

        SelectionResult selection = Game.Select(start, end);
        if (selection.IsFound)
        {
            Board.MarkFound(selection.Cells, selection.Word);
            _hud.Render(Game);
            if (Game.IsComplete)
            {
                GD.Print($"[WordSearch] completada en {Config.DisplayName}");
            }

            return;
        }

        // Inválida o ya encontrada: se desvanece sin penalizar (IT-WS-02, C4).
        Board.FlashInvalid(selection.Cells);
    }

    private void OnBackPressed()
    {
        _sceneManager.GoTo(DifficultySelector.ScenePath);
    }
}
