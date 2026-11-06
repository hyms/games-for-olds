using ClasicosMenteActiva.core;
using Godot;
using Godot.Collections;

/// <summary>
///     Selector de dificultad previo a la partida (spec §4.1 · plan Fase 2 §2.3).
///     Cuatro botones grandes con nombre, tablero y objetivo; el grupo de palabras
///     no se elige: lo reparte la Shuffle Bag según la dificultad. Los niveles
///     distintos de Fácil se desbloquean al acumular victorias (Fase 7).
/// </summary>
public partial class DifficultySelector : Control
{
    /// <summary>Ruta de esta escena (la usa el menú principal y el tablero).</summary>
    public const string ScenePath = "res://ui/difficulty_selector/DifficultySelector.tscn";

    private const string BigButtonScenePath = "res://ui/components/BigButton.tscn";
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

        var buttonScene = GD.Load<PackedScene>(BigButtonScenePath);
        var save = GetNodeOrNull<SaveManager>("/root/SaveManager");
        var stats = save?.GetStats("word_search");
        var completed = stats?.Wins ?? 0;

        foreach (var level in DifficultyCatalog.LoadAll())
        {
            var button = buttonScene.Instantiate<BigButton>();
            button.Name = $"Level_{level.Id}";
            button.Text = level.MenuLabel;
            button.CustomMinimumSize = new Vector2(0, WordSearchVisuals.LevelButtonMinHeight);
            button.AddThemeFontSizeOverride("font_size", WordSearchVisuals.LevelButtonFontSize);

            var locked = level.Id != DifficultyCatalog.DefaultId
                         && completed < SaveManager.WinsToUnlockNextLevel;
            var required = SaveManager.WinsToUnlockNextLevel;
            button.Disabled = locked;
            button.Text = locked
                ? $"🔒 {level.MenuLabel} ({required}/{required})"
                : $"🔓 {level.MenuLabel} ({completed}/{required})";

            if (locked) button.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));

            button.Pressed += () => OnLevelPressed(level, locked);
            _levels.AddChild(button);
            LevelButtons.Add(button);
        }

        if (LevelButtons.Count > 0) LevelButtons[0].GrabFocus();
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