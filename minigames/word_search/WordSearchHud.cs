using System.Collections.Generic;
using System.Text;
using ClasicosMenteActiva.core;
using Godot;

/// <summary>
///     HUD de la Sopa de Letras (solo presentación, plan Fase 3/5):
///     contador, dificultad y aviso de victoria. La lista de palabras vive en
///     <see cref="WordsPanel" />; esta clase solo aporta el formateo compartido
///     (<see cref="BuildWordList" />). `WordSearchMain` le pasa los datos; no
///     conoce reglas.
/// </summary>
public sealed class WordSearchHud
{
    private readonly Label _counter;
    private readonly Label _difficulty;
    private readonly Label _victory;

    public WordSearchHud(Label counter, Label difficulty, Label victory)
    {
        _counter = counter;
        _difficulty = difficulty;
        _victory = victory;
    }

    /// <summary>Fija la etiqueta de dificultad de la partida actual.</summary>
    public void Bind(DifficultyConfig config)
    {
        if (_difficulty == null) return;

        if (config == null)
        {
            _difficulty.Text = string.Empty;
            return;
        }

        var level = DifficultyCatalog.LevelNumberFor(config.Id);
        _difficulty.Text = $"{config.DisplayName.ToUpperInvariant()} · NIVEL {level}";
    }

    /// <summary>Refresca contador y aviso de victoria desde el estado.</summary>
    public void Render(int found, int total, bool complete)
    {
        if (_counter != null) _counter.Text = $"{found}/{total} Palabras";
        if (_victory != null) _victory.Visible = complete;
    }

    /// <summary>
    ///     Texto del footer: primero las pendientes en el orden del tablero y al
    ///     final las encontradas, tachadas y en el color de «encontrada».
    ///     Flujo continuo separado por barras, sin saltos de línea.
    /// </summary>
    public static string BuildWordList(ICollection<string> pending, IReadOnlyList<string> found)
    {
        return BuildWordList(pending, found, null);
    }

    /// <summary>
    ///     Texto del footer con colores de tema: primero las pendientes y al
    ///     final las encontradas, tachadas (~~TEXTO~~) con el color del tema.
    /// </summary>
    public static string BuildWordList(ICollection<string> pending, IReadOnlyList<string> found, ThemeData theme)
    {
        var text = new StringBuilder();
        foreach (var word in pending)
        {
            if (text.Length > 0) text.Append(" | ");
            text.Append(word);
        }

        var strikeColor = theme?.FooterStrikethroughColor ?? PixelStyles.EncontradaBorde;
        var color = strikeColor.ToHtml(false);
        foreach (var word in found)
        {
            if (text.Length > 0) text.Append(" | ");
            text.Append("[color=#");
            text.Append(color);
            text.Append("][s]");
            text.Append(word);
            text.Append("[/s][/color]");
        }

        return text.ToString();
    }

}