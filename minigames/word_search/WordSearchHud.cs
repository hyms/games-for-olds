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
        _difficulty.Text = config != null ? config.SummaryLabel : string.Empty;
    }

    /// <summary>Refresca contador, lista y aviso de victoria desde el estado.</summary>
    public void Render(int found, int total, ICollection<string> pending, IReadOnlyList<string> foundOrder, bool complete)
    {
        _counter.Text = $"{found}/{total}";
        _wordList.Text = BuildWordList(pending, foundOrder);
        _victory.Visible = complete;
    }

    /// <summary>
    /// Texto del footer: primero las pendientes en el orden del tablero y al
    /// final las encontradas, tachadas y en el color de «encontrada».
    /// </summary>
    public static string BuildWordList(ICollection<string> pending, IReadOnlyList<string> found)
    {
        var text = new StringBuilder();
        foreach (string word in pending)
        {
            text.AppendLine(word);
        }

        string color = PixelStyles.EncontradaBorde.ToHtml(false);
        foreach (string word in found)
        {
            text.AppendLine($"[color=#{color}][s]{word}[/s][/color]");
        }

        return text.ToString().TrimEnd();
    }
}