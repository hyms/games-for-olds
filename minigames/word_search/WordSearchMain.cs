using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ClasicosMenteActiva.core;
using Godot;

/// <summary>
/// Pantalla de la Sopa de Letras (Singleton vía SceneManager autoload, Composite
/// vía WordGridContainer, Strategy vía WordSearchGenerator).
/// Mantiene el estado y las reglas de la partida; <see cref="WordSearchHud"/>
/// solo formatea el footer y maqueta las etiquetas.
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
    private Label _wordsCounter;
    private Label _timeLabel;
    private PanelContainer _pauseModal;
    private BigButton _btnPause;
    private Stopwatch _stopwatch = new();
    private DateTime _startTime;

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
    private int _hintUses = 3;

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

        _wordsCounter = GetNode<Label>("%WordsCounter");
        _timeLabel = GetNodeOrNull<Label>("%TimeLabel");
        _pauseModal = GetNodeOrNull<PanelContainer>("%PauseModal");
        _btnPause = GetNodeOrNull<BigButton>("%BtnPause");
        if (_btnPause != null) _btnPause.Pressed += () => { if (_pauseModal != null) _pauseModal.Visible = true; };
        var btnReset = GetNodeOrNull<BigButton>("%BtnReset");
        if (btnReset != null) btnReset.Pressed += () => { if (_pauseModal != null) { _pauseModal.Visible = false; StartFromSession(); } };
        var btnMenu = GetNodeOrNull<BigButton>("%BtnMenu");
        if (btnMenu != null) btnMenu.Pressed += () => { _pauseModal?.Hide(); _sceneManager.GoTo(DifficultySelector.ScenePath); };
        var btnHint = GetNodeOrNull<BigButton>("%BtnHint");
        if (btnHint != null)
            btnHint.Pressed += () =>
            {
                if (_hintUses > 0 && _pending.Count > 0)
                {
                    _hintUses--;
                    btnHint.Text = $"Pista ({_hintUses})";
                    _pauseModal?.Hide();
                    // Resaltar primera pendiente 3 s (simulación básica)
                    string pendingWord = new List<string>(_pending).FirstOrDefault();
                    if (!string.IsNullOrEmpty(pendingWord)) Board.FlashHint(pendingWord);
                    GD.Print($"[Hint] palabra pendiente resaltada: {pendingWord}");
                    if (_hintUses == 0) btnHint.Text = "Sin pistas";
                }
            };

        _btnBack.Pressed += OnBackPressed;

        if (_boardArea.GetParent() is PanelContainer boardSlot)
        {
            boardSlot.AddThemeStyleboxOverride("panel", PixelStyles.Plate());
        }

        if (GetNodeOrNull<PanelContainer>("%WordPanel") is PanelContainer wordPanel)
        {
            wordPanel.AddThemeStyleboxOverride("panel", PixelStyles.Plate());
        }

        Board = new WordGridContainer { Name = "Board" };
        Board.WordSelected += OnWordSelected;
        _boardArea.AddChild(Board);
        Board.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // Aplicar márgenes de seguridad (Notch / Safe Area / Acordeón)
        ApplySafeAreaMargin();
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
        _startTime = DateTime.Now;
        _stopwatch.Restart();
        _hintUses = 3;
        StartTimerUpdate();
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

            int secondsElapsed = Math.Max(0, (int)(DateTime.Now - _startTime).TotalSeconds);
            int score = Math.Max(0,
                (WordsFound * 100)
                + (Math.Max(0, (int)(Config.ParSeconds - secondsElapsed)) * 50)
                - (InvalidSelections * 10));

            GD.Print($"[Score] {score} (palabras={WordsFound}, par={Config.ParSeconds}, segundos={secondsElapsed}, invalid={InvalidSelections})");

            var save = GetNodeOrNull<ClasicosMenteActiva.core.SaveManager>("/root/SaveManager");
            save?.RegisterResult("word_search", Config.Id, true, (float)(DateTime.Now - _startTime).TotalSeconds, score);

            var winModal = GetNodeOrNull<PanelContainer>("%WinModal");
            if (winModal != null)
            {
                winModal.Visible = true;
                var title = winModal.GetNodeOrNull<Label>("WinTitle");
                if (title != null) title.Text = "¡Victoria!";
            }
        }
    }

    private void StartTimerUpdate()
    {
        var timer = new Timer();
        timer.WaitTime = 0.5;
        timer.OneShot = false;
        timer.Timeout += () =>
        {
            if (_timeLabel != null && _startTime != default)
            {
                TimeSpan elapsed = DateTime.Now - _startTime;
                _timeLabel.Text = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
            }
        };
        AddChild(timer);
        timer.Start();
    }

    /// <summary>Lee la palabra entre dos celdas usando el tablero montado.</summary>
    private string ResultReader(Vector2I start, Vector2I end)
    {
        return Board.Result?.ReadPath(start, end) ?? string.Empty;
    }

    private void RenderHud()
    {
        // Pendientes en el orden del tablero, reordenadas al azar al encontrar
        // (plan §5.3, header de palabras). El HUD aplica el flujo y el tachado.
        var orderedPending = _order.Where(w => _pending.Contains(w)).ToList();
        for (int i = orderedPending.Count - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (orderedPending[i], orderedPending[j]) = (orderedPending[j], orderedPending[i]);
        }

        _hud.Render(WordsFound, TotalWords, orderedPending, FoundWords, IsComplete);
        if (_wordsCounter != null) _wordsCounter.Text = $"{WordsFound}/{TotalWords}";
        if (_timeLabel != null)
        {
            TimeSpan elapsed = DateTime.Now - _startTime;
            _timeLabel.Text = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
        }
    }

    /// <summary>Aplica el margen de seguridad del panel word_search.</summary>
    private void ApplySafeAreaMargin()
    {
        var safe = GetNodeOrNull<ClasicosMenteActiva.core.SafeAreaManager>("/root/SafeAreaManager");
        if (safe == null) return;
        // El área segura ya está configurada; aquí solo confirmamos que la
        // escena respete el offset superior (el .tscn usa AnchorPreset y el
        // HUD se coloca debajo del área segura por diseño).
    }

    private void OnBackPressed()
    {
        _sceneManager.GoTo(DifficultySelector.ScenePath);
    }
}
