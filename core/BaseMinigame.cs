using System;
using System.Diagnostics;
using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
///     Clase base para los minijuegos. Encapsula la presentación compartida:
///     encabezado (dificultad, contador, tiempo), botón volver/pausa, modales de
///     pausa y victoria, y acceso a los servicios core. No incluye publicidad ni
///     lógica de tablero (SRP · plan Fase 4.1 · plan Sopa Fase 9.1).
///     Cada subclase define su tablero, selección y reglas propias.
/// </summary>
public abstract partial class BaseMinigame : Control
{
    protected SceneManager SceneManager { get; private set; }
    protected ThemeManager ThemeMgr { get; private set; }
    protected SaveManager Save { get; private set; }

    protected Label DifficultyLabel { get; private set; }
    protected Label TimeLabel { get; private set; }
    protected Label WordsCounter { get; private set; }
    protected BigButton BtnBack { get; private set; }
    protected BigButton BtnPause { get; private set; }
    protected BigButton BtnResume { get; private set; }
    protected BigButton BtnWinMenu { get; private set; }

    protected PanelContainer PauseModal { get; private set; }
    protected PanelContainer WinModal { get; private set; }
    protected PanelContainer TitlePanel { get; private set; }
    protected PanelContainer SubtitlePanel { get; private set; }

    private Timer _clockTimer;
    protected readonly Stopwatch Stopwatch = new();
    protected bool _completed;

    public override void _Ready()
    {
        SceneManager = GetNode<SceneManager>($"/root/{Constants.AutoloadSceneManager}");
        ThemeMgr = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
        Save = GetNodeOrNull<SaveManager>("/root/SaveManager");

        TitlePanel = GetNodeOrNull<PanelContainer>("%TitlePanel");
        SubtitlePanel = GetNodeOrNull<PanelContainer>("%SubtitlePanel");
        DifficultyLabel = GetNodeOrNull<Label>("%DifficultyLabel");
        TimeLabel = GetNodeOrNull<Label>("%TimeLabel");
        WordsCounter = GetNodeOrNull<Label>("%WordsCounter");
        BtnBack = GetNodeOrNull<BigButton>("%BtnBack");
        BtnPause = GetNodeOrNull<BigButton>("%BtnPause");
        BtnResume = GetNodeOrNull<BigButton>("%BtnResume");
        BtnWinMenu = GetNodeOrNull<BigButton>("%BtnWinMenu");
        PauseModal = GetNodeOrNull<PanelContainer>("%PauseModal");
        WinModal = GetNodeOrNull<PanelContainer>("%WinModal");

        WireSharedButtons();
        CreateClockTimer();
        ApplyTheme(ThemeMgr?.CurrentThemeData);
        if (ThemeMgr != null)
            ThemeMgr.OnThemeChanged += ApplyTheme;
    }

    private void WireSharedButtons()
    {
        if (BtnBack != null) BtnBack.Pressed += OnBack;
        if (BtnPause != null) BtnPause.Pressed += OpenPause;
        if (BtnResume != null) BtnResume.Pressed += ClosePause;
        if (BtnWinMenu != null) BtnWinMenu.Pressed += GoToMainMenu;
    }

    /// <summary>Acción del botón volver; por defecto vuelve al menú principal.</summary>
    protected virtual void OnBack()
    {
        GoToMainMenu();
    }

    public void GoToMainMenu()
    {
        HideModals();
        SceneManager?.GoToMainMenu();
    }

    protected void OpenPause()
    {
        if (PauseModal == null || _completed) return;
        StopClock();
        PauseModal.Visible = true;
        BtnResume?.GrabFocus();
    }

    protected void ClosePause()
    {
        if (PauseModal != null) PauseModal.Visible = false;
        if (!_completed) StartClock();
    }

    protected void HideModals()
    {
        if (PauseModal != null) PauseModal.Visible = false;
        if (WinModal != null) WinModal.Visible = false;
    }

    /// <summary>Aplica el tema a los paneles compartidos (estilo píxel: sin suavizado, esquinas 0).</summary>
    protected virtual void ApplyTheme(ThemeData themeData)
    {
        if (themeData == null) return;

        if (TitlePanel != null)
            TitlePanel.AddThemeStyleboxOverride("panel", PixelStyles.TitlePanel());
        if (SubtitlePanel != null)
            SubtitlePanel.AddThemeStyleboxOverride("panel", PixelStyles.SubtitlePanel());

        var background = GetNodeOrNull<ColorRect>("Background");
        if (background != null) background.Color = themeData.BackgroundColor;
    }

    private void CreateClockTimer()
    {
        _clockTimer = new Timer
        {
            Name = "ClockTimer",
            WaitTime = 0.5f,
            OneShot = false,
            Autostart = false
        };
        _clockTimer.Timeout += UpdateClock;
        AddChild(_clockTimer);
    }

    protected void StartClock() => _clockTimer.Start();
    protected void StopClock() => _clockTimer.Stop();

    protected void UpdateClock()
    {
        if (TimeLabel == null) return;
        TimeLabel.Text = FormatSeconds((int)Stopwatch.Elapsed.TotalSeconds);
    }

    protected static string FormatSeconds(int totalSeconds)
    {
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    protected void SetWordsCounter(int found, int total)
    {
        if (WordsCounter != null) WordsCounter.Text = $"{found}/{total} Palabras";
    }

    protected bool IsCompleted => _completed;
}
