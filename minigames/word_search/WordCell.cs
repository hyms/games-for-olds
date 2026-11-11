using ClasicosMenteActiva.core;
using ClasicosMenteActiva.scenes.ui.components;
using Godot;

namespace ClasicosMenteActiva.minigames.word_search;

/// <summary>Estados visuales de una celda del tablero (plan Fase 3 §3.1).</summary>
public enum WordCellState
{
    Normal,
    Selected,
    Found,
    Hint
}

/// <summary>
///     Celda del tablero de Sopa de Letras. Hereda de <see cref="PixelCell" />
///     para obtener estilo pixel art puro (sin anti-aliasing, corner_radius = 0)
///     y soporte de temas dinámicos mediante <see cref="ThemeManager.OnThemeChanged" />.
///     Añade la coordenada, la letra y el seguimiento de la palabra que la encontró.
/// </summary>
public partial class WordCell : PixelCell
{
    private static Font _pixelFont;

    /// <summary>Palabra que marcó la celda por última vez (vacío si no se ha encontrado).</summary>
    public string FoundBy { get; private set; } = string.Empty;

    /// <summary>Estado visual específico de Sopa de Letras.</summary>
    public WordCellState WordState { get; private set; } = WordCellState.Normal;

    /// <summary>Fuente pixel compartida por todas las celdas (PixelOperator, spec §1.3).</summary>
    private static Font PixelFont
    {
        get
        {
            if (_pixelFont == null && ResourceLoader.Exists(Constants.FontCellPath))
                _pixelFont = GD.Load<Font>(Constants.FontCellPath);

            return _pixelFont;
        }
    }

    /// <summary>Configura la celda; debe llamarse antes de añadirla a la escena.</summary>
    public new void Setup(Vector2I coords, char letter, int fontSize)
    {
        Coords = coords;
        Letter = letter;
        MouseFilter = MouseFilterEnum.Ignore;
        ClipContents = false;

        // Reutiliza el Label creado por PixelCell (sin duplicar).
        base.Setup(coords, letter, fontSize);
        // Sobreescribe fuente específica del juego después de que base lo haya configurado.
        if (_label != null && PixelFont != null)
            _label.AddThemeFontOverride("font", PixelFont);

        RestoreBaseState();
    }

    /// <summary>Marca la celda como parte de una palabra encontrada (prevalece la última).</summary>
    public void MarkFound(string word)
    {
        IsFound = true;
        FoundBy = word ?? string.Empty;
        WordState = WordCellState.Found;
        State = PixelCellState.Found;
        UpdateVisuals();
    }

    /// <summary>Cambia solo el estado visual (selección, pista o reposo).</summary>
    public void SetState(WordCellState state)
    {
        WordState = state;
        State = state switch
        {
            WordCellState.Normal => PixelCellState.Idle,
            WordCellState.Selected => PixelCellState.Selected,
            WordCellState.Found => PixelCellState.Found,
            WordCellState.Hint => PixelCellState.Hint,
            _ => PixelCellState.Idle
        };
        UpdateVisuals();
    }

    /// <summary>Ajusta el tamaño de la letra al tamaño de la celda (delegado a base).</summary>
    public new void SetFontSize(int size)
    {
        // Delega a PixelCell para mantener un único punto de control de fuente.
        base.SetFontSize(size);
        // Aplica la fuente específica del juego si existe.
        if (_label != null && PixelFont != null)
            _label.AddThemeFontOverride("font", PixelFont);
    }

    /// <summary>Vuelve al estado permanente: encontrada si pertenece a una palabra, si no reposo.</summary>
    public new void RestoreBaseState()
    {
        var found = IsFound;
        WordState = found ? WordCellState.Found : WordCellState.Normal;
        State = found ? PixelCellState.Found : PixelCellState.Idle;
        UpdateVisuals();
    }

    private new void UpdateVisuals()
    {
        // Propagar al estilo base (StyleBox + relieve 3D + opacidad absoluta).
        base.UpdateVisuals();

        if (_label == null) return;

        // El color de la letra sale de la misma fábrica que el relleno, de modo
        // que cada estado del tema mantiene el contraste WCAG (docs §2).
        _label.AddThemeColorOverride("font_color", CurrentVisual.TextColor);
        _label.AddThemeColorOverride("font_outline_color", Colors.White);
        _label.AddThemeConstantOverride("outline_size", 0);
    }
}