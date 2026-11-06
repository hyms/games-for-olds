using Godot;

/// <summary>
///     Trazo de arrastre dibujado por encima de las celdas (plan Fase 3 §3.2).
///     Es un control transparente que no captura entrada: la línea semitransparente
///     y los nodos de los extremos los pinta <see cref="WordGridContainer" />.
/// </summary>
public partial class WordTraceOverlay : Control
{
    private Vector2 _from;
    private bool _hasTrace;
    private Vector2 _to;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    /// <summary>Muestra la línea entre dos puntos locales al contenedor.</summary>
    public void ShowTrace(Vector2 from, Vector2 to)
    {
        _from = from;
        _to = to;
        _hasTrace = true;
        QueueRedraw();
    }

    /// <summary>Oculta el trazo (fin o cancelación de la selección).</summary>
    public void HideTrace()
    {
        if (!_hasTrace) return;

        _hasTrace = false;
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_hasTrace) return;

        DrawLine(_from, _to, PixelStyles.Trazo, WordSearchVisuals.TraceWidth);

        DrawCircle(_from, WordSearchVisuals.TraceDotRadius, PixelStyles.Trazo);
        DrawCircle(_to, WordSearchVisuals.TraceDotRadius, PixelStyles.Trazo);
    }
}