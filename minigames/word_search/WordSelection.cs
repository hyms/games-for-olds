using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Grupos temáticos y palabras de una partida (plan Fase 2 §2.3 · spec §5).
/// El jugador no elige categoría: la <see cref="ShuffleBagSystem"/> reparte
/// <see cref="DifficultyConfig.CategoryCount"/> grupos y de ellos se sortean
/// las palabras del objetivo.
/// </summary>
public sealed class WordSelectionResult
{
    public WordSelectionResult(IReadOnlyList<string> words, IReadOnlyList<string> categoryIds)
    {
        Words = words;
        CategoryIds = categoryIds;
    }

    /// <summary>Palabras de la partida, ya normalizadas y barajadas.</summary>
    public IReadOnlyList<string> Words { get; }

    /// <summary>Ids de los grupos temáticos usados, en el orden en que salieron.</summary>
    public IReadOnlyList<string> CategoryIds { get; }
}

/// <summary>
/// Selección de palabras de una partida: mezcla de categorías según la
/// dificultad y barajado de palabras (plan Fase 2 §2.2).
/// </summary>
public static class WordSelection
{
    /// <summary>Máximo de extracciones de la bolsa para conseguir grupos distintos.</summary>
    private const int MaxBagDraws = 80;

    /// <summary>
    /// Elige las palabras de una partida. Si <paramref name="bag"/> es null o
    /// no da grupos suficientes, se completan en el orden del catálogo.
    /// <paramref name="seed"/> ≥ 0 fija la semilla del barajado (tests).
    /// </summary>
    public static WordSelectionResult Pick(
        DifficultyConfig config,
        IReadOnlyList<WordSearchTheme> categories,
        ShuffleBagSystem bag = null,
        long seed = -1)
    {
        ArgumentNullException.ThrowIfNull(config);

        var byId = new Dictionary<string, WordSearchTheme>();
        var ordered = new List<WordSearchTheme>();
        if (categories != null)
        {
            foreach (WordSearchTheme category in categories)
            {
                if (category == null || string.IsNullOrEmpty(category.CategoryId) || byId.ContainsKey(category.CategoryId))
                {
                    continue;
                }

                byId[category.CategoryId] = category;
                ordered.Add(category);
            }
        }

        int wanted = Mathf.Clamp(config.CategoryCount, 1, Mathf.Max(1, byId.Count));
        var chosen = new List<string>();

        if (bag != null)
        {
            for (int draw = 0; draw < MaxBagDraws && chosen.Count < wanted; draw++)
            {
                string id = bag.NextGroup();
                if (!string.IsNullOrEmpty(id) && byId.ContainsKey(id) && !chosen.Contains(id))
                {
                    chosen.Add(id);
                }
            }
        }

        foreach (WordSearchTheme category in ordered)
        {
            if (chosen.Count >= wanted)
            {
                break;
            }

            if (!chosen.Contains(category.CategoryId))
            {
                chosen.Add(category.CategoryId);
            }
        }

        var pool = new List<string>();
        var seen = new HashSet<string>();
        foreach (string id in chosen)
        {
            foreach (string word in WordSearchGenerator.PrepareWords(byId[id].Words, config.GridSize))
            {
                if (seen.Add(word))
                {
                    pool.Add(word);
                }
            }
        }

        Utils.Shuffle(pool, Utils.CreateRng(seed));

        int take = Mathf.Min(Mathf.Max(1, config.TargetWords), pool.Count);
        List<string> words = pool.GetRange(0, take);
        return new WordSelectionResult(words, chosen);
    }
}
