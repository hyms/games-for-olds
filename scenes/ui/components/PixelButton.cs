using ClasicosMenteActiva.core;
using Godot;

namespace ClasicosMenteActiva.scenes.ui.components;

/// <summary>
///     Botón táctil universal en pixel art puro. Comparte con las celdas el bisel
///     de <see cref="PixelReliefPainter" />: reposo y foco elevados, hover y pulsado
///     hundidos, contenido 2 px más abajo al pulsar y sin sombra dura en esos
///     estados. Los estilos, fuentes y colores proceden de
///     <see cref="PixelButtonStyle.For" />, nunca de valores locales.
///     Se suscribe a <see cref="ThemeManager.OnThemeChanged" /> para refrescar la paleta.
/// </summary>
public partial class PixelButton : Button
{
	private readonly StyleBoxFlat[] _styles = new StyleBoxFlat[5];
	private readonly Color[] _textColors = new Color[5];
	private ThemeData _theme = new();
	private BaseButton.DrawMode _lastDrawMode = (BaseButton.DrawMode)(-1);

	/// <summary>Contexto visual del botón (HUD, acción de modal o menú).</summary>
	[Export]
	public PixelButtonVariant ButtonVariant { get; set; } = PixelButtonVariant.Menu;

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;
		AddThemeConstantOverride("icon_max_width", PixelStyles.ButtonIconSize);

		var theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
		ApplyTheme(theme?.CurrentThemeData);
		if (theme != null) theme.OnThemeChanged += OnThemeChanged;
	}

	public override void _Process(double delta)
	{
		// El bisel debe invertirse en cuanto cambia el estado de dibujo, sin esperar
		// a que otro evento provoque un redraw.
		var mode = GetDrawMode();
		if (mode == _lastDrawMode) return;
		_lastDrawMode = mode;
		QueueRedraw();
	}

	public override void _ExitTree()
	{
		var theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
		if (theme != null) theme.OnThemeChanged -= OnThemeChanged;
		base._ExitTree();
	}

	public override void _Draw()
	{
		if (Size.X <= 0 || Size.Y <= 0) return;
		var mode = GetDrawMode();
		if (mode == BaseButton.DrawMode.Disabled) return;

		var raised = mode is BaseButton.DrawMode.Normal;
		PixelReliefPainter.Draw(this, Size, raised);
	}

	private void OnThemeChanged(ThemeData themeData) => ApplyTheme(themeData);

	private void ApplyTheme(ThemeData theme)
	{
		_theme = theme ?? new ThemeData();

		for (var i = 0; i < _styles.Length; i++)
		{
			var visual = PixelButtonStyle.For(ButtonVariant, (PixelButtonState)i, _theme);
			_styles[i] = visual.Style;
			_textColors[i] = visual.TextColor;
		}

		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		var normal = _styles[(int)PixelButtonState.Normal];
		var hover = _styles[(int)PixelButtonState.Hover];
		var pressed = _styles[(int)PixelButtonState.Pressed];
		var focus = _styles[(int)PixelButtonState.Focus];
		var disabled = _styles[(int)PixelButtonState.Disabled];

		AddThemeStyleboxOverride("normal", normal);
		AddThemeStyleboxOverride("hover", hover);
		AddThemeStyleboxOverride("pressed", pressed);
		AddThemeStyleboxOverride("hover_pressed", pressed);
		AddThemeStyleboxOverride("focus", focus);
		AddThemeStyleboxOverride("disabled", disabled);

		var metrics = PixelButtonStyle.MetricsFor(ButtonVariant);
		if (metrics.Font != null) AddThemeFontOverride("font", metrics.Font);
		AddThemeFontSizeOverride("font_size", metrics.FontSize);
		AddThemeConstantOverride("h_separation", metrics.HSeparation);

		AddThemeColorOverride("font_color", _textColors[(int)PixelButtonState.Normal]);
		AddThemeColorOverride("font_hover_color", _textColors[(int)PixelButtonState.Hover]);
		AddThemeColorOverride("font_pressed_color", _textColors[(int)PixelButtonState.Pressed]);
		AddThemeColorOverride("font_focus_color", _textColors[(int)PixelButtonState.Focus]);
		AddThemeColorOverride("font_disabled_color", _textColors[(int)PixelButtonState.Disabled]);

		AddThemeColorOverride("icon_normal_color", _textColors[(int)PixelButtonState.Normal]);
		AddThemeColorOverride("icon_hover_color", _textColors[(int)PixelButtonState.Hover]);
		AddThemeColorOverride("icon_pressed_color", _textColors[(int)PixelButtonState.Pressed]);
		AddThemeColorOverride("icon_focus_color", _textColors[(int)PixelButtonState.Focus]);
		AddThemeColorOverride("icon_disabled_color", _textColors[(int)PixelButtonState.Disabled]);

		_lastDrawMode = GetDrawMode();
		QueueRedraw();
	}
}
