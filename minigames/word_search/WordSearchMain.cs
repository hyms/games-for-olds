using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClasicosMenteActiva.core;
using ClasicosMenteActiva.minigames.word_search;
using Godot;

/// <summary>
///     Pantalla de la Sopa de Letras (hereda de <see cref="BaseMinigame" /> para la
///     presentación compartida: encabezado, pausa, victoria y navegación).
///     Mantiene el estado y las reglas de la partida; <see cref="WordSearchHud" />
///     solo formatea el footer y maqueta las etiquetas.
/// </summary>
public partial class WordSearchMain : BaseMinigame
{
    /// <summary>Ruta de esta escena (la usa el selector de dificultad).</summary>
    public const string ScenePath = "res://minigames/word_search/WordSearchMain.tscn";

    /// <summary>Pistas disponibles por partida.</summary>
    private const int MaxHints = 3;

    /// <summary>Palabras encontradas, en orden de descubrimiento.</summary>
    private readonly List<string> _foundOrder = new();

    /// <summary>Palabras del tablero, en orden de colocación.</summary>
    private readonly List<string> _order = new();

    /// <summary>Palabras pendientes (orden del tablero).</summary>
    private readonly HashSet<string> _pending = new();

    private Control _boardArea;
    private BigButton _btnHint;
    private BigButton _btnNextGame;
    private BigButton _btnPauseMenu;
    private BigButton _btnReset;

    private WordSearchHud _hud;
    private WordsPanel _wordsPanel;
    private Label _winScore;
    private Label _winTime;
    private int _hintUses = MaxHints;

    public DifficultyConfig Config { get; private set; }

    public WordGridContainer Board { get; private set; }

    public IReadOnlyList<string> FoundWords => _foundOrder;

    public int WordsFound => _foundOrder.Count;

    public int TotalWords => _order.Count;

    /// <summary>Selecciones descartadas (diagnóstico y pruebas; no penalizan, C4).</summary>
    public int InvalidSelections { get; private set; }

    public bool IsComplete => _order.Count > 0 && _pending.Count == 0;

    public override void _Ready()
    {
        base._Ready();

        _boardArea = GetNode<Control>("%BoardArea");
        _winScore = GetNodeOrNull<Label>("%WinScore");
        _winTime = GetNodeOrNull<Label>("%WinTime");
        _btnHint = GetNode<BigButton>("%BtnHint");
        _btnReset = GetNode<BigButton>("%BtnReset");
        _btnNextGame = GetNode<BigButton>("%BtnNextGame");
        _btnPauseMenu = GetNode<BigButton>("%BtnPauseMenu");

        _hud = new WordSearchHud(
            WordsCounter,
            DifficultyLabel,
            GetNodeOrNull<Label>("%VictoryLabel"));

        WireButtons();

        Board = new WordGridContainer { Name = "Board" };
        Board.WordSelected += OnWordSelected;
        _boardArea.AddChild(Board);
        Board.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // Panel de palabras: componente WordsPanel de la escena (marco retro,
        // tachado dinámico y confeti).
        _wordsPanel = GetNodeOrNull<WordsPanel>("%WordsPanel");

        StartFromSession();
    }

    /// <summary>Aplica el tema a los paneles de la Sopa y refresca el HUD.</summary>
    protected override void ApplyTheme(ThemeData themeData)
    {
        base.ApplyTheme(themeData);

        if (GetNodeOrNull<PanelContainer>("%PauseModal") is PanelContainer pauseModal)
            pauseModal.AddThemeStyleboxOverride("panel", PixelStyles.ModalBackdrop());
        if (GetNodeOrNull<PanelContainer>("%PauseCard") is PanelContainer pauseCard)
            pauseCard.AddThemeStyleboxOverride("panel", PixelStyles.ModalCard());
        if (GetNodeOrNull<PanelContainer>("%WinModal") is PanelContainer winModal)
            winModal.AddThemeStyleboxOverride("panel", PixelStyles.ModalBackdrop());
        if (GetNodeOrNull<PanelContainer>("%WinCard") is PanelContainer winCard)
            winCard.AddThemeStyleboxOverride("panel", PixelStyles.ModalCard());

        RenderHud();
    }

    /// <summary>Vuelve al selector de dificultad (no al menú principal).</summary>
    protected override void OnBack()
    {
        SceneManager?.GoTo(DifficultySelector.ScenePath);
    }

    private void WireButtons()
    {
        _btnHint.Pressed += UseHint;
        _btnReset.Pressed += StartNewGame;
        _btnNextGame.Pressed += StartNewGame;
        _btnPauseMenu.Pressed += GoToMainMenu;
    }

