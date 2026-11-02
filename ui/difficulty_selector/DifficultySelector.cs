using ClasicosMenteActiva.core;
using Godot;

/// <summary>
/// Selector de dificultad previo a la partida (spec §4.1 · plan Fase 2 §2.3).
/// Cuatro botones grandes con nombre, tablero y objetivo; el grupo de palabras
/// no se elige: lo reparte la Shuffle Bag según la dificultad.
/// Provisional hasta la Fase 7: todos los niveles entran sin candado.
/// </summary>
public partial class DifficultySelector : Control
{
    /// <summary>Ruta de esta escena (la usa el menú principal y el tablero).</summary>
    public const string ScenePath = "res://ui/difficulty_selector/DifficultySelector.tscn";

    private const string BigButtonScenePath = "res://ui/components/BigButton.tscn";

    private SceneManager _sceneManager;
    private VBoxContainer _levels;
    private BigButton _btnBack;

    /// <summary>Botones de nivel creados desde el catálogo (tests y accesibilidad).</summary>
    public Godot.Collections.Array<BigButton> LevelButtons { get; } = new();

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
        foreach (Node child in _levels.GetChildren())
        {
            _levels.RemoveChild(child);
            child.QueueFree();
        }

        LevelButtons.Clear();

        var buttonScene = GD.Load<PackedScene>(BigButtonScenePath);
        var save = GetNodeOrNull<ClasicosMenteActiva.core.SaveManager>("/root/SaveManager");
        var stats = save?.GetStats("word_search");
        int completed = stats?.Wins ?? 0;

        foreach (DifficultyConfig level in DifficultyCatalog.LoadAll())
        {
            BigButton button = buttonScene.Instantiate<BigButton>();
            button.Name = $"Level_{level.Id}";
            button.Text = level.MenuLabel;
            button.CustomMinimumSize = new Vector2(0, WordSearchVisuals.LevelButtonMinHeight);
            button.AddThemeFontSizeOverride("font_size", WordSearchVisuals.LevelButtonFontSize);

            bool locked = level.Id != "facil" && completed < 20;
            button.Disabled = locked;
            button.Text = locked
                ? $"🔒 {level.MenuLabel} (20/20)"
                : $"🔓 {level.MenuLabel} ({completed}/20)";

            if (locked)
            {
                button.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            }

            button.Pressed += () => OnLevelPressed(level, locked);
            _levels.AddChild(button);
            LevelButtons.Add(button);
        }

        if (LevelButtons.Count > 0)
        {
            LevelButtons[0].GrabFocus();
        }
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
