using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Baraja la lista en el sitio (Fisher–Yates). Fuente única del algoritmo
    /// para el generador y la selección de palabras.
    /// </summary>
    public static void Shuffle<T>(IList<T> items, RandomNumberGenerator rng)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(rng);

        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = rng.RandiRange(0, i);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    /// <summary>
    /// RNG de Godot con la semilla indicada; <paramref name="seed"/> &lt; 0 usa
    /// una semilla aleatoria. Evita repetir el mismo arranque en cada sistema.
    /// </summary>
    public static RandomNumberGenerator CreateRng(long seed = -1)
    {
        var rng = new RandomNumberGenerator();
        Seed(rng, seed);
        return rng;
    }

    /// <summary>Aplica la semilla (o aleatoriza si es negativa) a un RNG existente.</summary>
    public static void Seed(RandomNumberGenerator rng, long seed)
    {
        ArgumentNullException.ThrowIfNull(rng);

        if (seed >= 0)
        {
            rng.Seed = (ulong)seed;
        }
        else
        {
            rng.Randomize();
        }
    }
}
