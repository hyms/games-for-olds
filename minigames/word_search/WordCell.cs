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
    public Vector2I Coords { get; private set; }

    public char Letter { get; private set; } = ' ';

    /// <summary>Estado visual actual (puede ser <c>Selected</c> de forma temporal).</summary>
    public WordCellState State { get; private set; } = WordCellState.Normal;

    /// <summary>¿La celda pertenece ya a alguna palabra encontrada?</summary>
    public bool IsFound { get; private set; }

    /// <summary>Palabra que marcó la celda por última vez (vacío si no se ha encontrado).</summary>
    public string FoundBy { get; private set; } = string.Empty;

    private Label _label;

    /// <summary>Configura la celda; debe llamarse antes de añadirla a la escena.</summary>
    public void Setup(Vector2I coords, char letter, int fontSize)
    {
        Coords = coords;
        Letter = letter;
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = false;

        _label ??= CreateLabel();
        _label.Text = letter.ToString();
        _label.AddThemeFontSizeOverride("font_size", Mathf.Max(10, fontSize));
        _label.AddThemeColorOverride("font_color", PixelStyles.Tinta);

        RestoreBaseState();
    }

    /// <summary>Marca la celda como parte de una palabra encontrada (prevalece la última).</summary>
    public void MarkFound(string word)
    {
        IsFound = true;
        FoundBy = word ?? string.Empty;
        State = WordCellState.Found;
        ApplyState();
    }

    /// <summary>Cambia solo el estado visual (selección, pista o reposo).</summary>
    public void SetState(WordCellState state)
    {
        State = state;
        ApplyState();
    }

    /// <summary>Ajusta el tamaño de la letra al tamaño de la celda.</summary>
    public void SetFontSize(int size)
    {
        _label?.AddThemeFontSizeOverride("font_size", Mathf.Max(10, size));
    }

    /// <summary>Vuelve al estado permanente: encontrada si pertenece a una palabra, si no reposo.</summary>
    public void RestoreBaseState()
    {
        State = IsFound ? WordCellState.Found : WordCellState.Normal;
        ApplyState();
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

    private void ApplyState()
    {
        StyleBoxFlat style = State switch
        {
            WordCellState.Selected => PixelStyles.CellSelected(),
            WordCellState.Found => PixelStyles.CellFound(),
            WordCellState.Hint => PixelStyles.CellHint(),
            _ => PixelStyles.CellNormal(),
        };
        AddThemeStyleboxOverride("panel", style);
    }
}
