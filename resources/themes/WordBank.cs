using System.Collections.Generic;
using Godot;

/// <summary>
/// Catálogo de las 10 categorías del banco de palabras (spec §5.1).
/// Recursos <c>.tres</c> incluidos en la app (decisión C9): funcionan sin
/// conexión y se editan desde el editor. Rutas explícitas para que el orden
/// de la spec sea estable también dentro de una exportación.
/// Equivalente C# de <c>resources/themes/WordBank.gd</c>.
/// </summary>
public static class WordBank
{
    /// <summary>Rutas en el orden de la spec §5.1 (id = nombre del archivo).</summary>
    public static readonly string[] CategoryPaths =
    {
        "res://resources/themes/themes/animales.tres",
        "res://resources/themes/themes/plantas.tres",
        "res://resources/themes/themes/frutas_verduras.tres",
        "res://resources/themes/themes/objetos.tres",
        "res://resources/themes/themes/profesiones.tres",
        "res://resources/themes/themes/paises_ciudades.tres",
        "res://resources/themes/themes/comidas.tres",
        "res://resources/themes/themes/instrumentos.tres",
        "res://resources/themes/themes/herramientas.tres",
        "res://resources/themes/themes/ropa.tres",
    };

    /// <summary>
    /// Carga las 10 categorías en el orden de la spec. Los recursos ausentes o
    /// inválidos se saltan con error en consola (no rompen la partida).
    /// </summary>
    public static List<WordSearchTheme> LoadAll()
    {
        var categories = new List<WordSearchTheme>();
        foreach (string path in CategoryPaths)
        {
            if (!ResourceLoader.Exists(path))
            {
                GD.PushError($"[WordBank] falta el recurso {path}");
                continue;
            }

            var category = GD.Load<WordSearchTheme>(path);
            if (category == null)
            {
                GD.PushError($"[WordBank] {path} no es una WordSearchTheme");
                continue;
            }

            categories.Add(category);
        }

        return categories;
    }

    /// <summary>Carga una categoría por identificador; null si no existe.</summary>
    public static WordSearchTheme LoadById(string categoryId)
    {
        foreach (WordSearchTheme category in LoadAll())
        {
            if (category.CategoryId == categoryId)
            {
                return category;
            }
        }

        return null;
    }

    /// <summary>Identificadores de las categorías cargadas, en orden de la spec.</summary>
    public static List<string> Ids()
    {
        var ids = new List<string>();
        foreach (WordSearchTheme category in LoadAll())
        {
            ids.Add(category.CategoryId);
        }

        return ids;
    }
}
