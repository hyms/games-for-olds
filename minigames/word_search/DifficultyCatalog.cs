using System;
using System.Collections.Generic;
using ClasicosMenteActiva.core;
using Godot;

/// <summary>
/// Catálogo de los cuatro niveles de dificultad (plan Fase 2 §2.1).
/// Recursos <c>.tres</c> editables desde el editor, en el orden de la spec §4.2.
/// </summary>
public static class DifficultyCatalog
{
	/// <summary>Nivel abierto por defecto (C6: el Fácil no tiene candado).</summary>
	public const string DefaultId = Constants.DefaultDifficultyId;

	/// <summary>Rutas en el orden de la spec §4.2 (id = nombre del archivo).</summary>
	public static readonly string[] Paths =
	{
		"res://resources/difficulties/facil.tres",
		"res://resources/difficulties/medio.tres",
		"res://resources/difficulties/dificil.tres",
		"res://resources/difficulties/maestro.tres",
	};

	/// <summary>Caché de niveles cargados (evita recargar .tres en cada llamada).</summary>
	private static List<DifficultyConfig> _levelsCache;

	/// <summary>
	/// Carga los cuatro niveles. Se carga una sola vez y se reutiliza el caché.
	/// Un recurso ausente o inválido se salta con error en consola.
	/// </summary>
	public static List<DifficultyConfig> LoadAll()
	{
		if (_levelsCache != null)
		{
			return _levelsCache;
		}

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

		_levelsCache = levels;
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

	/// <summary>Número visible del nivel (1 = Fácil, 2 = Medio, …).</summary>
	public static int LevelNumberFor(string id)
	{
		List<DifficultyConfig> levels = LoadAll();
		for (int i = 0; i < levels.Count; i++)
		{
			if (string.Equals(levels[i].Id, id, StringComparison.Ordinal))
			{
				return i + 1;
			}
		}

		return 1;
	}

	/// <summary>Invalida el caché (útil para tests o recargas de recursos).</summary>
	public static void InvalidateCache() => _levelsCache = null;
}
