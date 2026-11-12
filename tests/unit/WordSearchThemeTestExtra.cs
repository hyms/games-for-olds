using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class WordSearchThemeTestExtra
{
    [TestCase]
    public void NormalizeWordManejaAcentosYCaracteresEspeciales()
    {
        AssertString(WordSearchTheme.NormalizeWord(null)).IsEmpty();
        AssertString(WordSearchTheme.NormalizeWord("  árbol  ")).IsEqual("ARBOL");
        AssertString(WordSearchTheme.NormalizeWord("pingüino")).IsEqual("PINGUINO");
        AssertString(WordSearchTheme.NormalizeWord("niño")).IsEqual("NINO");
        AssertString(WordSearchTheme.NormalizeWord("ca-fe!")).IsEqual("CAFE");
        AssertString(WordSearchTheme.NormalizeWord("123")).IsEmpty();
    }

    [TestCase]
    public void ValidateDetectaErroresComunes()
    {
        var theme = new WordSearchTheme { CategoryId = "", ThemeName = "" };
        var errors = theme.Validate();
        AssertBool(errors.Length > 0).IsTrue();

        var ok = WordBank.LoadById("animales");
        AssertObject(ok).IsNotNull();
        AssertInt(ok.Validate().Length).IsEqual(0);
    }
}
