using System.Collections.Generic;
using System.Text;
using Godot;

/// <summary>
/// HUD de la Sopa de Letras (solo presentación, plan Fase 3/5):
/// contador, dificultad, footer de palabras y aviso de victoria.
/// `WordSearchMain` le pasa los datos; no conoce reglas.
/// </summary>
public sealed class WordSearchHud
{
    private readonly Label _counter;
    private readonly Label _difficulty;
    private readonly RichTextLabel _wordList;
    private readonly Label _victory;

    public WordSearchHud(Label counter, Label difficulty, RichTextLabel wordList, Label victory)
    {
        _counter = counter;
        _difficulty = difficulty;
        _wordList = wordList;
        _victory = victory;
    }

    /// <summary>Fija la etiqueta de dificultad de la partida actual.</summary>
    public void Bind(DifficultyConfig config)
    {
        if (_difficulty == null)
        {
            return;
        }

        if (config == null)
        {
            _difficulty.Text = string.Empty;
            return;
        }

        int level = DifficultyCatalog.LevelNumberFor(config.Id);
        _difficulty.Text = $"{config.DisplayName.ToUpperInvariant()} - NIVEL {level}";
    }

    /// <summary>Refresca contador, lista y aviso de victoria desde el estado.</summary>
    public void Render(int found, int total, ICollection<string> pending, IReadOnlyList<string> foundOrder, bool complete)
    {
        if (_counter != null) _counter.Text = $"{found}/{total} Palabras";
        if (_wordList != null) _wordList.Text = BuildWordList(pending, foundOrder);
        if (_victory != null) _victory.Visible = complete;
    }

    /// <summary>
    /// Texto del footer: primero las pendientes en el orden del tablero y al
    /// final las encontradas, tachadas y en el color de «encontrada».
    /// Flujo continuo separado por barras, sin saltos de línea.
    /// </summary>
    public static string BuildWordList(ICollection<string> pending, IReadOnlyList<string> found)
    {
        var text = new StringBuilder();
        foreach (string word in pending)
        {
            AppendSeparator(text);
            text.Append(word);
        }

        string color = PixelStyles.EncontradaBorde.ToHtml(false);
        foreach (string word in found)
        {
            AppendSeparator(text);
            text.Append("[color=#");
            text.Append(color);
            text.Append("][s]");
            text.Append(word);
            text.Append("[/s][/color]");
        }

        return text.ToString();
    }

    private static void AppendSeparator(StringBuilder text)
    {
        if (text.Length > 0)
        {
            text.Append(" | ");
        }
    }
}
