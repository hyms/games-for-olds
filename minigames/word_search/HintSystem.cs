using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
///     Sistema de pistas para la Sopa de Letras (Fase 6).
///     Cada dificultad tiene 3 pistas fijas y adicionales cuando se acaba el panel.
///     El jugador puede solicitar pistas sin penalización (MVP: no-op, sin anuncios).
/// </summary>
public static class HintSystem
{
    /// <summary>Pistas por nivel. El orden refleja el nivel de dificultad.</summary>
    private static readonly Dictionary<string, string[]> HintsByLevel = new()
    {
        {
            "facil", new[]
            {
                "Empieza con las letras de la palabra en el tablero.",
                "Busca una palabra corta (4-6 letras) en horizontal o vertical.",
                "La primera pista suele aparecer doble por la izquierda o arriba."
            }
        },
        {
            "medio", new[]
            {
                "Cada palabra tiene al menos dos letras en común con su categoria.",
                "Busca una palabra de 7-8 letras en diagonal o horizontal.",
                "La pista puede estar invertida (de derecha a izquierda)."
            }
        },
        {
            "dificil", new[]
            {
                "Une dos palabras que comparten una letra clave.",
                "Una palabra sale de la esquina opuesta a la actual.",
                "La pista suele seguir un camino que no es recto (diagonal)."
            }
        },
        {
            "maestro", new[]
            {
                "Las palabras mas complejas suelen doblarse dentro del tablero.",
                "Será mas facil si buscas palabras de 10-12 letras.",
                "Recuerda que el tablero puede tener espacios vacios o sobrerellenos."
            }
        }
    };

    /// <summary>Obtiene la pista N para un nivel.</summary>
    public static string GetHint(string levelId, int hintIndex)
    {
        if (string.IsNullOrEmpty(levelId)) return "Sin pista disponible.";
        var hints = HintsByLevel.GetValueOrDefault(levelId);
        if (hints == null || hintIndex < 0 || hintIndex >= hints.Length) return "Pista no disponible en este nivel.";
        return hints[hintIndex];
    }

    /// <summary>Número de pistas fijas disponibles por nivel.</summary>
    public static int GetHintCount(string levelId)
    {
        return HintsByLevel.GetValueOrDefault(levelId)?.Length ?? 0;
    }

    /// <summary>Siempre devuelve 3 pistas (independiente del nivel).</summary>
    public static string[] GetHints(string levelId)
    {
        return GetHintsInternal(levelId, 3);
    }

    private static string[] GetHintsInternal(string levelId, int count)
    {
        var all = HintsByLevel.GetValueOrDefault(levelId);
        if (all == null || all.Length == 0) return new[] { "Sin pistas disponibles." };
        var take = Mathf.Clamp(count, 1, all.Length);
        return all.Take(take).ToArray();
    }
}