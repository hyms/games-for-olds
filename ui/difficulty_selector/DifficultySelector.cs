using System;
using ClasicosMenteActiva.core;
using Godot;
using Godot.Collections;

/// <summary>
///     Selector de dificultad previo a la partida (spec §4.1 · plan Fase 2 §2.3).
///     Un botón por nivel del catálogo con nombre, dimensiones del tablero y
///     progreso (X/20). Los niveles distintos de Fácil se desbloquean al
///     acumular victorias (Fase 7) y se muestran atenuados y deshabilitados.
/// </summary>
public partial class DifficultySelector : Control
{
    /// <summary>Ruta de esta escena (la usa el menú principal y el tablero).</summary>
    public const string ScenePath = "res://ui/difficulty_selector/DifficultySelector.tscn";

    private BigButton _btnBack;
    private VBoxContainer _levels;

    private SceneManager _sceneManager;

    /// <summary>Botones de nivel creados desde el catálogo (tests y accesibilidad).</summary>
    public Array<BigButton> LevelButtons { get; } = new();

    public override void _Ready()
    {
        _sceneManager = GetNode<SceneManager>($"/root/{Constants.AutoloadSceneManager}");
        _levels = GetNode<VBoxContainer>("%Levels");
        _btnBack = GetNode<BigButton>("%BtnBack");

        _btnBack.Pressed += OnBackPressed;
        BuildLevelButtons();
    }

    private void BuildLevelButtons()
    {
        foreach (var child in _levels.GetChildren())
        {
            _levels.RemoveChild(child);
            child.QueueFree();
        }

        LevelButtons.Clear();

        var save = GetNodeOrNull<SaveManager>("/root/SaveManager");
        var stats = save?.GetStats("word_search");
        var completed = stats?.Wins ?? 0;
        var required = SaveManager.WinsToUnlockNextLevel;

        foreach (var level in DifficultyCatalog.LoadAll())
        {
            var button = InstantiateLevelButton(level, completed, required);
            _levels.AddChild(button);
            LevelButtons.Add(button);
        }

        if (LevelButtons.Count > 0) LevelButtons[0].GrabFocus();
    }

    private BigButton InstantiateLevelButton(DifficultyConfig level, int completed, int required)
    {
        var buttonScene = GD.Load<PackedScene>("res://ui/components/BigButton.tscn");
        var button = buttonScene.Instantiate<BigButton>();
        button.Name = $"Level_{level.Id}";
        button.CustomMinimumSize = new Vector2(0, WordSearchVisuals.LevelButtonMinHeight);
        button.AddThemeFontSizeOverride("font_size", WordSearchVisuals.LevelButtonFontSize);

        var progress = Mathf.Min(completed, required);
        var locked = level.Id != DifficultyCatalog.DefaultId && completed < required;

        button.Text = locked
            ? $"{level.MenuLabel}\n🔒 {progress}/{required}"
            : $"{level.MenuLabel}\n{progress}/{required}";

        if (locked)
        {
            button.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f); // Atenúa niveles bloqueados
            button.Disabled = true;
        }

        button.Pressed += () => OnLevelPressed(level, locked);
        return button;
    }

    private void OnLevelPressed(DifficultyConfig level, bool locked)
    {
        if (locked)
        {
            GD.Print($"[DifficultySelector] nivel bloqueado: {level.Id} (completar 20 para desbloquear)");
            return;
        }

        WordSearchSession.Select(level);
        _sceneManager.GoTo(WordSearchMain.ScenePath);
    }

    private void OnBackPressed()
    {
        _sceneManager.GoToMainMenu();
    }
}