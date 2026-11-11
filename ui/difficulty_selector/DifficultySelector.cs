using System;
using ClasicosMenteActiva.core;
using Godot;
using Godot.Collections;

/// <summary>
///     Selector de dificultad previo a la partida (spec §4.1 · plan Fase 2 §2.3).
///     Cuatro tarjetas grandes con nombre, tablero y objetivo; el grupo de palabras
///     no se elige: lo reparte la Shuffle Bag según la dificultad. Los niveles
///     distintos de Fácil se desbloquean al acumular victorias (Fase 7).
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
        _sceneManager = GetNode<SceneManger>($"/root/{Constants.AutoloadResourceManager}");
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

        foreach (var level in DifficultyCatalog.LoadAll())
        {
            var card = CreateLevelCard(level, completed);
            _levels.AddChild(card);
        }

        if (LevelButtons.Count > 0) LevelButtons[0].GrabFocus();
    }

    private PanelContainer CreateLevelCard(DifficultyConfig level, int completed)
    {
        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(0, WordSearchVisuals.LevelCardMinHeight)
        };

        var vbox = new VBoxContainer
        {
            ThemeOverrideConstants = { Separation = 4 }
        };

        // Level name
        var nameLabel = new Label
        {
            Text = level.MenuLabel,
            AddThemeFontSizeOverride = WordSearchVisuals.LevelCardFontSize
        };

        // Completion tracker
        var required = SaveManager.WinsToUnlockNextLevel;
        var progressLabel = new Label
        {
            Text = $"{completed}/{required}",
            AddThemeFontSizeOverride = WordSearchVisuals.LevelCardSubFontSize
        };

        // Board dimensions
        var boardLabel = new Label
        {
            Text = $"{level.Rows}x{level.Columns}",
            AddThemeFontSizeOverride = WordSearchVisuals.LevelCardSubFontSize
        };

        vbox.AddChild(nameLabel);
        vbox.AddChild(progressLabel);
        vbox.AddChild(boardLabel);

        // Lock state
        var locked = level.Id != DifficultyCatalog.DefaultId && completed < required;
        var requiredValue = required;

        if (locked)
        {
            card.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.5f); // Dimmed alpha
            card.Disabled = true;
        }

        card.Pressed += () => OnLevelPressed(level, locked);
        LevelButtons.Add(card);
        return card;
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