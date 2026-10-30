using Godot;

/// <summary>
/// Utilidades de geometría y texto compartidas.
/// Equivalente C# de <c>core/Utils.gd</c>.
/// </summary>
public static class Utils
{
    /// <summary>
    /// Amplía el rectángulo global de un control para dar margen táctil.
    /// </summary>
    public static Rect2 GetTouchRect(Control control, float margin = 8.0f)
    {
        if (control == null || !GodotObject.IsInstanceValid(control))
        {
            return new Rect2();
        }

        return control.GetGlobalRect().Grow(margin);
    }

    /// <summary>Nombre legible de una ruta de escena ("ui/main_menu/MainMenu.tscn" → "Main Menu").</summary>
    public static string SceneNameFromPath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        string fileName = path.GetFile().GetBaseName();
        return fileName.Replace("_", " ").Capitalize();
    }
}
