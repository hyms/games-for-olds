using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClasicosMenteActiva.core;
using Godot;

/// <summary>
/// Pantalla de la Sopa de Letras (Singleton vía SceneManager autoload, Composite
/// vía WordGridContainer, Strategy vía WordSearchGenerator).
/// Mantiene el estado y las reglas de la partida; <see cref="WordSearchHud"/>
/// solo formatea el footer y maqueta las etiquetas. La presentación (tiempo,
/// pausa, pistas y victoria) vive aquí para no mezclar reglas con píxeles.
/// </summary>
public partial class WordSearchMain : Control
{
    /// <summary>Ruta de esta escena (la usa el selector de dificultad).</summary>
    public const string ScenePath = "res://minigames/word_search/WordSearchMain.tscn";

    /// <summary>Pistas disponibles por partida.</summary>
    private const int MaxHints = 3;

    private SceneManager _sceneManager;
    private BigButton _btnBack;
    private BigButton _btnPause;
    private BigButton _btnResume;
    private BigButton _btnHint;
    private BigButton _btnReset;
    private BigButton _btnPauseMenu;
    private BigButton _btnNextGame;
    private BigButton _btnWinMenu;
    private Control _boardArea;
    private WordSearchHud _hud;
    private Label _wordsCounter;
    private Label _timeLabel;
    private Label _winScore;
    private Label _winTime;
    private PanelContainer _pauseModal;
    private PanelContainer _winModal;
    private Timer _clockTimer;
    private readonly Stopwatch _stopwatch = new();
    private bool _completed;
    private int _hintUses = MaxHints;
    private ThemeData _currentThemeData;

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
        _btnPause = GetNode<BigButton>("%BtnPause");
        _btnResume = GetNode<BigButton>("%BtnResume");
        _btnHint = GetNode<BigButton>("%BtnHint");
        _btnReset = GetNode<BigButton>("%BtnReset");
        _btnPauseMenu = GetNode<BigButton>("%BtnPauseMenu");
        _btnNextGame = GetNode<BigButton>("%BtnNextGame");
        _btnWinMenu = GetNode<BigButton>("%BtnWinMenu");
        _boardArea = GetNode<Control>("%BoardArea");
        _wordsCounter = GetNode<Label>("%WordsCounter");
        _timeLabel = GetNode<Label>("%TimeLabel");
        _winScore = GetNodeOrNull<Label>("%WinScore");
        _winTime = GetNodeOrNull<Label>("%WinTime");
        _pauseModal = GetNodeOrNull<PanelContainer>("%PauseModal");
        _winModal = GetNodeOrNull<PanelContainer>("%WinModal");

        _hud = new WordSearchHud(
            _wordsCounter,
            GetNode<Label>("%DifficultyLabel"),
            GetNode<RichTextLabel>("%WordList"),
            GetNodeOrNull<Label>("%VictoryLabel"));

        StylePanels();
        WireButtons();
        CreateClockTimer();

        Board = new WordGridContainer { Name = "Board" };
        Board.WordSelected += OnWordSelected;
        _boardArea.AddChild(Board);
        Board.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // Suscribirse a cambios de tema para actualizar la UI globalmente.
        ThemeManager theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
        if (theme != null)
        {
            _currentThemeData = theme.CurrentThemeData;
            theme.OnThemeChanged += OnThemeChanged;
        }

