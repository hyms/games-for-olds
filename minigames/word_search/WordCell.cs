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
    /// <summary>Fuente en negrita para las letras ya encontradas (Assets/fonts).</summary>
    private const string BoldFontPath = Constants.FontBoldPath;

    private static Font _boldFont;

    public Vector2I Coords { get; private set; }

    public char Letter { get; private set; } = ' ';

    /// <summary>Estado visual actual (puede ser <c>Selected</c> de forma temporal).</summary>
    public WordCellState State { get; private set; } = WordCellState.Normal;

    /// <summary>¿La celda pertenece ya a alguna palabra encontrada?</summary>
    public bool IsFound { get; private set; }

    /// <summary>Palabra que marcó la celda por última vez (vacío si no se ha encontrado).</summary>
    public string FoundBy { get; private set; } = string.Empty;

    private Label _label;

    /// <summary>Negrita del tema, cargada una sola vez (todas las celdas la comparten).</summary>
    private static Font BoldFont
    {
        get
        {
            if (_boldFont == null && ResourceLoader.Exists(BoldFontPath))
            {
                _boldFont = GD.Load<Font>(BoldFontPath);
            }

            return _boldFont;
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
        _label.AddThemeColorOverride("font_color", PixelStyles.Tinta);

        RestoreBaseState();
    }

    /// <summary>Marca la celda como parte de una palabra encontrada (prevalece la última).</summary>
    public void MarkFound(string word)
    {
        IsFound = true;
        FoundBy = word ?? string.Empty;
        State = WordCellState.Found;
        ApplyState(State);
    }

    /// <summary>Cambia solo el estado visual (selección, pista o reposo).</summary>
    public void SetState(WordCellState state)
    {
        State = state;
        ApplyState(State);
    }

    /// <summary>Ajusta el tamaño de la letra al tamaño de la celda.</summary>
    public void SetFontSize(int size)
    {
        _label?.AddThemeFontSizeOverride("font_size", Mathf.Max(WordSearchVisuals.MinCellFontSize, size));
    }

    /// <summary>Vuelve al estado permanente: encontrada si pertenece a una palabra, si no reposo.</summary>
    public void RestoreBaseState()
    {
        State = IsFound ? WordCellState.Found : WordCellState.Normal;
        ApplyState(State);
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

        // La celda encontrada invierte el contraste (letras crema sobre turquesa
        // oscuro) y engorda la letra: no se distingue solo por el color.
        bool found = State == WordCellState.Found;
        Color textColor = found ? PixelStyles.Celda : (State == WordCellState.Selected ? Colors.White : PixelStyles.TextDark);
        _label.AddThemeColorOverride("font_color", textColor);
        if (found && BoldFont != null)
        {
            _label.AddThemeFontOverride("font", BoldFont);
        }
        else
        {
            _label.RemoveThemeFontOverride("font");
        }
    }
}
