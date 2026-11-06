using ClasicosMenteActiva.core;
using Godot;

/// <summary>
///     Pantalla de ajustes navegable (Fase 4 §4.3).
///     Permite cambiar tamaño de texto y tema, y guarda en <see cref="SaveManager" />.
/// </summary>
public partial class SettingsScreen : Control
{
    private Button _btnBack;
    private SaveManager _saveManager;
    private SceneManager _sceneManager;
    private OptionButton _textSizeOption;
    private OptionButton _themeOption;

    public override void _Ready()
    {
        _sceneManager = GetNode<SceneManager>($"/root/{Constants.AutoloadSceneManager}");
        _saveManager = GetNodeOrNull<SaveManager>("/root/SaveManager");

        _textSizeOption = GetNode<OptionButton>("%TextSizeOption");
        _themeOption = GetNode<OptionButton>("%ThemeOption");
        _btnBack = GetNode<Button>("%BtnBack");

        _textSizeOption.AddItem("Normal (20px)", 0);
        _textSizeOption.AddItem("Grande (24px)", 1);
        _textSizeOption.AddItem("Muy Grande (28px)", 2);
        _textSizeOption.ItemSelected += OnTextSizeChanged;

        _themeOption.AddItem("Claro", 0);
        _themeOption.AddItem("Oscuro", 1);
        _themeOption.ItemSelected += OnThemeChanged;

        _btnBack.Pressed += OnBackPressed;

        // Cargar valores guardados
        if (_saveManager != null)
        {
            var size = _saveManager.Settings.TextSize;
            var theme = _saveManager.Settings.Theme;
            _textSizeOption.Selected = size switch
            {
                "normal" => 0,
                "large" => 1,
                _ => 2
            };
            _themeOption.Selected = theme == "dark" ? 1 : 0;
        }

        UpdateLabels();
    }

    private void OnTextSizeChanged(long index)
    {
        var sizeId = index switch
        {
            0 => "normal",
            1 => "large",
            _ => "xlarge"
        };
        if (_saveManager != null)
            _saveManager.Settings.TextSize = sizeId;
        UpdateLabels();
    }

    private void OnThemeChanged(long index)
    {
        var themeId = index == 1 ? "dark" : "light";
        if (_saveManager != null)
            _saveManager.Settings.Theme = themeId;
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        // Los labels se actualizan automáticamente por OptionButton en Godot 4.x
    }

    private void OnBackPressed()
    {
        _saveManager?.SaveSettings();
        _sceneManager.GoTo(Constants.SceneMainMenu);
    }
}