using System.Collections.Generic;
using Godot;

/// <summary>
/// Catálogo de los cuatro niveles de dificultad (plan Fase 2 §2.1).
/// Recursos <c>.tres</c> editables desde el editor, en el orden de la spec §4.2.
/// </summary>
public static class DifficultyCatalog
{
    /// <summary>Nivel abierto por defecto (C6: el Fácil no tiene candado).</summary>
    public const string DefaultId = "facil";

    /// <summary>Rutas en el orden de la spec §4.2 (id = nombre del archivo).</summary>
    public static readonly string[] Paths =
    {
        "res://resources/difficulties/facil.tres",
        "res://resources/difficulties/medio.tres",
        "res://resources/difficulties/dificil.tres",
        "res://resources/difficulties/maestro.tres",
    };

    /// <summary>
    /// Carga los cuatro niveles. Un recurso ausente o inválido se salta con
    /// error en consola para no romper la pantalla de selección.
    /// </summary>
    public static List<DifficultyConfig> LoadAll()
    {
        var levels = new List<DifficultyConfig>();
        foreach (string path in Paths)
        {
            if (!ResourceLoader.Exists(path))
            {
                GD.PushError($"[DifficultyCatalog] falta el recurso {path}");
                continue;
            }

            var level = GD.Load<DifficultyConfig>(path);
            if (level == null)
            {
                GD.PushError($"[DifficultyCatalog] {path} no es un DifficultyConfig");
                continue;
            }

            levels.Add(level);
        }

        return levels;
    }

    /// <summary>Carga un nivel por id; null si no existe.</summary>
    public static DifficultyConfig LoadById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        foreach (DifficultyConfig level in LoadAll())
        {
            if (level.Id == id)
            {
                return level;
            }
        }

        return null;
    }

    /// <summary>Primer nivel del catálogo (Fácil) o null si no hay recursos.</summary>
    public static DifficultyConfig First()
    {
        List<DifficultyConfig> levels = LoadAll();
        return levels.Count > 0 ? levels[0] : null;
    }
}
