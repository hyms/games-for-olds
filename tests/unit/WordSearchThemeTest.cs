using GdUnit4;
using Godot;
using static GdUnit4.Assertions;

/// <summary>
/// Modelo del banco de palabras: normalización y <c>Validate()</c> (spec §5).
/// El filtrado por longitud y la dificultad viven en <c>DifficultyConfig</c> y
/// <c>WordSearchGenerator.PrepareWords</c>; aquí solo se prueba el banco.
/// </summary>
[TestSuite]
[RequireGodotRuntime]
public class WordSearchThemeTest
{
    private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    [TestCase]
    public void NormalizaAMayusculasYQuitaEspacios()
    {
        AssertString(WordSearchTheme.NormalizeWord("casa")).IsEqual("CASA");
        AssertString(WordSearchTheme.NormalizeWord("  mesa  ")).IsEqual("MESA");
        AssertString(WordSearchTheme.NormalizeWord("LUNA")).IsEqual("LUNA");
    }

    [TestCase]
    public void QuitaTildesDieresisYEnye()
    {
        AssertString(WordSearchTheme.NormalizeWord("ESPAÑA")).IsEqual("ESPANA");
        AssertString(WordSearchTheme.NormalizeWord("pingüino")).IsEqual("PINGUINO");
        AssertString(WordSearchTheme.NormalizeWord("jardín")).IsEqual("JARDIN");
    }

    [TestCase]
    public void DescartaCaracteresQueNoSonLetras()
    {
        AssertString(WordSearchTheme.NormalizeWord("1234")).IsEqual(string.Empty);
        AssertString(WordSearchTheme.NormalizeWord("m3s4")).IsEqual("MS");
    }

    [TestCase]
    public void LaNormalizacionNoModificaLaListaOriginal()
    {
        var category = new WordSearchTheme();
        category.Words.AddRange(new[] { "sol", "mesa" });
        AssertString(WordSearchTheme.NormalizeWord(category.Words[0])).IsEqual("SOL");
        AssertThat(category.Words).ContainsExactly("sol", "mesa");
    }

    // --- Fase 1: Validate() del banco (spec §5) ------------------------------

    /// <summary>Categoría de prueba válida: identificador, nombre y palabras únicas.</summary>
    private static WordSearchTheme MakeCategory(string[] words)
    {
        var category = new WordSearchTheme
        {
            CategoryId = "prueba",
            ThemeName = "Prueba",
        };
        category.Words.AddRange(words);
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
