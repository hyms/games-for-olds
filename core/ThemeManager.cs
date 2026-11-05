using System;
using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
/// Gestor de temas y accesibilidad visual (plan Fase 3 §3.2 · spec §8.2).
/// Controla tamaño de texto (normal/grande/muy grande) y tema de color.
/// Emite <see cref="OnThemeChanged"/> con los <see cref="ThemeData"/> del tema activo
/// para que celdas, botones y banners se actualicen en tiempo real sin tocar escenas.
/// No cambia el viewport ni reduce objetivos táctiles (D4).
/// </summary>
public partial class ThemeManager : Node
{
    [Signal]
    public delegate void ThemeChangedEventHandler(string themeId);

    [Signal]
    public delegate void TextSizeChangedEventHandler(string sizeId);

    /// <summary>Evento C# puro notificado con el <see cref="ThemeData"/> del nuevo tema.</summary>
    public event Action<ThemeData> OnThemeChanged;

    public const string SizeNormal = "normal";
    public const string SizeLarge = "large";
    public const string SizeXLarge = "xlarge";

    public const int FontNormal = 20;
    public const int FontLarge = 24;
    public const int FontXLarge = 28;

    private string _currentTheme = "default";
    private string _currentTextSize = SizeNormal;
    private ThemeData _currentThemeData;

    public string CurrentTheme => _currentTheme;
    public string CurrentTextSize => _currentTextSize;
    public ThemeData CurrentThemeData => _currentThemeData;
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

        _currentThemeData = GetThemeData(_currentTheme);
    }

    /// <summary>Devuelve los datos de tema para un id dado, o el tema por defecto si no se encuentra.</summary>
    public ThemeData GetThemeData(string themeId)
    {
        return themeId switch
        {
            "classic" => new ThemeData
            {
                ThemeId = "classic",
                BackgroundColor = Color.FromHtml("#FFFFFF"),
                HeaderBannerColor = Color.FromHtml("#A0BFC7"),
                SubheaderBannerColor = Color.FromHtml("#477987"),
                CellIdleBackgroundColor = Color.FromHtml("#EBF2F5"),
                CellIdleBorderColor = Color.FromHtml("#A0BFC7"),
                CellIdleTextColor = Color.FromHtml("#101010"),
                CellFoundBackgroundColor = Color.FromHtml("#099C92"),
                CellFoundTextColor = Color.FromHtml("#FFFFFF"),
                FooterTextColor = Color.FromHtml("#477987"),
                FooterStrikethroughColor = Color.FromHtml("#099C92"),
            },
            "dark_contrast" => new ThemeData
            {
                ThemeId = "dark_contrast",
                BackgroundColor = Color.FromHtml("#121212"),
                HeaderBannerColor = Color.FromHtml("#2A2A2A"),
                SubheaderBannerColor = Color.FromHtml("#3A3A3A"),
                CellIdleBackgroundColor = Color.FromHtml("#1E1E1E"),
                CellIdleBorderColor = Color.FromHtml("#4A4A4A"),
                CellIdleTextColor = Color.FromHtml("#FFFFFF"),
                CellFoundBackgroundColor = Color.FromHtml("#099C92"),
                CellFoundTextColor = Color.FromHtml("#FFFFFF"),
                FooterTextColor = Color.FromHtml("#B0B0B0"),
                FooterStrikethroughColor = Color.FromHtml("#099C92"),
            },
            _ => new ThemeData { ThemeId = themeId },
        };
    }

    public void SetTheme(string themeId)
    {
        if (string.IsNullOrEmpty(themeId))
        {
            return;
        }

        _currentTheme = themeId;
        _currentThemeData = GetThemeData(themeId);

        var save = GetNodeOrNull<ClasicosMenteActiva.core.SaveManager>("/root/SaveManager");
        if (save != null)
        {
            save.Settings.Theme = themeId;
            save.SaveSettings();
        }

        EmitSignal(SignalName.ThemeChanged, _currentTheme);
        OnThemeChanged?.Invoke(_currentThemeData);
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

        EmitSignal(SignalName.TextSizeChanged, _currentTextSize);
    }
}