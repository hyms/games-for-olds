using ClasicosMenteActiva.core;
using Godot;

/// <summary>
/// Pantalla de estadísticas (Fase 4 §4.3).
/// Lee de <see cref="SaveManager.Data.Stats"/> y muestra una tarjeta por juego:
/// nivel actual, mejor tiempo, mejor puntaje, partidas y victorias.
/// Incluye botón para reiniciar estadísticas con confirmación.
/// </summary>
public partial class StatsScreen : Control
{
    [Signal]
    public delegate void BackRequestedEventHandler();

    private SceneManager _sceneManager;
    private SaveManager _saveManager;
    private Button _btnBack;
    private Button _btnReset;
    private VBoxContainer _statsList;
    private Label _emptyLabel;

    public override void _Ready()
    {
        _sceneManager = GetNode<SceneManager>($"/root/{Constants.AutoloadSceneManager}");
        _saveManager = GetNodeOrNull<SaveManager>("/root/SaveManager");

        _btnBack = GetNode<Button>("%BtnBack");
        _btnReset = GetNode<Button>("%BtnReset");
        _statsList = GetNode<VBoxContainer>("%StatsList");
        _emptyLabel = GetNode<Label>("%EmptyLabel");

        _btnBack.Pressed += OnBackPressed;
        _btnReset.Pressed += OnResetPressed;

        Refresh();
    }

    private void Refresh()
    {
        // Limpiar lista (excepto el label vacío)
        foreach (Node child in _statsList.GetChildren())
        {
            if (child is Label) continue;
            child.QueueFree();
        }

        if (_saveManager == null)
        {
            _emptyLabel.Visible = true;
            _btnReset.Disabled = true;
            return;
        }

        _emptyLabel.Visible = false;
        _btnReset.Disabled = false;

        foreach (string gameId in SaveManager.KnownGames)
        {
            GameStats stats = _saveManager.GetStats(gameId);
            if (stats == null) continue;

            var card = CreateStatCard(gameId, stats);
            _statsList.AddChild(card);
        }
    }

    private Control CreateStatCard(string gameId, GameStats stats)
    {
        var panel = new PanelContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            CustomMinimumSize = new Vector2I(300, 120),
        };

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 4);
        panel.AddChild(vbox);

        var title = new Label
        {
            Text = gameId.ToUpper(),
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 24)
        };
        vbox.AddChild(title);

        var row1 = new HBoxContainer();
        row1.AddThemeConstantOverride("separation", 8);
        row1.AddChild(new Label { Text = "Nivel:", CustomMinimumSize = new Vector2I(60, 0) });
        row1.AddChild(new Label { Text = stats.CurrentLevelId, HorizontalAlignment = HorizontalAlignment.Right, SizeFlagsHorizontal = SizeFlags.ExpandFill });
        vbox.AddChild(row1);

        var row2 = new HBoxContainer();
        row2.AddThemeConstantOverride("separation", 8);
        row2.AddChild(new Label { Text = "Partidas:", CustomMinimumSize = new Vector2I(60, 0) });
        row2.AddChild(new Label { Text = stats.Plays.ToString(), HorizontalAlignment = HorizontalAlignment.Right, SizeFlagsHorizontal = SizeFlags.ExpandFill });
        vbox.AddChild(row2);

        var row3 = new HBoxContainer();
        row3.AddThemeConstantOverride("separation", 8);
        row3.AddChild(new Label { Text = "Victorias:", CustomMinimumSize = new Vector2I(60, 0) });
        row3.AddChild(new Label { Text = stats.Wins.ToString(), HorizontalAlignment = HorizontalAlignment.Right, SizeFlagsHorizontal = SizeFlags.ExpandFill });
        vbox.AddChild(row3);

        var row4 = new HBoxContainer();
        row4.AddThemeConstantOverride("separation", 8);
        row4.AddChild(new Label { Text = "Mejor tiempo:", CustomMinimumSize = new Vector2I(60, 0) });
        string bestTime = stats.BestTimeSeconds.HasValue
            ? $"{(int)(stats.BestTimeSeconds.Value / 60):D2}:{stats.BestTimeSeconds.Value % 60:D2}"
            : "—";
        row4.AddChild(new Label { Text = bestTime, HorizontalAlignment = HorizontalAlignment.Right, SizeFlagsHorizontal = SizeFlags.ExpandFill });
        vbox.AddChild(row4);

        var row5 = new HBoxContainer();
        row5.AddThemeConstantOverride("separation", 8);
        row5.AddChild(new Label { Text = "Mejor puntaje:", CustomMinimumSize = new Vector2I(60, 0) });
        row5.AddChild(new Label { Text = stats.BestScore.ToString(), HorizontalAlignment = HorizontalAlignment.Right, SizeFlagsHorizontal = SizeFlags.ExpandFill });
        vbox.AddChild(row5);

        return panel;
    }

    private void OnBackPressed()
    {
        EmitSignal(SignalName.BackRequested);
        _sceneManager.GoTo(Constants.SceneMainMenu);
    }

    private void OnResetPressed()
    {
        // Diálogo de confirmación simple
        var dialog = new AcceptDialog
        {
            Title = "Reiniciar estadísticas",
            DialogText = "¿Estás seguro? Se borrarán todos los niveles, tiempos y puntajes.",
            Size = new Vector2I(300, 120)
        };
        dialog.Confirmed += () =>
        {
            _saveManager?.ResetStats();
            Refresh();
        };
        AddChild(dialog);
        dialog.PopupCentered();
    }
}
