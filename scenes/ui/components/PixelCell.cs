#nullable enable
using ClasicosMenteActiva.core;
using Godot;

namespace ClasicosMenteActiva.scenes.ui.components;

/// <summary>
///     Celda reutilizable en pixel art puro para grillas de juego (Sopa de Letras,
///     Tangram, lógica). Reposo y pista elevan el bisel; pulsada, seleccionada y
///     encontrada lo hunden, como un botón físico presionado. El relleno, el borde
///     y los colores de texto proceden de <see cref="PixelCellStyle.For" />, y el
///     bisel de 1 px lo comparte con los botones mediante
///     <see cref="PixelReliefPainter" />. Alpha absoluto en todo momento
///     (<c>Alpha = 1.0</c>, docs/art_direction.md §2).
///     Se suscribe a <see cref="ThemeManager.OnThemeChanged" /> para temas dinámicos.
/// </summary>
[Tool]
public partial class PixelCell : Panel
{
	private readonly PixelCellVisual[] _visuals = new PixelCellVisual[5];
	private ThemeData _theme = new();
	private bool _visualsReady;

	protected Label? _label;
	private Timer? _hintTimer;

	public char Letter { get; protected set; } = ' ';
	public PixelCellState State { get; protected set; } = PixelCellState.Idle;
	public bool IsFound { get; protected set; }
	public Vector2I Coords { get; protected set; }
	public bool IsPressed { get; private set; }

	/// <summary>Estilo y color de texto ya resueltos para <see cref="State" />.</summary>
	protected PixelCellVisual CurrentVisual
	{
		get
		{
			if (!_visualsReady) BuildVisuals();
			return _visuals[(int)State];
		}
	}

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
		ClipContents = false;
		var theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
		ApplyTheme(theme?.CurrentThemeData);
		UpdateVisuals();
		if (theme != null) theme.OnThemeChanged += OnThemeChanged;
	}

	public override void _ExitTree()
	{
		var theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
		if (theme != null) theme.OnThemeChanged -= OnThemeChanged;
		base._ExitTree();
	}

	private void OnThemeChanged(ThemeData themeData) => ApplyTheme(themeData);

	public virtual void Setup(Vector2I coords, char letter, int fontSize)
	{
		Coords = coords;
		Letter = letter;
		_label ??= CreateLabel();
		if (_label != null) _label.Text = letter.ToString();
		if (_label != null) _label.AddThemeFontSizeOverride("font_size", fontSize);
		if (PixelStyles.CellFont != null && _label != null)
			_label.AddThemeFontOverride("font", PixelStyles.CellFont);
		RestoreBaseState();
	}

	public void MarkFound()
	{
		IsFound = true;
		State = PixelCellState.Found;
		IsPressed = false;
		UpdateVisuals();
	}

	public void SetState(PixelCellState state)
	{
		State = state;
		IsPressed = state is PixelCellState.Pressed or PixelCellState.Selected;
		UpdateVisuals();
	}

	public void RestoreBaseState()
	{
		IsPressed = false;
		State = IsFound ? PixelCellState.Found : PixelCellState.Idle;
		UpdateVisuals();
	}

	public void FlashHint(float durationSeconds = WordSearchVisuals.HintFlashSeconds)
	{
		State = PixelCellState.Hint;
		IsPressed = false;
		UpdateVisuals();
		if (_hintTimer == null)
		{
			_hintTimer = new Timer { Name = "HintFlash", WaitTime = durationSeconds, OneShot = true, Autostart = false };
			_hintTimer.Timeout += () => { RestoreBaseState(); _hintTimer = null; };
			AddChild(_hintTimer);
		}
		else
		{
			_hintTimer.Stop();
			_hintTimer.WaitTime = durationSeconds;
		}
		_hintTimer.Start();
	}

	public void SetFontSize(int size) => _label?.AddThemeFontSizeOverride("font_size", size);

	public override void _Draw()
	{
		if (Size.X <= 0 || Size.Y <= 0) return;
		var visual = CurrentVisual;
		if (visual.DrawRelief)
			PixelReliefPainter.Draw(this, Size, visual.Raised,
				WordSearchVisuals.ReliefHighlightWidth, WordSearchVisuals.ReliefShadowWidth);
	}

	private void BuildVisuals()
	{
		for (var i = 0; i < _visuals.Length; i++)
			_visuals[i] = PixelCellStyle.For((PixelCellState)i, _theme);
		_visualsReady = true;
	}

	private void ApplyTheme(ThemeData? theme)
	{
		_theme = theme ?? new ThemeData();
		_visualsReady = false;
		UpdateVisuals();
	}

	protected void UpdateVisuals()
	{
		IsPressed = State is PixelCellState.Pressed or PixelCellState.Selected;
		var visual = CurrentVisual;
		AddThemeStyleboxOverride("panel", visual.Style);

		if (_label != null)
		{
			Modulate = Colors.White;
			_label.Modulate = Colors.White;
			_label.AddThemeColorOverride("font_color", visual.TextColor);
			_label.AddThemeConstantOverride("outline_size", 0);
		}

		QueueRedraw();
	}

	private Label CreateLabel()
	{
		var label = new Label { MouseFilter = MouseFilterEnum.Ignore, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
		AddChild(label);
		label.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		return label;
	}
}
