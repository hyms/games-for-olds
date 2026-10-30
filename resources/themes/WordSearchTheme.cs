using System.Collections.Generic;
using Godot;

/// <summary>
/// Categoría del banco de palabras de la Sopa de Letras (spec §5 y §5.1).
/// Un archivo <c>.tres</c> por grupo temático, cargado por <see cref="WordBank"/>.
/// Evolución del modelo original de "tema" (decisión C9): solo describe el
/// grupo temático y su banco de palabras. La dificultad (tablero, objetivo,
/// direcciones) vive en <see cref="DifficultyConfig"/>: una sola fuente.
/// Equivalente C# de <c>resources/themes/WordSearchTheme.gd</c>.
/// </summary>
[GlobalClass]
public partial class WordSearchTheme : Resource
{
    /// <summary>Identificador estable del grupo temático (= nombre del archivo .tres).</summary>
    [Export]
    public string CategoryId { get; set; } = string.Empty;

    /// <summary>Nombre visible para el jugador (spec §5.1).</summary>
    [Export]
    public string ThemeName { get; set; } = "General";

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = string.Empty;

    /// <summary>Lista de palabras del grupo, ya normalizadas (A–Z, sin tildes ni Ñ).</summary>
    [Export]
    public Godot.Collections.Array<string> Words { get; set; } = new();

    /// <summary>Mínimo de palabras por categoría (spec §5).</summary>
    public const int MinWords = 60;

    /// <summary>Longitud mínima admitida de una palabra.</summary>
    public const int MinBankWordLength = 4;

    /// <summary>Longitud máxima admitida (tablero 15×15, decisión C3).</summary>
    public const int MaxWordLength = 15;

    private static readonly Dictionary<char, char> AccentReplacements = new()
    {
        ['Á'] = 'A', ['É'] = 'E', ['Í'] = 'I', ['Ó'] = 'O', ['Ú'] = 'U',
        ['Ü'] = 'U', ['Ñ'] = 'N',
    };

    /// <summary>
    /// Normaliza una palabra a mayúsculas A–Z sin tildes, diéresis ni Ñ.
    /// Elimina espacios y cualquier carácter no alfabético (ESPAÑA → ESPANA).
    /// </summary>
    public static string NormalizeWord(string word)
    {
        string upper = (word ?? string.Empty).Trim().ToUpperInvariant();
        var cleaned = new System.Text.StringBuilder(upper.Length);
        foreach (char c in upper)
        {
            char baseChar = AccentReplacements.TryGetValue(c, out char replacement) ? replacement : c;
            if (baseChar is >= 'A' and <= 'Z')
            {
                cleaned.Append(baseChar);
            }
        }

        return cleaned.ToString();
    }

    /// <summary>
    /// Comprueba la categoría contra las reglas del banco (spec §5).
    /// Devuelve la lista de errores; vacío = categoría válida.
    /// </summary>
    public string[] Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(CategoryId))
        {
            errors.Add("falta category_id");
        }

        if (string.IsNullOrWhiteSpace(ThemeName))
        {
            errors.Add("falta theme_name");
        }

        if (Words.Count < MinWords)
        {
            errors.Add($"solo {Words.Count} palabras, mínimo {MinWords}");
        }

        var seen = new HashSet<string>();
        foreach (string word in Words)
        {
            string normalized = NormalizeWord(word);
            if (normalized.Length == 0)
            {
                errors.Add($"palabra sin letras válidas: '{word}'");
                continue;
            }

            if (normalized != word)
            {
                errors.Add($"palabra sin normalizar: '{word}' (debe ir como {normalized})");
            }

            if (normalized.Length < MinBankWordLength || normalized.Length > MaxWordLength)
            {
                errors.Add($"longitud {normalized.Length} fuera de {MinBankWordLength}–{MaxWordLength}: '{word}'");
            }

            if (!seen.Add(normalized))
            {
                errors.Add($"palabra duplicada: '{normalized}'");
            }
        }

        if (seen.Count == 0)
        {
            errors.Add("categoría vacía tras el filtro");
        }

        return errors.ToArray();
    }
}
