using System.Collections.Generic;
using System.Text;
using Godot;

/// <summary>
/// HUD provisional de la Sopa de Letras (SRP · plan Fase 3): contador,
/// dificultad, footer de palabras y aviso de victoria. `WordSearchMain` se
/// limita a llamar a <see cref="Render"/> cuando cambia la partida.
/// El formato del footer es puro (<see cref="BuildWordList"/>) y se prueba
/// sin escenas.
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
    public void Render(WordSearchGame game)
    {
        _counter.Text = $"{game.FoundCount}/{game.TotalCount}";
        _wordList.Text = BuildWordList(game.Order, game.PendingWords, game.FoundWords);
        _victory.Visible = game.IsComplete;
    }

    /// <summary>
    /// Texto del footer: primero las pendientes en el orden del tablero y al
    /// final las encontradas, tachadas y en el color de «encontrada».
    /// </summary>
    public static string BuildWordList(
        IReadOnlyList<string> order,
        IReadOnlySet<string> pending,
        IReadOnlyList<string> found)
    {
        var text = new StringBuilder();
        foreach (string word in order)
        {
            if (pending.Contains(word))
            {
                text.AppendLine(word);
            }
        }

        string color = PixelStyles.EncontradaBorde.ToHtml(false);
        foreach (string word in found)
        {
            text.AppendLine($"[color=#{color}][s]{word}[/s][/color]");
        }

        return text.ToString().TrimEnd();
    }
}
