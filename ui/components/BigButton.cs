using Godot;

/// <summary>
/// Botón grande pensado para móviles y adultos mayores.
/// Garantiza <see cref="Constants.MinTouchSize"/> y se registra en los grupos
/// <c>ui_focusable</c> y <c>touchable</c>.
/// Equivalente C# de <c>ui/components/BigButton.gd</c>.
/// </summary>
[Tool]
public partial class BigButton : Button
{
    /// <summary>Píxeles extra para ampliar el área táctil (0 = ninguna).</summary>
    [Export]
    public float GrowHitbox { get; set; }

    public override void _Ready()
    {
        Vector2 minimum = CustomMinimumSize;
        CustomMinimumSize = new Vector2(
            Mathf.Max(minimum.X, Constants.MinTouchSize.X),
            Mathf.Max(minimum.Y, Constants.MinTouchSize.Y));

        AddToGroup(Constants.GroupUiFocusable);
        AddToGroup(Constants.GroupTouchable);

        MouseFilter = MouseFilterEnum.Stop;
    }
}
