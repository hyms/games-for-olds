using ClasicosMenteActiva.core;
using Godot;

/// <summary>Estados visuales de una celda del tablero (plan Fase 3 §3.1).</summary>
public enum WordCellState
{
    Normal,
    Selected,
    Found,
    Hint,
}

/// <summary>
/// Celda del tablero: letra, coordenada y estado visual. Es un <see cref="Panel"/>
/// (no captura entrada: el arrastre lo gestiona <see cref="WordGridContainer"/>).
/// Una celda puede pertenecer a varias palabras; guarda la última que la marcó
/// (<see cref="FoundBy"/>) y prevalece su estilo (plan Fase 3 §3.1).
/// </summary>
public partial class WordCell : Panel
{
    private static Font _pixelFont;

    public Vector2I Coords { get; private set; }

    public char Letter { get; private set; } = ' ';

    /// <summary>Estado visual actual (puede ser <c>Selected</c> de forma temporal).</summary>
    public WordCellState State { get; private set; } = WordCellState.Normal;

    /// <summary>¿La celda pertenece ya a alguna palabra encontrada?</summary>
    public bool IsFound { get; private set; }

    /// <summary>Palabra que marcó la celda por última vez (vacío si no se ha encontrado).</summary>
    public string FoundBy { get; private set; } = string.Empty;

    private Label _label;

    /// <summary>Fuente pixel compartida por todas las celdas (Press Start 2P).</summary>
    private static Font PixelFont
    {
        get
        {
            if (_pixelFont == null && ResourceLoader.Exists(Constants.FontPixelPath))
            {
                _pixelFont = GD.Load<Font>(Constants.FontPixelPath);
            }

            return _pixelFont;
        }
    }

    /// <summary>Configura la celda; debe llamarse antes de añadirla a la escena.</summary>
    public void Setup(Vector2I coords, char letter, int fontSize)
    {
        Coords = coords;
        Letter = letter;
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = false;

        _label ??= CreateLabel();
        _label.Text = letter.ToString();
        _label.AddThemeFontSizeOverride("font_size", Mathf.Max(WordSearchVisuals.MinCellFontSize, fontSize));
        if (PixelFont != null)
        {
            _label.AddThemeFontOverride("font", PixelFont);
        }

        RestoreBaseState();
    }

    /// <summary>Marca la celda como parte de una palabra encontrada (prevalece la última).</summary>
    public void MarkFound(string word)
    {
        IsFound = true;
        FoundBy = word ?? string.Empty;
        ApplyState(WordCellState.Found);
    }

    /// <summary>Cambia solo el estado visual (selección, pista o reposo).</summary>
    public void SetState(WordCellState state)
    {
        ApplyState(state);
    }

    /// <summary>Ajusta el tamaño de la letra al tamaño de la celda.</summary>
    public void SetFontSize(int size)
    {
        _label?.AddThemeFontSizeOverride("font_size", Mathf.Max(WordSearchVisuals.MinCellFontSize, size));
    }

    /// <summary>Vuelve al estado permanente: encontrada si pertenece a una palabra, si no reposo.</summary>
    public void RestoreBaseState()
    {
        ApplyState(IsFound ? WordCellState.Found : WordCellState.Normal);
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
        label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        return label;
    }

    public void ApplyState(WordCellState state)
    {
        State = state;
        StyleBoxFlat style = State switch
        {
            WordCellState.Selected => PixelStyles.CellSelected(),
            WordCellState.Found => PixelStyles.CellFound(),
            WordCellState.Hint => PixelStyles.CellHint(),
            _ => PixelStyles.CellNormal(),
        };
        AddThemeStyleboxOverride("panel", style);

        if (_label == null)
        {
            return;
        }

        // Reposo: tinta oscura sobre celda clara. Selección y acierto: texto
        // blanco puro sobre el relleno de acento (nunca negro).
        bool whiteText = State is WordCellState.Found or WordCellState.Selected;
        _label.AddThemeColorOverride(
            "font_color",
            whiteText ? PixelStyles.TextLight : PixelStyles.TextDark);

        // El texto blanco lleva contorno turquesa oscuro para reforzar el
        // contraste y distinguir el estado por forma, no solo por color.
        _label.AddThemeColorOverride(
            "font_outline_color",
            whiteText ? PixelStyles.EncontradaBorde : PixelStyles.TextLight);
        _label.AddThemeConstantOverride("outline_size", whiteText ? 2 : 0);
    }
}
