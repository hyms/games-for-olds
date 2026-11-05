using ClasicosMenteActiva.core;
using Godot;

namespace ClasicosMenteActiva.scenes.ui.components;

/// <summary>
/// Celda reutilizable en pixel art puro para grillas de juego (Sopa de Letras,
/// Tangram, lógica). Sin anti-aliasing, sin bordes redondeados orgánicos:
/// <see cref="StyleBoxFlat"/> con <c>corner_radius = 0</c> y bordes rectos de
/// 2 píxeles. Se suscribe a <see cref="ThemeManager.OnThemeChanged"/> para
/// actualizar sus colores cuando el tema cambia globalmente.
/// </summary>
[Tool]
public partial class PixelGridCell : Panel
{
    private Label _label;
    private StyleBoxFlat _styleIdle;
    private StyleBoxFlat _styleFound;
    private StyleBoxFlat _styleSelected;
    private StyleBoxFlat _styleHint;

    /// <summary>Carácter mostrado en la celda.</summary>
    public char Letter { get; protected set; } = ' ';

    /// <summary>Estado visual actual.</summary>
    public PixelCellState State { get; protected set; } = PixelCellState.Idle;

    /// <summary>¿La celda pertenece a una palabra ya encontrada?</summary>
    public bool IsFound { get; protected set; }

    /// <summary>Coordenadas de la celda en la grilla.</summary>
    public Vector2I Coords { get; protected set; }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = false;

        BuildStyleBoxes();

        ThemeManager theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
        ApplyTheme(theme?.CurrentThemeData);
        UpdateVisuals();

        // Suscribirse a cambios de tema para actualizar colores dinámicamente.
        if (theme != null)
        {
            theme.OnThemeChanged += OnThemeChanged;
        }
    }

    public override void _ExitTree()
    {
        ThemeManager theme = GetNodeOrNull<ThemeManager>("/root/ThemeManager");
        if (theme != null)
        {
            theme.OnThemeChanged -= OnThemeChanged;
        }
        base._ExitTree();
    }

    private void OnThemeChanged(ThemeData themeData)
    {
        ApplyTheme(themeData);
    }

    /// <summary>Configura la celda antes de añadirla a la escena.</summary>
    public void Setup(Vector2I coords, char letter, int fontSize)
    {
        Coords = coords;
        Letter = letter;

        _label ??= CreateLabel();
        _label.Text = letter.ToString();
        _label.AddThemeFontSizeOverride("font_size", fontSize);
        UpdateVisuals();
    }

    /// <summary>Marca la celda como encontrada (prevalece el estado Found).</summary>
    public void MarkFound()
    {
        IsFound = true;
        State = PixelCellState.Found;
        UpdateVisuals();
    }

    /// <summary>Cambia el estado visual (Idle / Selected / Hint).</summary>
    public void SetState(PixelCellState state)
    {
        State = state;
        UpdateVisuals();
    }

    /// <summary>Vuelve al estado base (Found si pertenece a una palabra, si no Idle).</summary>
    public void RestoreBaseState()
    {
        State = IsFound ? PixelCellState.Found : PixelCellState.Idle;
        UpdateVisuals();
    }

    /// <summary>Ajusta el tamaño de la letra.</summary>
    public void SetFontSize(int size)
    {
        _label?.AddThemeFontSizeOverride("font_size", size);
    }

    private void BuildStyleBoxes()
    {
        // Pixel art puro: anti_aliasing = false, corner_radius = 0, bordes rectos de 2 px.
        _styleIdle = MakeStyleBox(Color.FromHtml("#EBF2F5"), Color.FromHtml("#A0BFC7"), 2);
        _styleFound = MakeStyleBox(Color.FromHtml("#099C92"), Color.FromHtml("#06706a"), 3);
        _styleSelected = MakeStyleBox(Color.FromHtml("#099C92"), Color.FromHtml("#101010"), 2);
        _styleHint = MakeStyleBox(Color.FromHtml("#EBF2F5"), Color.FromHtml("#8a4600"), 3);
    }

    private static StyleBoxFlat MakeStyleBox(Color bg, Color border, int borderWidth)
    {
        var box = new StyleBoxFlat
        {
            BgColor = bg,
            BorderColor = border,
            AntiAliasing = false,
            CornerDetail = 1,
        };
        box.SetBorderWidthAll(borderWidth);
        box.SetCornerRadiusAll(0);
        return box;
    }

    private void ApplyTheme(ThemeData theme)
    {
        if (theme == null) return;

        _styleIdle.BgColor = theme.CellIdleBackgroundColor;
        _styleIdle.BorderColor = theme.CellIdleBorderColor;

        _styleFound.BgColor = theme.CellFoundBackgroundColor;
        _styleFound.BorderColor = theme.FooterStrikethroughColor;

        _styleSelected.BgColor = theme.CellFoundBackgroundColor;
        _styleSelected.BorderColor = theme.CellIdleTextColor;

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        AddThemeStyleboxOverride("panel", State switch
        {
            PixelCellState.Found => _styleFound,
            PixelCellState.Selected => _styleSelected,
            PixelCellState.Hint => _styleHint,
            _ => _styleIdle,
        });

        if (_label == null) return;

        bool whiteText = State is PixelCellState.Found or PixelCellState.Selected;
        _label.AddThemeColorOverride("font_color", whiteText ? Colors.White : Color.FromHtml("#101010"));
        _label.AddThemeColorOverride("font_outline_color", whiteText ? Color.FromHtml("#06706a") : Colors.White);
        _label.AddThemeConstantOverride("outline_size", whiteText ? 2 : 0);
    }

    private Label CreateLabel()
    {
        var label = new Label
        {
            MouseFilter = MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        AddChild(label);
        label.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        return label;
    }
}

/// <summary>Estados visuales de una celda de píxel.</summary>
public enum PixelCellState
{
    Idle,
    Selected,
    Found,
    Hint,
}