    /// <summary>Carga el nivel elegido, sortea las palabras y genera el tablero.</summary>
    public void StartFromSession()
    {
        var config = WordSearchSession.ResolveDifficulty();
        if (config == null)
        {
            GD.PushError("[WordSearchMain] no hay ninguna dificultad cargada");
            _hud.Bind(null);
            return;
        }

        var selection = WordSelection.Pick(config, WordBank.LoadAll(), WordSearchSession.Bag);
        var stopwatch = Stopwatch.StartNew();
        var result = new WordSearchGenerator().Generate(config, selection.Words);
        stopwatch.Stop();

        GD.Print($"[WordSearch] {config.Id} {config.GridSize}×{config.GridSize}: "
                 + $"{result.PlacedCount}/{config.TargetWords} palabras, "
                 + $"{result.Attempts} intento(s), {stopwatch.Elapsed.TotalMilliseconds:0.0} ms "
                 + $"(grupos: {string.Join(", ", selection.CategoryIds)})");

        if (!result.MeetsMinimum)
            GD.PushWarning($"[WordSearch] tablero por debajo del mínimo: "
                           + $"{result.PlacedCount} de {result.MinimumRequired}");

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
        _completed = false;

        foreach (var placement in result.Placements)
            if (_pending.Add(placement.Word))
                _order.Add(placement.Word);

        Board.Setup(result);
        _hud.Bind(config);
        Stopwatch.Restart();
        StartClock();
        UpdateHintButton();
        HideModals();
        RenderHud();
        UpdateClock();
    }

    private void OnWordSelected(Vector2I start, Vector2I end)
    {
        if (_order.Count == 0 || IsCompleted) return;

        var path = WordPath.Build(start, end);
        var word = Board.Result?.ReadPath(start, end) ?? string.Empty;
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

        // Confeti p\u00edxel directamente sobre la palabra encontrada en el footer.
        var startCell = Board.GetCell(start);
        if (startCell != null)
            _wordsPanel?.TriggerConfetti(startCell.GlobalPosition);

        if (_pending.Count == 0) CompleteGame();
    }

    private void CompleteGame()
    {
        if (_completed) return;

        _completed = true;
        StopClock();

        var secondsElapsed = Math.Max(0, (int)Math.Ceiling(Stopwatch.Elapsed.TotalSeconds));
        var score = CalculateScore(secondsElapsed);

        GD.Print($"[WordSearch] completada en {Config.DisplayName}");
        GD.Print($"[Score] {score} (palabras={WordsFound}, par={Config.ParSeconds}, "
                 + $"segundos={secondsElapsed}, pistas={MaxHints - _hintUses}, invalid={InvalidSelections})");

        var save = SceneManager?.Save
                   ?? GetNodeOrNull<SaveManager>("/root/SaveManager");
        save?.RegisterResult("word_search", Config.Id, true, secondsElapsed, score);

        if (_winScore != null) _winScore.Text = $"Puntaje: {score}";

        if (_winTime != null) _winTime.Text = $"Tiempo: {FormatSeconds(secondsElapsed)}";

        if (WinModal != null)
        {
            WinModal.Visible = true;
            _btnNextGame.GrabFocus();
        }
    }

    private int CalculateScore(int secondsElapsed)
    {
        var multiplier = Config.Id switch
        {
            "medio" => 1.25f,
            "dificil" => 1.5f,
            "maestro" => 2.0f,
            _ => 1.0f
        };

        var hintsUsed = MaxHints - _hintUses;
        var baseScore = WordsFound * 100
                        + Math.Max(0, Config.ParSeconds - secondsElapsed)
                        - hintsUsed * 50;

        return Math.Max(0, Mathf.RoundToInt(baseScore * multiplier));
    }

    /// <summary>Refresca contador, lista y aviso de victoria del HUD.</summary>
    private void RenderHud()
    {
        // El HUD se construye al final de _Ready (tras base._Ready, que puede
        // disparar ApplyTheme); si aún no existe, no hay nada que pintar.
        if (_hud == null) return;

        // Pendientes en el orden del tablero, reordenadas al azar al encontrar
        // (plan §5.3, header de palabras). El HUD aplica el flujo y el tachado.
        var orderedPending = _order.Where(w => _pending.Contains(w)).ToList();
        for (var i = orderedPending.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (orderedPending[i], orderedPending[j]) = (orderedPending[j], orderedPending[i]);
        }

        _hud.Render(WordsFound, TotalWords, IsCompleted);
        _wordsPanel?.SetWords(orderedPending, FoundWords, ThemeMgr?.CurrentThemeData);
        UpdateClock();
    }

    private void UseHint()
    {
        if (_hintUses <= 0 || _pending.Count == 0) return;

        var pendingWord = _pending.FirstOrDefault();
        if (string.IsNullOrEmpty(pendingWord)) return;

        _hintUses--;
        UpdateHintButton();
        ClosePause();
        Board.FlashHint(pendingWord);
        GD.Print($"[Hint] palabra pendiente resaltada: {pendingWord}");
    }

    private void UpdateHintButton()
    {
        if (_btnHint == null) return;
        _btnHint.Text = _hintUses > 0 ? $"Pista ({_hintUses})" : "Sin pistas";
        _btnHint.Disabled = _hintUses <= 0;
    }

    private void StartNewGame()
    {
        HideModals();
        StartFromSession();
    }
}
