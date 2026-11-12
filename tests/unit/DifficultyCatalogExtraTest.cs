using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class DifficultyCatalogExtraTest
{
    [TestCase]
    public void LoadByIdLevelNumberYCache()
    {
        DifficultyCatalog.InvalidateCache();
        AssertObject(DifficultyCatalog.LoadById(null)).IsNull();
        AssertObject(DifficultyCatalog.LoadById("")).IsNull();
        AssertObject(DifficultyCatalog.LoadById("  ")).IsNull();
        AssertObject(DifficultyCatalog.LoadById("nope")).IsNull();
        var facil = DifficultyCatalog.LoadById("facil");
        AssertObject(facil).IsNotNull();
        AssertString(facil.Id).IsEqual("facil");

        AssertInt(DifficultyCatalog.LevelNumberFor("facil")).IsEqual(1);
        AssertInt(DifficultyCatalog.LevelNumberFor("medio")).IsEqual(2);
        AssertInt(DifficultyCatalog.LevelNumberFor("dificil")).IsEqual(3);
        AssertInt(DifficultyCatalog.LevelNumberFor("maestro")).IsEqual(4);
        AssertInt(DifficultyCatalog.LevelNumberFor("inexistente")).IsEqual(1);

        var first = DifficultyCatalog.First();
        var first2 = DifficultyCatalog.First();
        AssertObject(first).IsNotNull();
        AssertBool(ReferenceEquals(first, first2)).IsTrue();
        DifficultyCatalog.InvalidateCache();
        var reloaded = DifficultyCatalog.First();
        AssertObject(reloaded).IsNotNull();
        AssertString(reloaded.Id).IsEqual("facil");
    }

    [TestCase]
    public void LabelsYResumenesSonCoherentes()
    {
        var d = DifficultyCatalog.LoadById("dificil");
        AssertString(d.BoardLabel).IsEqual("12 × 12");
        AssertBool(d.SummaryLabel.Contains("Difícil")).IsTrue();
        AssertBool(d.MenuLabel.Contains("12 × 12")).IsTrue();
        AssertBool(d.MenuLabel.Contains("12 palabras")).IsTrue();
        AssertBool(d.AllowsDirection(Godot.Vector2I.Zero)).IsFalse();
    }
}
