using ClasicosMenteActiva.core;
using ClasicosMenteActiva.scenes.ui.components;
using Godot;

/// <summary>
///     Botón grande pensado para móviles y adultos mayores. Hereda de
///     <see cref="PixelButton" /> para recibir el relieve 3D y la tipografía
///     compartida, y añade el tamaño táctil mínimo y los grupos de accesibilidad.
///     Equivalente C# de <c>ui/components/BigButton.gd</c>.
/// </summary>
public partial class BigButton : PixelButton
{
    /// <summary>Píxeles extra para ampliar el área táctil (0 = ninguna).</summary>
    [Export]
    public float GrowHitbox { get; set; }

    public override void _Ready()
    {
        base._Ready();

        var minimum = CustomMinimumSize;
        CustomMinimumSize = new Vector2(
            Mathf.Max(minimum.X, Constants.MinTouchSize.X),
            Mathf.Max(minimum.Y, Constants.MinTouchSize.Y));

        AddToGroup(Constants.GroupUiFocusable);
        AddToGroup(Constants.GroupTouchable);
    }
}
