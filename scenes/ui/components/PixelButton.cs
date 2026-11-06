using ClasicosMenteActiva.core;
using Godot;

namespace ClasicosMenteActiva.scenes.ui.components;

/// <summary>
///     Botón táctil universal en pixel art puro. Sin anti-aliasing, sin bordes
///     redondeados: <see cref="StyleBoxFlat" /> con <c>corner_radius = 0</c> y
///     borde recto de 2 píxeles. Soporta temas mediante
///     <see cref="ThemeManager.OnThemeChanged" />.
/// </summary>
[Tool]
public partial class PixelButton : Button
{
    private StyleBoxFlat _styleDisabled;
    private StyleBoxFlat _styleFocus;
    private StyleBoxFlat _styleHover;
    private StyleBoxFlat _styleNormal;
    private StyleBoxFlat _stylePressed;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        BuildStyleBoxes();

        var theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
        ApplyTheme(theme?.CurrentThemeData);
        UpdateVisuals();
    }

    private void BuildStyleBoxes()
    {
        // Pixel art puro: anti_aliasing = false, corner_radius = 0, bordes rectos de 2 px.
        _styleNormal = MakeStyleBox(Color.FromHtml("#A0BFC7"), Color.FromHtml("#101010"), 2);
        _styleHover = MakeStyleBox(Color.FromHtml("#477987"), Color.FromHtml("#101010"), 3);
        _stylePressed = MakeStyleBox(Color.FromHtml("#099C92"), Color.FromHtml("#FFFFFF"), 3);
        _styleFocus = MakeFocusRing();
        _styleDisabled = MakeStyleBox(Color.FromHtml("#4A4A4A"), Color.FromHtml("#101010"), 2);

        _stylePressed.ShadowSize = 0;
        _styleDisabled.ShadowSize = 0;
    }

    private static StyleBoxFlat MakeStyleBox(Color bg, Color border, int borderWidth)
    {
        var box = new StyleBoxFlat
        {
            BgColor = bg,
            BorderColor = border,
            AntiAliasing = false,
            CornerDetail = 1
        };
        box.SetBorderWidthAll(borderWidth);
        box.SetCornerRadiusAll(0);
        return box;
    }

    private static StyleBoxFlat MakeFocusRing()
    {
        var ring = new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 0, 0),
            BorderColor = Color.FromHtml("#101010"),
            DrawCenter = false,
            AntiAliasing = false,
            CornerDetail = 1
        };
        ring.SetBorderWidthAll(3);
        ring.SetCornerRadiusAll(0);
        ring.SetExpandMarginAll(3);
        return ring;
    }

    private void ApplyTheme(ThemeData theme)
    {
        if (theme == null) return;

        _styleNormal.BgColor = theme.HeaderBannerColor;
        _styleNormal.BorderColor = theme.CellIdleTextColor;

        _styleHover.BgColor = theme.SubheaderBannerColor;
        _styleHover.BorderColor = theme.CellIdleTextColor;

        _stylePressed.BgColor = theme.CellFoundBackgroundColor;
        _stylePressed.BorderColor = theme.CellFoundTextColor;

        _styleDisabled.BgColor = Color.FromHtml("#4A4A4A");
        _styleDisabled.BorderColor = theme.CellIdleTextColor;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        AddThemeStyleboxOverride("normal", _styleNormal);
        AddThemeStyleboxOverride("hover", _styleHover);
        AddThemeStyleboxOverride("pressed", _stylePressed);
        AddThemeStyleboxOverride("focus", _styleFocus);
        AddThemeStyleboxOverride("disabled", _styleDisabled);

        AddThemeColorOverride("font_color", Colors.White);
        AddThemeColorOverride("font_hover_color", Colors.White);
        AddThemeColorOverride("font_pressed_color", Colors.White);
        AddThemeColorOverride("font_focus_color", Colors.White);
        AddThemeColorOverride("font_disabled_color", Color.FromHtml("#8A8A8A"));
    }
}