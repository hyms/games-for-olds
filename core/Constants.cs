using Godot;

/// <summary>
/// Rutas, grupos y medidas compartidas del proyecto.
/// Equivalente C# de <c>core/Constants.gd</c>.
/// </summary>
public static class Constants
{
    /// <summary>Escena del menú principal.</summary>
    public const string SceneMainMenu = "res://ui/main_menu/MainMenu.tscn";

    /// <summary>Grupo de controles con área táctil ampliada.</summary>
    public const string GroupTouchable = "touchable";

    /// <summary>Grupo de controles navegables por teclado/mando.</summary>
    public const string GroupUiFocusable = "ui_focusable";

    /// <summary>Nombre del autoload que gestiona las escenas.</summary>
    public const string AutoloadSceneManager = "SceneManager";

    /// <summary>Tamaño mínimo táctil recomendado en Android (px lógicos).</summary>
    public static readonly Vector2 MinTouchSize = new(56, 56);
}
