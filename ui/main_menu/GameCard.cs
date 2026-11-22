using ClasicosMenteActiva.core;
using ClasicosMenteActiva.scenes.ui.components;
using Godot;

/// <summary>
///     Tarjeta de un juego en el carrusel del menú principal: icono resumen,
///     título y acción de lanzamiento. Alterna entre estilo activo (centro) y
///     estilo atenuado (costados); el relieve y los colores salen de
///     <see cref="PixelButtonStyle" /> sin valores de color locales.
/// </summary>
public partial class GameCard : PixelButton
{
    [Export] public string GameId { get; set; } = "";
    [Export] public string GameTitle { get; set; } = "";
    [Export] public Texture2D SummaryIcon { get; set; }
    [Export] public bool Available { get; set; } = true;

    private TextureRect _icon;
    private Label _title;
    private Label _hint;

    public override void _Ready()
    {
        base._Ready();

        ButtonVariant = PixelButtonVariant.Menu;
        Scope = ButtonScope.WordSearch;
        AllowCompactHeight = false;
        Text = "";
        Icon = null;

        AddToGroup(Constants.GroupUiFocusable);
        AddToGroup(Constants.GroupTouchable);

        _icon = GetNodeOrNull<TextureRect>("Content/Icon");
        _title = GetNodeOrNull<Label>("Content/Title");
        _hint = GetNodeOrNull<Label>("Content/Hint");

        if (_icon == null || _title == null || _hint == null)
        {
            GD.PushError("[GameCard] faltan nodos Content/Icon | Content/Title | Content/Hint");
            return;
        }

        ApplyContent();
        ApplyVisuals();
        SetActive(false);
    }

    public void Configure(string id, string title, Texture2D icon, bool available)
    {
        GameId = id;
        GameTitle = title;
        SummaryIcon = icon;
        Available = available;
        ApplyContent();
        ApplyVisuals();
        RefreshVisuals();
    }

    public void SetActive(bool active)
    {
        if (Muted == !active) return;
        Muted = !active;
        RefreshVisuals();
        ApplyLabelColors();
        if (active)
        {
            GrabFocus();
        }
    }

    private void ApplyContent()
    {
        if (_icon != null) _icon.Texture = SummaryIcon;
        if (_title != null) _title.Text = GameTitle;
        if (_hint != null) _hint.Text = Available ? "Jugar" : "Próximamente";
        Disabled = !Available;
    }

    private void ApplyVisuals()
    {
        _icon.CustomMinimumSize = new Vector2(64, 64);
        _icon.MouseFilter = MouseFilterEnum.Ignore;

        _title.AddThemeFontOverride("font", PixelStyles.DisplayFont);
        _title.AddThemeFontSizeOverride("font_size", 14);
        _title.HorizontalAlignment = HorizontalAlignment.Center;

        _hint.AddThemeFontOverride("font", PixelStyles.HudFont);
        _hint.AddThemeFontSizeOverride("font_size", 12);
        _hint.HorizontalAlignment = HorizontalAlignment.Center;

        MouseFilter = MouseFilterEnum.Stop;
        ApplyLabelColors();
    }

    private void ApplyLabelColors()
    {
        if (_title == null || _hint == null) return;

        var fill = Muted
            ? PixelStyles.Opaque(PixelStyles.WSCremaSuave.Darkened(0.35f))
            : PixelStyles.Opaque(PixelStyles.WSCremaSuave);

        _title.AddThemeColorOverride("font_color", PixelStyles.TextColor(fill));
        _hint.AddThemeColorOverride("font_color", PixelStyles.TextColor(fill));
    }
}
