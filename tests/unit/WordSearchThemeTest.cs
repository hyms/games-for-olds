using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Primer test puro (Fase 0.5): normalización y filtros de <see cref="WordSearchTheme"/>.
/// No usa escenas: solo la lógica del modelo de categoría.
/// Equivalente C# de <c>tests/unit/WordSearchThemeTest.gd</c>.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSearchThemeTest
{
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private static WordSearchTheme MakeTheme(string[] words, int gridSize = 8, int minWordLength = 4)
    {
        var theme = new WordSearchTheme
        {
            GridSize = gridSize,
            MinWordLength = minWordLength,
        };
        theme.Words.AddRange(words);
        return theme;
    }

    [TestCase]
    public void NormalizaAMayusculasYQuitaEspacios()
    {
        WordSearchTheme theme = MakeTheme(new[] { "casa", "  mesa  ", "LUNA" });
        AssertThat(theme.GetFilteredWords()).ContainsExactly("CASA", "MESA", "LUNA");
    }

    [TestCase]
    public void QuitaTildesDieresisYEnye()
    {
        WordSearchTheme theme = MakeTheme(new[] { "ESPAÑA", "pingüino", "jardín" });
        AssertThat(theme.GetFilteredWords()).ContainsExactly("ESPANA", "PINGUINO", "JARDIN");
    }

    [TestCase]
    public void DescartaPalabrasMasCortasQueElMinimo()
    {
        WordSearchTheme theme = MakeTheme(new[] { "sol", "luna", "mar", "mesa" });
        AssertThat(theme.GetFilteredWords()).ContainsExactly("LUNA", "MESA");
    }

    [TestCase]
    public void DescartaPalabrasMasLargasQueElTablero()
    {
        WordSearchTheme theme = MakeTheme(new[] { "mesa", "refrigerador" }, 8);
        AssertThat(theme.GetFilteredWords()).ContainsExactly("MESA");
    }

    [TestCase]
    public void DescartaPalabrasSinLetrasValidas()
    {
        WordSearchTheme theme = MakeTheme(new[] { "1234", "m3s4", "casa" });
        AssertThat(theme.GetFilteredWords()).ContainsExactly("CASA");
    }

    [TestCase]
    public void ElFiltroNoModificaLaListaOriginal()
    {
        WordSearchTheme theme = MakeTheme(new[] { "sol", "mesa" });
        _ = theme.GetFilteredWords();
        AssertThat(theme.Words).ContainsExactly("sol", "mesa");
    }

    // --- Fase 1: Validate() del banco (spec §5) ------------------------------

    /// <summary>Categoría de prueba válida: identificador, nombre y palabras únicas.</summary>
    private static WordSearchTheme MakeCategory(string[] words)
    {
        WordSearchTheme category = MakeTheme(words, 15, 4);
        category.CategoryId = "prueba";
        category.ThemeName = "Prueba";
        return category;
    }

    /// <summary><paramref name="count"/> palabras únicas de 6 letras A–Z (AACASA, ABCASA, …).</summary>
    private static string[] ValidWords(int count)
    {
        var words = new string[count];
        for (int i = 0; i < count; i++)
        {
            words[i] = $"{Letters[i / 26]}{Letters[i % 26]}CASA";
        }

        return words;
    }

    [TestCase]
    public void ValidateAceptaUnaCategoriaCompleta()
    {
        WordSearchTheme category = MakeCategory(ValidWords(60));
        string[] errors = category.Validate();
        AssertBool(errors.Length == 0)
            .OverrideFailureMessage($"Validate() = {string.Join(", ", errors)}")
            .IsTrue();
    }

    [TestCase]
    public void ValidateDetectaDuplicadosLongitudYPalabrasSinNormalizar()
    {
        var words = new System.Collections.Generic.List<string>(ValidWords(60));
        words.Add(words[0]); // duplicada
        words.Add("ABC"); // demasiado corta
        words.Add("MESAÑ"); // con Ñ: debe ir como MESAN
        string[] errors = MakeCategory(words.ToArray()).Validate();
        AssertInt(errors.Length).IsEqual(3);
    }

    [TestCase]
    public void ValidateMarcaComoInvalidaUnaCategoriaVacia()
    {
        string[] errors = MakeCategory(System.Array.Empty<string>()).Validate();
        // sin palabras: mínimo 60 incumplido + categoría vacía tras el filtro
        AssertInt(errors.Length).IsGreaterEqual(2);
    }
}
