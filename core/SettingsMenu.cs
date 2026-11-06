using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
///     Menú de ajustes (Fase 4 §4.2).
///     Permite al jugador ajustar tamaño de texto y tema de color.
///     Los ajustes se aplican inmediatamente pero no se persisten entre sesiones.
/// </summary>
public partial class SettingsMenu : Control
{
    [Signal]
    public delegate void SettingsChangedEventHandler(string settingId, string value);

    private Button _btnApply;
    private Button _btnBack;

    private string _currentTextSize = "normal";
    private string _currentTheme = "default";

    private Label _textSizeLabel;
    private Label _themeLabel;

    public override void _Ready()
    {
        _textSizeLabel = GetNode<Label>("%TextSizeLabel");
        _themeLabel = GetNode<Label>("%ThemeLabel");
        _btnApply = GetNode<Button>("%BtnApply");
        _btnBack = GetNode<Button>("%BtnBack");

        // Usar valores por defecto (no persistir entre sesiones por simplicidad)
        _currentTextSize = "normal";
        _currentTheme = "default";

        // Conectar señales
        _btnBack.Pressed += OnBackPressed;
        _btnApply.Pressed += OnApplyPressed;

        UpdateUI();
    }

    private void OnBackPressed()
    {
        GetTree().Root.GetNode<Control>("%MainMenu").Show();
        Visible = false;
    }

    private void OnApplyPressed()
    {
        ApplyTextSize(_currentTextSize);
        ApplyTheme(_currentTheme);

        EmitSignal(SignalName.SettingsChanged, _currentTextSize, _currentTheme);

        _textSizeLabel.Text = $"Texto: {_currentTextSize}";
        _themeLabel.Text = $"Tema: {_currentTheme}";
    }

    private void ApplyTextSize(string sizeId)
    {
        _currentTextSize = sizeId;
        GD.Print($"[SettingsMenu] Texto aplicado: {sizeId}");
    }

    private void ApplyTheme(string themeId)
    {
        _currentTheme = themeId;
        GD.Print($"[SettingsMenu] Tema aplicado: {themeId}");
    }

    private void UpdateUI()
    {
        var sizeText = _currentTextSize == "normal" ? "Normal (20px)" :
            _currentTextSize == "large" ? "Grande (24px)" : "Muy Grande (28px)";
        _textSizeLabel.Text = $"Texto: {sizeText}";
        _themeLabel.Text = $"Tema: {_currentTheme}";
    }
}