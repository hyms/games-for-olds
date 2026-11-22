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
	private ThemeData _theme = new();
	private int _cornerRadius;
	private BaseButton.DrawMode _lastDrawMode = (BaseButton.DrawMode)(-1);

	/// <summary>Contexto visual del botón (HUD, acción de modal o menú).</summary>
	[Export]
	public PixelButtonVariant ButtonVariant { get; set; } = PixelButtonVariant.Menu;

	/// <summary>Alcance del botón: determina qué paleta de colores usar (global o Word Search).</summary>
	[Export]
	public ButtonScope Scope { get; set; } = ButtonScope.Global;

	/// <summary>Posición del icono respecto al texto del botón.</summary>
	[Export]
	public IconPosition IconPosition { get; set; } = IconPosition.Right;

	/// <summary>True permite alto inferior a MinTouchSize.Y (HUD compacto, ej. 48–52 px).
	/// BigButton respeta esta bandera y no clampa la Y al mínimo táctil.</summary>
	[Export]
	public bool AllowCompactHeight { get; set; }

	/// <summary>Estilo atenuado para tarjetas secundarias del carrusel: relleno más
	/// oscuro, sin alfa. Úsalo con <see cref="RefreshVisuals" /> para activar/desactivar.</summary>
	[Export]
	public bool Muted { get; set; }

	/// <summary>Omite el ancho mínimo de la variante y usa esquina 0: botón cuadrado
	/// compacto para icono único (ej. salida). Default false no cambia comportamiento existente.</summary>
	[Export]
	public bool CompactSquare { get; set; }

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Stop;
		AddThemeConstantOverride("icon_max_width", PixelStyles.ButtonIconSize);

		var theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
		ApplyTheme(theme?.CurrentThemeData);
		if (theme != null) theme.OnThemeChanged += OnThemeChanged;

		ApplyGeometry();
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
		var hl = Scope == ButtonScope.WordSearch ? _theme.WSCremaSuave : (Color?)null;
		var sh = Scope == ButtonScope.WordSearch ? _theme.WSMaderaSombra : (Color?)null;
		PixelReliefPainter.Draw(this, Size, raised, 1, 1, _cornerRadius, hl, sh);
	}

	private void OnThemeChanged(ThemeData themeData) => ApplyTheme(themeData);

	private void ApplyGeometry()
	{
		var metrics = PixelButtonStyle.MetricsFor(ButtonVariant);
		var minSize = metrics.MinSize;

		if (CompactSquare)
		{
			// Botón cuadrado: no se clampa al ancho mínimo de la variante (88 en Hud).
			var current = CustomMinimumSize;
			CustomMinimumSize = new Vector2(
				current.X,
				minSize.Y > 0 ? Mathf.Max(minSize.Y, current.Y) : current.Y);
		}
		else if (minSize.X > 0 || minSize.Y > 0)
		{
			var current = CustomMinimumSize;
			CustomMinimumSize = new Vector2(
				minSize.X > 0 ? Mathf.Max(minSize.X, current.X) : current.X,
				minSize.Y > 0 ? Mathf.Max(minSize.Y, current.Y) : current.Y);
		}

		IconAlignment = IconPosition == IconPosition.Left
			? HorizontalAlignment.Left
			: HorizontalAlignment.Right;
	}

	private void ApplyTheme(ThemeData theme)
	{
		_theme = theme ?? new ThemeData();
		_cornerRadius = CompactSquare ? 0 : PixelButtonStyle.MetricsFor(ButtonVariant).CornerRadius;

		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		var normal = VisualFor(PixelButtonState.Normal);
		var hover = VisualFor(PixelButtonState.Hover);
		var pressed = VisualFor(PixelButtonState.Pressed);
		var focus = VisualFor(PixelButtonState.Focus);
		var disabled = VisualFor(PixelButtonState.Disabled);

		AddThemeStyleboxOverride("normal", normal.Style);
		AddThemeStyleboxOverride("hover", hover.Style);
		AddThemeStyleboxOverride("pressed", pressed.Style);
		AddThemeStyleboxOverride("hover_pressed", pressed.Style);
		AddThemeStyleboxOverride("focus", focus.Style);
		AddThemeStyleboxOverride("disabled", disabled.Style);

		var metrics = PixelButtonStyle.MetricsFor(ButtonVariant);
		if (metrics.Font != null) AddThemeFontOverride("font", metrics.Font);
		AddThemeFontSizeOverride("font_size", metrics.FontSize);
		AddThemeConstantOverride("h_separation", metrics.HSeparation);

		AddThemeColorOverride("font_color", normal.TextColor);
		AddThemeColorOverride("font_hover_color", hover.TextColor);
		AddThemeColorOverride("font_pressed_color", pressed.TextColor);
		AddThemeColorOverride("font_focus_color", focus.TextColor);
		AddThemeColorOverride("font_disabled_color", disabled.TextColor);

		AddThemeColorOverride("icon_normal_color", normal.TextColor);
		AddThemeColorOverride("icon_hover_color", hover.TextColor);
		AddThemeColorOverride("icon_pressed_color", pressed.TextColor);
		AddThemeColorOverride("icon_focus_color", focus.TextColor);
		AddThemeColorOverride("icon_disabled_color", disabled.TextColor);

		_lastDrawMode = GetDrawMode();
		QueueRedraw();
	}

	/// <summary>Vuelve a aplicar los estilos (útil tras cambiar <see cref="Muted" />).</summary>
	public void RefreshVisuals() => UpdateVisuals();

	private PixelButtonVisual VisualFor(PixelButtonState state)
	{
		return Muted
			? PixelButtonStyle.MutedFor(ButtonVariant, state, _theme, Scope)
			: PixelButtonStyle.For(ButtonVariant, state, _theme, Scope);
	}
}
