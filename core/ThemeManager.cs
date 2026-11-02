using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
/// Gestor de temas y accesibilidad visual (plan Fase 3 §3.2 · spec §8.2).
/// Controla tamaño de texto (normal/grande/muy grande) y tema de color.
/// No cambia el viewport ni reduce objetivos táctiles (D4).
/// </summary>
public partial class ThemeManager : Node
{
    [Signal]
    public delegate void ThemeChangedEventHandler(string themeId);

    [Signal]
    public delegate void TextSizeChangedEventHandler(string sizeId);

    public const string SizeNormal = "normal";
    public const string SizeLarge = "large";
    public const string SizeXLarge = "xlarge";

    public const int FontNormal = 20;
    public const int FontLarge = 24;
    public const int FontXLarge = 28;

    private string _currentTheme = "default";
    private string _currentTextSize = SizeNormal;

    public string CurrentTheme => _currentTheme;
    public string CurrentTextSize => _currentTextSize;
    public int CurrentFontSize => _currentTextSize switch
    {
        SizeLarge => FontLarge,
        SizeXLarge => FontXLarge,
        _ => FontNormal,
    };

    public override void _Ready()
    {
        var save = GetNodeOrNull<ClasicosMenteActiva.core.SaveManager>("/root/SaveManager");
        if (save != null)
        {
            _currentTheme = save.Settings.Theme;
            _currentTextSize = save.Settings.TextSize;
        }
    }

    public void SetTheme(string themeId)
    {
        if (string.IsNullOrEmpty(themeId))
        {
            return;
        }

        _currentTheme = themeId;
        var save = GetNodeOrNull<ClasicosMenteActiva.core.SaveManager>("/root/SaveManager");
        if (save != null)
        {
            save.Settings.Theme = themeId;
            save.SaveSettings();
        }

        EmitSignal(global::ClasicosMenteActiva.core.ThemeManager.SignalName.ThemeChanged, _currentTheme);
    }

    public void SetTextSize(string sizeId)
    {
        if (sizeId != SizeNormal && sizeId != SizeLarge && sizeId != SizeXLarge)
        {
            return;
        }

        _currentTextSize = sizeId;
        var save = GetNodeOrNull<ClasicosMenteActiva.core.SaveManager>("/root/SaveManager");
        if (save != null)
        {
            save.Settings.TextSize = sizeId;
            save.SaveSettings();
        }

        EmitSignal(global::ClasicosMenteActiva.core.ThemeManager.SignalName.TextSizeChanged, _currentTextSize);
    }
}