        StartFromSession();
    }

    private void OnThemeChanged(ThemeData themeData)
    {
        _currentThemeData = themeData;

        // Aplicar colores de tema a banners, paneles y fondo.
        if (GetNodeOrNull<PanelContainer>("%TitlePanel") is PanelContainer titlePanel)
        {
            titlePanel.AddThemeStyleboxOverride("panel", MakeBannerStyle(themeData.HeaderBannerColor));
        }
        if (GetNodeOrNull<PanelContainer>("%SubtitlePanel") is PanelContainer subtitlePanel)
        {
            subtitlePanel.AddThemeStyleboxOverride("panel", MakeBannerStyle(themeData.SubheaderBannerColor));
        }
        if (GetNodeOrNull<PanelContainer>("%WordPanel") is PanelContainer wordPanel)
        {
            wordPanel.AddThemeStyleboxOverride("panel", MakeBannerStyle(themeData.SubheaderBannerColor));
        }

        // Refrescar fondo raíz.
        if (GetNodeOrNull<ColorRect>("Background") is ColorRect bg)
        {
            bg.Color = themeData.BackgroundColor;
        }

        // Refrescar HUD (colores de texto del footer).
        RenderHud();
    }

    private static StyleBoxFlat MakeBannerStyle(Color bgColor)
    {
        var box = new StyleBoxFlat
        {
            BgColor = bgColor,
            BorderColor = Color.FromHtml("#101010"),
            AntiAliasing = false,
            CornerDetail = 1,
        };
        box.SetBorderWidthAll(2);
        box.SetCornerRadiusAll(0);
        return box;
    }

    private void StylePanels()
    {
        StylePanel("%TitlePanel", PixelStyles.TitlePanel());
        StylePanel("%SubtitlePanel", PixelStyles.SubtitlePanel());
        StylePanel("%WordPanel", PixelStyles.WordPanel());
        StylePanel("%PauseModal", PixelStyles.ModalBackdrop());
        StylePanel("%PauseCard", PixelStyles.ModalCard());
        StylePanel("%WinModal", PixelStyles.ModalBackdrop());
        StylePanel("%WinCard", PixelStyles.ModalCard());
    }

    private void StylePanel(string uniquePath, StyleBoxFlat style)
    {
        if (GetNodeOrNull<PanelContainer>(uniquePath) is PanelContainer panel)
        {
            panel.AddThemeStyleboxOverride("panel", style);
        }
    }

    private void WireButtons()
    {
        _btnBack.Pressed += OnBackPressed;
        _btnPause.Pressed += OpenPause;
        _btnResume.Pressed += ClosePause;
        _btnHint.Pressed += UseHint;
        _btnReset.Pressed += StartNewGame;
        _btnPauseMenu.Pressed += GoToMainMenu;
        _btnNextGame.Pressed += StartNewGame;
        _btnWinMenu.Pressed += GoToMainMenu;
    }

    private void CreateClockTimer()
    {
        _clockTimer = new Timer
        {
            Name = "ClockTimer",
            WaitTime = 0.5,
            OneShot = false,
            Autostart = false,
        };
        _clockTimer.Timeout += UpdateClock;
        AddChild(_clockTimer);
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
        _completed = false;
        _hintUses = MaxHints;

        foreach (WordPlacement placement in result.Placements)
        {
            if (_pending.Add(placement.Word))
            {
                _order.Add(placement.Word);
            }
        }

        Board.Setup(result);
        _hud.Bind(config);
        _stopwatch.Restart();
        _clockTimer.Start();
        UpdateHintButton();
        HideModals();
        RenderHud();
        UpdateClock();
    }

    private void OnWordSelected(Vector2I start, Vector2I end)
    {
        if (_order.Count == 0 || _completed)
        {
            return;
        }

        List<Vector2I> path = WordPath.Build(start, end);
        string word = Board.Result?.ReadPath(start, end) ?? string.Empty;
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
            CompleteGame();
        }
    }

    private void CompleteGame()
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        _stopwatch.Stop();
        _clockTimer.Stop();

        int secondsElapsed = Math.Max(0, (int)Math.Ceiling(_stopwatch.Elapsed.TotalSeconds));
        int score = CalculateScore(secondsElapsed);

        GD.Print($"[WordSearch] completada en {Config.DisplayName}");
        GD.Print($"[Score] {score} (palabras={WordsFound}, par={Config.ParSeconds}, "
            + $"segundos={secondsElapsed}, pistas={MaxHints - _hintUses}, invalid={InvalidSelections})");

        SaveManager save = _sceneManager?.Save
            ?? GetNodeOrNull<SaveManager>("/root/SaveManager");
        save?.RegisterResult("word_search", Config.Id, true, secondsElapsed, score);

        if (_winScore != null)
        {
            _winScore.Text = $"Puntaje: {score}";
        }

        if (_winTime != null)
        {
            _winTime.Text = $"Tiempo: {FormatSeconds(secondsElapsed)}";
        }

        if (_winModal != null)
        {
            _winModal.Visible = true;
            _btnNextGame.GrabFocus();
        }
    }

    private int CalculateScore(int secondsElapsed)
    {
        float multiplier = Config.Id switch
        {
            "medio" => 1.25f,
            "dificil" => 1.5f,
            "maestro" => 2.0f,
            _ => 1.0f,
        };

        int hintsUsed = MaxHints - _hintUses;
        int baseScore = (WordsFound * 100)
            + Math.Max(0, Config.ParSeconds - secondsElapsed)
            - (hintsUsed * 50);

        return Math.Max(0, Mathf.RoundToInt(baseScore * multiplier));
    }

    private void UpdateClock()
    {
        if (_timeLabel == null)
        {
            return;
        }

        _timeLabel.Text = FormatSeconds((int)_stopwatch.Elapsed.TotalSeconds);
    }

    private static string FormatSeconds(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    /// <summary>Refresca contador, tiempo y lista de palabras del HUD.</summary>
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

        _hud.RenderWithTheme(WordsFound, TotalWords, orderedPending, FoundWords, IsComplete, _currentThemeData);
        UpdateClock();
    }

    private void OpenPause()
    {
        if (_pauseModal == null || _completed)
        {
            return;
        }

        _stopwatch.Stop();
        _clockTimer.Stop();
        _pauseModal.Visible = true;
        _btnResume.GrabFocus();
    }

    private void ClosePause()
    {
        if (_pauseModal != null)
        {
            _pauseModal.Visible = false;
        }

        if (!_completed)
        {
            _stopwatch.Start();
            _clockTimer.Start();
        }
    }

    private void UseHint()
    {
        if (_hintUses <= 0 || _pending.Count == 0)
        {
            return;
        }

        string pendingWord = _pending.FirstOrDefault();
        if (string.IsNullOrEmpty(pendingWord))
        {
            return;
        }

        _hintUses--;
        UpdateHintButton();
        ClosePause();
        Board.FlashHint(pendingWord);
        GD.Print($"[Hint] palabra pendiente resaltada: {pendingWord}");
    }

    private void UpdateHintButton()
    {
        _btnHint.Text = _hintUses > 0 ? $"Pista ({_hintUses})" : "Sin pistas";
        _btnHint.Disabled = _hintUses <= 0;
    }

    private void StartNewGame()
    {
        HideModals();
        StartFromSession();
    }

    private void HideModals()
    {
        if (_pauseModal != null)
        {
            _pauseModal.Visible = false;
        }

        if (_winModal != null)
        {
            _winModal.Visible = false;
        }
    }

    private void GoToMainMenu()
    {
        HideModals();
        _sceneManager.GoToMainMenu();
    }

    private void OnBackPressed()
    {
        _sceneManager.GoTo(DifficultySelector.ScenePath);
    }
}
