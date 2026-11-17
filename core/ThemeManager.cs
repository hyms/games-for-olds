using System;
using Godot;

namespace ClasicosMenteActiva.core;

/// <summary>
///     Gestor de temas y accesibilidad visual (plan Fase 3 §3.2 · spec §8.2).
///     Controla tamaño de texto (normal/grande/muy grande) y tema de color.
///     Emite <see cref="OnThemeChanged" /> con los <see cref="ThemeData" /> del tema activo
///     para que celdas, botones y banners se actualicen en tiempo real sin tocar escenas.
///     No cambia el viewport ni reduce objetivos táctiles (D4).
/// </summary>
public partial class ThemeManager : Node
{
    [Signal]
    public delegate void TextSizeChangedEventHandler(string sizeId);

    [Signal]
    public delegate void ThemeChangedEventHandler(string themeId);

    public const string SizeNormal = "normal";
    public const string SizeLarge = "large";
    public const string SizeXLarge = "xlarge";

    public const int FontNormal = 20;
    public const int FontLarge = 24;
    public const int FontXLarge = 28;

    public string CurrentTheme { get; private set; } = "default";

    public string CurrentTextSize { get; private set; } = SizeNormal;

    public ThemeData CurrentThemeData { get; private set; }

    public int CurrentFontSize => CurrentTextSize switch
    {
        SizeLarge => FontLarge,
        SizeXLarge => FontXLarge,
        _ => FontNormal
    };

    /// <summary>Evento C# puro notificado con el <see cref="ThemeData" /> del nuevo tema.</summary>
    public event Action<ThemeData> OnThemeChanged;

    public override void _Ready()
    {
        var save = GetNodeOrNull<SaveManager>("/root/SaveManager");
        if (save != null)
        {
            CurrentTheme = save.Settings.Theme;
            CurrentTextSize = save.Settings.TextSize;
        }

        CurrentThemeData = GetThemeData(CurrentTheme);
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
                CellIdleBackgroundColor = Color.FromHtml("#F1EDE1"), // WS-Celda (rev. 1.1)
                CellIdleBorderColor = Color.FromHtml("#A17A5B"), // WS-Fondo (rev. 1.1)
                CellIdleTextColor = Color.FromHtml("#101010"),
                CellFoundBackgroundColor = Color.FromHtml("#FD6D52"), // WS-Acento (rev. 1.1)
                CellFoundTextColor = Color.FromHtml("#101010"),
                FooterTextColor = Color.FromHtml("#101010"),
                FooterStrikethroughColor = Color.FromHtml("#9B4E48"), // WS-Tachado (rev. 1.1)
                CelebrationRed = Color.FromHtml("#B45252"),
                WSFondo = Color.FromHtml("#A17A5B"),
                WSMaderaBorde = Color.FromHtml("#3A2A1E"),
                WSCrema = Color.FromHtml("#EDE19E"),
                WSCremaBorde = Color.FromHtml("#C4B476"),
                WSCremaSuave = Color.FromHtml("#F1EFE2"),
                WSCelda = Color.FromHtml("#F1EDE1"),
                WSAcento = Color.FromHtml("#FD6D52"),
                WSEncontradoBorde = Color.FromHtml("#8E3B33"),
                WSCeldaAviso = Color.FromHtml("#B07259"),
                WSCianTitulo = Color.FromHtml("#65B9C4"),
                WSMaderaSombra = Color.FromHtml("#2E1D12"),
                WSTachado = Color.FromHtml("#9B4E48")
            },
            "dark_contrast" => new ThemeData
            {
                ThemeId = "dark_contrast",
                BackgroundColor = Color.FromHtml("#121212"),
                HeaderBannerColor = Color.FromHtml("#2A2A2A"),
                SubheaderBannerColor = Color.FromHtml("#3A3A3A"),
                CellIdleBackgroundColor = Color.FromHtml("#F1EDE1"), // WS-Celda (rev. 1.1)
                CellIdleBorderColor = Color.FromHtml("#A17A5B"), // WS-Fondo (rev. 1.1)
                CellIdleTextColor = Color.FromHtml("#101010"),
                CellFoundBackgroundColor = Color.FromHtml("#FD6D52"), // WS-Acento (rev. 1.1)
                CellFoundTextColor = Color.FromHtml("#101010"),
                FooterTextColor = Color.FromHtml("#101010"),
                FooterStrikethroughColor = Color.FromHtml("#9B4E48"), // WS-Tachado (rev. 1.1)
                CelebrationRed = Color.FromHtml("#B45252"),
                WSFondo = Color.FromHtml("#A17A5B"),
                WSMaderaBorde = Color.FromHtml("#3A2A1E"),
                WSCrema = Color.FromHtml("#EDE19E"),
                WSCremaBorde = Color.FromHtml("#C4B476"),
                WSCremaSuave = Color.FromHtml("#F1EFE2"),
                WSCelda = Color.FromHtml("#F1EDE1"),
                WSAcento = Color.FromHtml("#FD6D52"),
                WSEncontradoBorde = Color.FromHtml("#8E3B33"),
                WSCeldaAviso = Color.FromHtml("#B07259"),
                WSCianTitulo = Color.FromHtml("#65B9C4"),
                WSMaderaSombra = Color.FromHtml("#2E1D12"),
                WSTachado = Color.FromHtml("#9B4E48")
            },
            _ => new ThemeData { ThemeId = themeId }
        };
    }

    public void SetTheme(string themeId)
    {
        if (string.IsNullOrEmpty(themeId)) return;

        CurrentTheme = themeId;
        CurrentThemeData = GetThemeData(themeId);

        var save = GetNodeOrNull<SaveManager>("/root/SaveManager");
        if (save != null)
        {
            save.Settings.Theme = themeId;
            save.SaveSettings();
        }

        EmitSignal(SignalName.ThemeChanged, CurrentTheme);
        OnThemeChanged?.Invoke(CurrentThemeData);
    }

    public void SetTextSize(string sizeId)
    {
        if (sizeId != SizeNormal && sizeId != SizeLarge && sizeId != SizeXLarge) return;

        CurrentTextSize = sizeId;
        var save = GetNodeOrNull<SaveManager>("/root/SaveManager");
        if (save != null)
        {
            save.Settings.TextSize = sizeId;
            save.SaveSettings();
        }

        EmitSignal(SignalName.TextSizeChanged, CurrentTextSize);
    }
}